namespace RTC
{
    public interface ILog
    {
        void Log(string msg);
    }

    public class DefaultLogger : ILog
    {
        public void Log(string msg)
        {
            System.Console.WriteLine(msg);
        }
    }

    public static class Logger
    {
        public static ILog log = new DefaultLogger();

        public static void Log(string msg)
        {
            log?.Log(msg);
        }
    }
}