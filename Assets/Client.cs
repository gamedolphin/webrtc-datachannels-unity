using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RTC
{
    public class Client : IDisposable
    {

        private WS websocket;

        private Dictionary<string,PeerConnection> connections = new Dictionary<string,PeerConnection>();
        private Dictionary<string,List<Tuple<string,string>>> iceCache = new Dictionary<string,List<Tuple<string,string>>>();

        private string id;
        private ILog logger;
        private bool wsConnected = false;

        public event GenericCallback OnReady;
        public event GenericCallback OnShutdown;
        public event MessageStringCallback OnConnected;

        public Client(string _id, ILog _logger)
        {
            // remove this later
            Logger.log = new UnityLogger();

            id = _id;
            logger = _logger;
        }

        public void JoinRoom(string roomId)
        {
            if (wsConnected)
            {
                var msg = MessageUtil.GetMessage(MessageType.JoinRequest, new JoinRequestData{ RoomId = roomId });
                logger.Log("Joining room "+msg);
                websocket.SendMessage(msg);
            }
            else
            {
                logger.Log("Cannot join before ready");
            }

        }

        public void SendMessage(string msg)
        {
            foreach (var connection in connections)
            {
                connection.Value.SendMessage(msg);
            }
        }

        public void Connect(string url)
        {
            websocket = new WS($"ws://{url}/?id={id}", logger);

            websocket.OnOpenEvent += OnOpen;
            websocket.OnCloseEvent += OnClose;
            websocket.OnErrorEvent += (str) => logger.Log($"Websocket err :  {str}");
            websocket.OnMessageEvent += OnMessage;
        }

        public void Disconnect()
        {
            Dispose();
        }

        private void OnClose()
        {
            wsConnected = false;
            logger.Log("Disconnected from signalling server");
            OnShutdown?.Invoke();
        }

        private void OnOpen()
        {
            logger.Log("Connected to signalling server");
            wsConnected = true;
            OnReady?.Invoke();
        }

        private void OnMessage(string message)
        {
            try {
                var strType = message[0] + "";
                var byteType = int.Parse(strType);
                var type = (MessageType)byteType;
                var data = message.Substring(1);

                switch(type)
                {
                    case MessageType.JoinResponse:
                        OnJoinResponse(data);
                        break;
                    case MessageType.SendOffer:
                        ProcessOffer(data);
                        break;
                    case MessageType.SendAnswer:
                        ProcessAnswer(data);
                        break;
                    case MessageType.SendIce:
                        ProcessIce(data);
                        break;
                    default:
                        logger.Log("Unable to process message type "+type);
                        break;
                }
            }
            catch(Exception ex)
            {
                logger.Log(ex.Message);
            }
        }

        private async void OnJoinResponse(string data)
        {
            var responseData = MessageUtil.GetData<JoinResponseData>(data);
            var ids = responseData.OtherClients;
            logger.Log("Received total clients "+ids.Length);

            foreach(var otherId in ids)
            {
                await ConnectToID(otherId);
            }
        }

        private async Task<PeerConnection> GetPC(bool offer, string otherId)
        {
            var connection = new PeerConnection(offer, logger);
            connections.Add(otherId, connection);

            var sdpType = offer ? "offer" : "answer";
            var msgType = offer ? MessageType.SendOffer : MessageType.SendAnswer;

            connection.OnDescriptionEvent += (sdp, type) =>
            {
                logger.Log($"Sending {sdpType} to "+otherId);
                var data = new SendSdpData {
                    FromId = id,
                    ClientId = otherId,
                    Sdp = sdp
                };
                var msg = MessageUtil.GetMessage(msgType, data);
                websocket.SendMessage(msg);
            };

            connection.OnCandidateEvent += (cand, mid) => OnIceCandidate(otherId, cand, mid);
            connection.OnStateChangeEvent += state => logger.Log(state.ToString());
            connection.OnReadyEvent += () => OnConnected?.Invoke(otherId);

            if (iceCache.ContainsKey(otherId))
            {
                foreach(var ice in iceCache[otherId])
                {
                    await connection.AddRemoteCandidate(ice.Item1, ice.Item2);
                }
                iceCache.Remove(otherId);
            }

            return connection;
        }

        private async Task ConnectToID(string otherId)
        {
            var connection = await GetPC(true, otherId);

            await connection.Start();
        }

        private async void ProcessOffer(string data)
        {
            var offerData = MessageUtil.GetData<SendSdpData>(data);
            var otherId = offerData.FromId;

            var connection = await GetPC(false, otherId);

            await connection.SetRemoteDescription(offerData.Sdp, "offer");
        }

        private void OnIceCandidate(string otherId, string candidate, string mid)
        {
            logger.Log("Sending ice to "+otherId);
            var msg = MessageUtil.GetMessage(MessageType.SendIce, new SendIceData
                                             {
                                                 FromId = id,
                                                 ClientId = otherId,
                                                 Candidate = candidate,
                                                 Mid = mid
                                             });
            websocket.SendMessage(msg);
        }

        private async void ProcessAnswer(string data)
        {
            var answerData = MessageUtil.GetData<SendSdpData>(data);

            if(connections.ContainsKey(answerData.FromId))
            {
                var connection = connections[answerData.FromId];
                await connection.SetRemoteDescription(answerData.Sdp, "answer");
            }
        }

        private async void ProcessIce(string data)
        {
            var iceData = MessageUtil.GetData<SendIceData>(data);
            var fromId = iceData.FromId;
            if (connections.ContainsKey(fromId))
            {
                var pc = connections[fromId];
                await pc.AddRemoteCandidate(iceData.Candidate, iceData.Mid);
            }
            else
            {
                var ice = new Tuple<string,string>(iceData.Candidate, iceData.Mid);
                if (iceCache.ContainsKey(fromId))
                {
                    iceCache[fromId].Add(ice);
                }
                else
                {
                    iceCache[fromId] = new List<Tuple<string,string>> { ice };
                }
            }
        }

        public void Dispose()
        {
            websocket.Dispose();
            foreach(var pc in connections)
            {
                pc.Value.Dispose();
            }
            connections.Clear();
        }
    }
}
