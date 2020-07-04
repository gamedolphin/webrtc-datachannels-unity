using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RTC
{

    public class DataChannel : IDisposable
    {
        // RTC_EXPORT void rtcInitLogger(rtcLogLevel level, rtcLogCallbackFunc cb); // NULL cb to log to stdout
        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern void rtcInitLogger(int logLevel, LogCb cb);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern void rtcPreload();

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern void rtcCleanup();

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern void rtcSetUserPointer(int id, IntPtr ptr);

        // PeerConnection
        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern int rtcCreatePeerConnection(ref RtcConfig config); // returns pc id
        
        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern int rtcDeletePeerConnection(int pc);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern int rtcSetDataChannelCallback(int pc, DataChannelCallback cb);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern int rtcSetLocalDescriptionCallback(int pc, DescriptionCallback cb);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern int rtcSetLocalCandidateCallback(int pc, CandidateCallback cb);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern int rtcSetStateChangeCallback(int pc, StateChangeCallback cb);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern int rtcSetGatheringStateChangeCallback(int pc, GatheringStateChangeCallback cb);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern int rtcSetRemoteDescription(int pc, [MarshalAs(UnmanagedType.LPWStr)] string sdp, [MarshalAs(UnmanagedType.LPWStr)]  string type);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern int rtcAddRemoteCandidate(int pc, [MarshalAs(UnmanagedType.LPWStr)] string cand, [MarshalAs(UnmanagedType.LPWStr)] string mid);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern int rtcGetLocalAddress(int pc, [MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 1)] ref string buffer, int size);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern int rtcGetRemoteAddress(int pc, [MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 1)] ref string buffer, int size);

        public DataChannel()
        {
            rtcPreload();
            // rtcInitLogger(6, OnLog);
        }

        public void Dispose()
        {
            rtcCleanup();
        }

        private void OnLog(int logLevel, string message)
        {
            Logger.Log($"[DataChannel] {message}");
        }
    }
}