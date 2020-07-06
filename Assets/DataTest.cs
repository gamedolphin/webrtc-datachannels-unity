using UnityEngine;

namespace RTC
{

    public class UnityLogger : ILog
    {
        public void Log(string str)
        {
            Debug.Log(str);
        }
    }

    public class DataTest : MonoBehaviour
    {
        private DataChannel rtc;
        private Websocket ws;

        [SerializeField]
        private string id;

        private void LogMessage(string msg)
        {
            Debug.Log($"[{id}] : {msg}");
        }

        // Start is called before the first frame update
        private void Start()
        {
            Logger.log = new UnityLogger();
            rtc = new DataChannel();
            ws = new Websocket();

            ws.OnConnect += () =>
            {
                var msg = MessageUtil.GetMessage(MessageType.JoinRequest, new JoinRequestData { RoomId = "first" });
                LogMessage("Connect cb, sending message "+msg);
                ws.SendMessage(msg);
            };
            ws.OnDisconnect += () => LogMessage("Called close cb");
            ws.OnError += (str) => LogMessage($"Error cb {str}");
            ws.OnMessage += OnMessage;
            ws.Connect($"ws://localhost:8080/?id={id}");
        }

        private void OnMessage(string message)
        {
            var strType = message[0] + "";
            var byteType = int.Parse(strType);
            var type = (MessageType)byteType;
            var data = message.Substring(1);
            switch(type)
            {
                case MessageType.JoinResponse:
                    OnJoinResponse(data);
                    break;
                default:
                    LogMessage("Unable to process message type "+type);
                    break;
            }
        }

        private void OnJoinResponse(string data)
        {
            var responseData = MessageUtil.GetData<JoinResponseData>(data);
            OnGetOtherClients(responseData.OtherClients);
        }

        private void OnGetOtherClients(string[] ids)
        {
            LogMessage("Received total clients "+ids.Length);
            for (int i = 0; i < ids.Length; ++i) {
                Debug.Log(ids[i]);
            }
        }


        private void OnDestroy()
        {
            LogMessage("Disposing...");
            ws.Dispose();
            rtc.Dispose();
        }
    }

}
