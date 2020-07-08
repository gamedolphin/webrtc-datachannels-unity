using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RTC
{
    public delegate void LogCb(int handle, String message);
    public delegate void OpenCb(IntPtr ptr);
    public delegate void CloseCb(IntPtr ptr);
    public delegate void ErrorCb(string error, IntPtr ptr);
    public delegate void MessageCb([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] message, int size, IntPtr ptr);
    public delegate void GenericCallback();
    public delegate void MessageCallback(byte[] msg);
    public delegate void MessageStringCallback(string msg);
    public delegate void DataChannelCallback(int dc, IntPtr ptr);
    public delegate void DescriptionCallback(string sdp, string type, IntPtr ptr);
    public delegate void CandidateCallback(string candidate, string mid, IntPtr ptr);
    public delegate void StateChangeCallback(RTCState state, IntPtr ptr);
    public delegate void GatheringStateChangeCallback(GatherState state, IntPtr ptr);

    public delegate void StateChangeDelegate(RTCState state);
    public delegate void GatherStateChangeDelegate(GatherState state);
    public delegate void CandidateDelegate(string candidate, string mid);
    public delegate void DescriptionDelegate (string sdp, string type);

    [StructLayout(LayoutKind.Sequential)]
    public class RtcConfig
    {
        public IntPtr iceServers;
        public int iceServerCount;
        public UInt16 portRangeBegin;
        public UInt16 portRangeEnd;
    }

    public enum DescriptionType
    {
        Offer,
        Answer
    }

    public static class CoreUtils
    {

        public static string GetDescriptionType(DescriptionType d)
        {
            if (d == DescriptionType.Offer)
            {
                return "offer";
            }
            return "answer";
        }

        private struct RtcInfo
        {
            public IntPtr[] serverArray;
            public GCHandle gch;
        }

        private static readonly Dictionary<int,RtcInfo> rtcInfo = new Dictionary<int,RtcInfo>();
        private static int configCount = 0;

        public static int GetConfig(string[] servers, out RtcConfig config)
        {
            var info = new RtcInfo();
            info.serverArray = new IntPtr[servers.Length];
            for (int i = 0; i < servers.Length; ++i) {
                info.serverArray[i] = Marshal.StringToCoTaskMemUni(servers[i]);
            }

            info.gch = GCHandle.Alloc(info.serverArray, GCHandleType.Pinned);

            IntPtr addr = info.gch.AddrOfPinnedObject();

            config = new RtcConfig();
            config.iceServers = addr;
            config.iceServerCount = servers.Length;

            configCount += 1;

            rtcInfo.Add(configCount, info);

            return configCount;
        }

        public static void CleanupConfig(int id)
        {
            if(rtcInfo.ContainsKey(id))
            {
                var info = rtcInfo[id];

                info.gch.Free();

                foreach(var server in info.serverArray)
                {
                    Marshal.FreeCoTaskMem(server);
                }

                rtcInfo.Remove(id);
            }
        }
    }

    public enum RTCState
    {
        RTC_NEW = 0,
        RTC_CONNECTING = 1,
        RTC_CONNECTED = 2,
        RTC_DISCONNECTED = 3,
        RTC_FAILED = 4,
        RTC_CLOSED = 5
    }

    public enum GatherState
    {
        RTC_GATHERING_NEW = 0,
        RTC_GATHERING_INPROGRESS = 1,
        RTC_GATHERING_COMPLETE = 2
    }
}
