using System;
using System.Runtime.InteropServices;

namespace RTC
{
    public class Websocket : IDisposable
    {
        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern int rtcCreateWebSocket(string url);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern int rtcDeleteWebsocket(int ws);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern int rtcSetOpenCallback(int id, OpenCb cb);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern int rtcSetClosedCallback(int id, CloseCb cb);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern int rtcSetErrorCallback(int id, ErrorCb cb);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern int rtcSetMessageCallback(int id, MessageCb cb);

        [DllImport("DataChannel", CallingConvention = CallingConvention.Cdecl)]
        private static extern int rtcSendMessage(int id, string data, int size);

        private int websocketId = -1;
        private bool disposed = false;


        public event GenericCallback OnConnect;
        public event GenericCallback OnDisconnect;
        public event MessageStringCallback OnError;
        public event MessageStringCallback OnMessage;
        public event MessageStringCallback OnStringMessage;

        public void Connect(string url)
        {
            websocketId = rtcCreateWebSocket(url);
            rtcSetOpenCallback(websocketId, OnOpen);
            rtcSetClosedCallback(websocketId, OnClose);
            rtcSetErrorCallback(websocketId, _OnError);
            rtcSetMessageCallback(websocketId, _OnMessage);
            Logger.Log("Connecting to " + url);
        }

        public void Dispose()
        {
            rtcDeleteWebsocket(websocketId);
            disposed = true;
        }

        public void SendMessage(string msg)
        {
            rtcSendMessage(websocketId, msg, msg.Length);
        }

        private void OnOpen(IntPtr ptr)
        {
            Logger.Log("[Websocket] Socket connected");
            OnConnect?.Invoke();
        }

        private void OnClose(IntPtr ptr)
        {
            Logger.Log("[Websocket] Socket disconnected");
            OnDisconnect?.Invoke();
        }

        private void _OnError(string error, IntPtr ptr)
        {
            Logger.Log($"[Websocket] Socket error {error}");
            OnError?.Invoke(error);
        }

        private void _OnMessage([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] msg, int size, IntPtr ptr)
        {
            if (size < 0)
            {
                Logger.Log($"[Websocket] Socket Message:  {msg}");
                var str = System.Text.Encoding.UTF8.GetString(msg);
                OnStringMessage?.Invoke(str);
            }
            else
            {
                var str = System.Text.Encoding.UTF8.GetString(msg);
                Logger.Log($"Received binary message {str}");
                OnMessage?.Invoke(str);
            }
        }

        ~Websocket()
        {
            if (!disposed)
            {
                Dispose();
            }
        }
    }
}
