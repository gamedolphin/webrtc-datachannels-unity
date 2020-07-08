using UnityEngine;
using System.Collections;

namespace RTC
{
    public class UnityLogger : ILog
    {
        public void Log(string str)
        {
            Debug.Log(str);
        }
    }

    public class UnityLoggerWithId : ILog
    {
        public string[] tags;
        private string id;
        private bool showLog = false;

        public UnityLoggerWithId(string _tag, bool _showLog)
        {
            id = _tag;
            showLog = _showLog;
        }

        public void Log(string str)
        {
            if(showLog)
            {
                Debug.Log($"{id} : {str}");
            }
        }
    }

    public class ConnectionTest : MonoBehaviour
    {
        [SerializeField]
        private string id = "";

        [SerializeField]
        private bool showLog = false;

        private UnityLoggerWithId logger;
        private Client client;

        private void Start()
        {
            // remove this later
            Logger.log = new UnityLogger();
            logger = new UnityLoggerWithId(id, showLog);

            client = new Client(id, logger);

            client.OnReady += () => client.JoinRoom("Room1");
            client.OnConnected += (otherId) => Debug.Log($"Connected to {otherId}");

            client.Connect("localhost:8080");
        }

        private void OnDestroy()
        {
            client.Dispose();
        }
    }
}
