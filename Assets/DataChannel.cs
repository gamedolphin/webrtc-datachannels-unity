using System;
using System.Runtime.InteropServices;

namespace RTC
{

    public static class DataChannel
    {
        // RTC_EXPORT void rtcInitLogger(rtcLogLevel level, rtcLogCallbackFunc cb); // NULL cb to log to stdout
        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern void rtcInitLogger(int logLevel, LogCb cb);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern void rtcPreload();

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern void rtcCleanup();

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern int rtcCreateWebSocket(string url);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern int rtcDeleteWebsocket(int ws);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern void rtcSetUserPointer(int id, IntPtr ptr);

        // PeerConnection
        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern int rtcCreatePeerConnection(ref RtcConfig config); // returns pc id

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern int rtcDeletePeerConnection(int pc);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern int rtcSetDataChannelCallback(int pc, DataChannelCallback cb);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern int rtcSetLocalDescriptionCallback(int pc, DescriptionCallback cb);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern int rtcSetLocalCandidateCallback(int pc, CandidateCallback cb);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern int rtcSetStateChangeCallback(int pc, StateChangeCallback cb);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern int rtcSetGatheringStateChangeCallback(int pc, GatheringStateChangeCallback cb);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern int rtcSetRemoteDescription(int pc, string sdp, string type);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern int rtcAddRemoteCandidate(int pc, string cand, string mid);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern int rtcGetLocalAddress(int pc, [MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 1)] ref string buffer, int size);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern int rtcGetRemoteAddress(int pc, [MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 1)] ref string buffer, int size);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern int rtcCreateDataChannel(int pc, string label); // returns dc id

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern int rtcDeleteDataChannel(int dc);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern int rtcSetOpenCallback(int id, OpenCb cb);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern int rtcSetClosedCallback(int id, CloseCb cb);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern int rtcSetErrorCallback(int id, ErrorCb cb);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern int rtcSetMessageCallback(int id, MessageCb cb);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        public static extern int rtcSendMessage(int id, string data, int size);


        private static bool rtcLoaded = false;
        private static int rtcCount = 0;

        public static void ReadyRTC()
        {
            rtcCount += 1;
            // load before any rtc is created
            if (rtcLoaded)
            {
                return;
            }

            // rtcInitLogger(4, (ptr,msg) => Logger.Log(msg));
            rtcPreload();
            rtcLoaded = true;
        }

        public static void UnloadRTC()
        {
            // unload after _all_ rtc are shutdown
            rtcCount -= 1;

            if (rtcCount == 0)
            {
                rtcCleanup();
            }
        }
    }
}
