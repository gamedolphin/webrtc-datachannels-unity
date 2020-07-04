using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

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

    public struct RtcConfig
    {
        public string[] iceServers;
        public int iceServerCount;
        public UInt16 portRangeBegin;
        public UInt16 portRangeEnd;
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