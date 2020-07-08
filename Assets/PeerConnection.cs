using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RTC
{
    public class PeerConnection : IDisposable
    {

        private int connectionId = -1;
        private int configId = -1;
        private int dataId = -1;
        private bool isOffer = false;

        private GCHandle gch;
        private bool disposed = false;

        public DescriptionDelegate OnDescriptionEvent;
        public CandidateDelegate OnCandidateEvent;
        public StateChangeDelegate OnStateChangeEvent;
        public GatherStateChangeDelegate OnGatherStateChangeEvent;
        public GenericCallback OnReadyEvent;
        public MessageStringCallback OnMessageEvent;

        private ILog logger;
        private bool descriptionSet = false;
        private List<Tuple<string,string>> iceCache = new List<Tuple<string,string>>();

        public PeerConnection(bool offer, ILog _logger)
        {
            logger = _logger;
            try
            {
                DataChannel.ReadyRTC();
                isOffer = offer;
                var iceServers = new string [] { "stun:stun.l.google.com:19302" };
                configId = CoreUtils.GetConfig(iceServers, out var config);
                connectionId = DataChannel.rtcCreatePeerConnection(ref config);

                gch = GCHandle.Alloc(this);
                IntPtr ptr = GCHandle.ToIntPtr(gch);

                DataChannel.rtcSetUserPointer(connectionId, ptr);
                DataChannel.rtcSetLocalDescriptionCallback(connectionId, InternalOnDescription);
                DataChannel.rtcSetLocalCandidateCallback(connectionId, InternalOnCandidate);
                DataChannel.rtcSetStateChangeCallback(connectionId, InternalOnState);
                DataChannel.rtcSetGatheringStateChangeCallback(connectionId, InternalOnGatheringState);
                DataChannel.rtcSetDataChannelCallback(connectionId, InternalOnDataChannel);
            }
            catch(Exception ex)
            {
                logger.Log(ex.Message);
            }
        }

        public async Task Start()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            if (isOffer)
            {
                dataId = DataChannel.rtcCreateDataChannel(connectionId, "test");
                DataChannel.rtcSetOpenCallback(dataId, InternalOnOpen);
                DataChannel.rtcSetClosedCallback(dataId, InternalOnClose);
                DataChannel.rtcSetMessageCallback(dataId, InternalOnMessage);
            }
        }

        ~PeerConnection()
        {
            if(!disposed)
            {
                Dispose();
            }
        }

        public void SendMessage(string msg)
        {
            if(dataId > -1)
            {
                DataChannel.rtcSendMessage(dataId, msg, msg.Length);
            }
        }

        public async Task SetRemoteDescription(string sdp, string type)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            DataChannel.rtcSetRemoteDescription(connectionId, sdp, type);
            await Task.Delay(TimeSpan.FromSeconds(1));
            iceCache.ForEach(ice =>
            {
                DataChannel.rtcAddRemoteCandidate(connectionId, ice.Item1, ice.Item2);
            });
            iceCache.Clear();
            descriptionSet = true;
        }

        public async Task AddRemoteCandidate(string candidate, string mid)
        {
            if (descriptionSet)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                DataChannel.rtcAddRemoteCandidate(connectionId, candidate, mid);
            }
            else
            {
                iceCache.Add(new Tuple<string,string>(candidate, mid));
            }
        }

        public void OnCandidate(string cand, string mid)
        {
            OnCandidateEvent?.Invoke(cand, mid);
        }

        public void OnDescription(string sdp, string type)
        {
            OnDescriptionEvent?.Invoke(sdp, type);
        }

        public void OnOpen()
        {

        }

        public void OnClose()
        {

        }

        public void OnMessage(byte[] data)
        {
            var str = System.Text.Encoding.UTF8.GetString(data);
            OnMessageEvent?.Invoke(str);
        }

        public void OnStateChange(RTCState state)
        {
            OnStateChangeEvent?.Invoke(state);

            if(state == RTCState.RTC_CONNECTED)
            {
                OnReadyEvent?.Invoke();
            }
        }

        public void OnGatherStateChange(GatherState state)
        {
            OnGatherStateChangeEvent?.Invoke(state);
        }

        public void OnDataChannel(int dc)
        {
            dataId = dc;
            DataChannel.rtcSetClosedCallback(dataId, InternalOnClose);
            DataChannel.rtcSetMessageCallback(dataId, InternalOnMessage);
        }

        public void Dispose()
        {
            if (dataId > -1)
            {
                DataChannel.rtcDeleteDataChannel(dataId);
            }
            DataChannel.rtcDeletePeerConnection(connectionId);
            DataChannel.UnloadRTC();
            gch.Free();
            CoreUtils.CleanupConfig(configId);
            connectionId = -1;
            dataId = -1;
            disposed = true;
        }

        private static void InternalOnDataChannel(int dc, IntPtr ptr)
        {
            var pc = ptr.GetPC();
            pc.OnDataChannel(dc);
        }

        private static void InternalOnState(RTCState state, IntPtr ptr)
        {
            var pc = ptr.GetPC();
            pc.OnStateChange(state);
        }

        private static void InternalOnGatheringState(GatherState state, IntPtr ptr)
        {
            var pc = ptr.GetPC();
            pc.OnGatherStateChange(state);
        }

        private static void InternalOnOpen(IntPtr ptr)
        {
            var pc = ptr.GetPC();
            pc.OnOpen();
        }

        private static void InternalOnClose(IntPtr ptr)
        {
            var pc = ptr.GetPC();
            pc.OnClose();
        }

        private static void InternalOnMessage([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] message, int size, IntPtr ptr)
        {
            var pc = ptr.GetPC();
            pc.OnMessage(message);
        }

        private static void InternalOnDescription(string sdp, string type, IntPtr ptr)
        {
            var pc = ptr.GetPC();
            pc.OnDescription(sdp, type);
        }

        private static void InternalOnCandidate(string cand, string mid, IntPtr ptr)
        {
            var pc = ptr.GetPC();
            pc.OnCandidate(cand, mid);
        }
    }

    public static class PCUtils
    {
        public static PeerConnection GetPC(this IntPtr ptr)
        {
            var handle = GCHandle.FromIntPtr(ptr);
            return handle.Target as PeerConnection;
        }
    }
}
