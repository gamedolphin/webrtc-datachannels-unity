using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

        // Start is called before the first frame update
        private void Start()
        {
            Logger.log = new UnityLogger();
            rtc = new DataChannel();
            ws = new Websocket();

            ws.OnConnect += () =>
            {
                Debug.Log("Connect cb, sending message");
                ws.SendMessage("Hello");
            };
            ws.OnDisconnect += () => Debug.Log("Called close cb");
            ws.OnError += (str) => Debug.Log($"Error cb {str}");
            ws.OnMessage += (str) => Debug.Log($"Message cb {str}");

            ws.Connect("ws://localhost:8080/?id=1");
        }


        private void OnDestroy()
        {
            Debug.Log("Disposing...");
            ws.Dispose();
            rtc.Dispose();
        }
    }

}
