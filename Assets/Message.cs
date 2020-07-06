using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace RTC
{
    public enum MessageType : byte
    {
        None = 127,
        JoinRequest = 0,
        JoinResponse = 1,
        LeaveRequest = 2,
        SendOffer = 3,
        SendAnswer = 4,
        SendIce = 5
    }

    public static class MessageUtil
    {

        private static readonly Dictionary<MessageType,System.Type> dataTypeMap = new Dictionary<MessageType, System.Type>()
        {
            { MessageType.JoinRequest, typeof(JoinRequestData) },
            { MessageType.JoinResponse, typeof(JoinResponseData) },
            { MessageType.LeaveRequest, typeof(LeaveRequestData) },
            { MessageType.SendOffer, typeof(SendOfferData) },
            { MessageType.SendAnswer, typeof(SendAnswerData) },
            { MessageType.SendIce, typeof(SendIceData) },
        };

        public static string GetMessage<T>(MessageType msgType, T data)
        {
            return ((byte)msgType).ToString() + JsonConvert.SerializeObject(data);
        }

        public static T GetData<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }
    }

    public struct SendOfferData
    {
        public string ClientId;
        public string Offer;
    }

    public struct SendAnswerData
    {
        public string ClientId;
        public string Answer;
    }

    public struct SendIceData
    {
        public string ClientId;
        public string Ice;
    }

    public struct JoinRequestData
    {
        public string RoomId;
    }

    public struct JoinResponseData
    {
        public string[] OtherClients;
    }

    public struct LeaveRequestData
    {

    }
}
