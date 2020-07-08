using System;
using System.Runtime.InteropServices;

namespace RTC
{
    public class WS : IDisposable
    {
        private int websocketId = -1;

        private GCHandle gch;
        private bool disposed = false;

        public event GenericCallback OnOpenEvent;
        public event GenericCallback OnCloseEvent;
        public event MessageStringCallback OnErrorEvent;
        public event MessageStringCallback OnMessageEvent;

        private ILog logger;

        public WS(string url, ILog _logger)
        {
            DataChannel.ReadyRTC();
            logger = _logger;
            websocketId = DataChannel.rtcCreateWebSocket(url);
            gch = GCHandle.Alloc(this);
            IntPtr ptr = GCHandle.ToIntPtr(gch);

            DataChannel.rtcSetUserPointer(websocketId, ptr);
            DataChannel.rtcSetOpenCallback(websocketId,  InternalOnOpen);
            DataChannel.rtcSetClosedCallback(websocketId, InternalOnClose);
            DataChannel.rtcSetErrorCallback(websocketId, InternalOnError);
            DataChannel.rtcSetMessageCallback(websocketId, InternalOnMessage);
        }

        public void SendMessage(string msg)
        {
            DataChannel.rtcSendMessage(websocketId, msg, msg.Length);
        }

        public void OnOpen()
        {
            logger.Log("Connected websocket");
            OnOpenEvent?.Invoke();
        }

        public void OnClose()
        {
            logger.Log("Disconnected websocket");
            OnCloseEvent?.Invoke();
        }

        public void OnError(string msg)
        {
            logger.Log($"Error : {msg}");
            OnErrorEvent?.Invoke(msg);
        }

        public void OnMessage(byte[] msg)
        {
            try {
                var str = System.Text.Encoding.UTF8.GetString(msg);
                logger.Log($"Message : {str}");
                OnMessageEvent?.Invoke(str);
            }
            catch (Exception ex)
            {
                logger.Log($"Exception parsing : {ex.Message}");
            }
        }


        ~WS()
        {
            if (!disposed)
            {
                Dispose();
            }
        }

        private static void InternalOnOpen(IntPtr ptr)
        {
            var ws = ptr.GetWS();
            ws.OnOpen();
        }

        private static void InternalOnClose(IntPtr ptr)
        {
            var ws = ptr.GetWS();
            ws.OnClose();
        }

        private static void InternalOnError(string msg, IntPtr ptr)
        {
            var ws = ptr.GetWS();
            ws.OnError(msg);
        }

        private static void InternalOnMessage([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] msg, int size, IntPtr ptr)
        {
            var ws = ptr.GetWS();
            ws.OnMessage(msg);
        }

        public void Dispose()
        {
            logger.Log("Disposing...");
            DataChannel.rtcDeleteWebsocket(websocketId);
            gch.Free();
            DataChannel.UnloadRTC();
            disposed = true;
        }
    }

    public static class WSUtils
    {
        public static WS GetWS(this IntPtr ptr)
        {
            var handle = GCHandle.FromIntPtr(ptr);
            return handle.Target as WS;
        }
    }
}
