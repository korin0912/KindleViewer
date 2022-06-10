using System;
using System.Text;

namespace KindleViewer
{
    public static class Log
    {
        private static StringBuilder sb = new StringBuilder();

        private enum Type
        {
            Info,
            Warning,
            Error,
        }

        private static void LogPut(Type type, string message)
        {
            switch (type)
            {
                case Type.Info: Console.ForegroundColor = ConsoleColor.White; break;
                case Type.Warning: Console.ForegroundColor = ConsoleColor.Yellow; break;
                case Type.Error: Console.ForegroundColor = ConsoleColor.Red; break;
            }
            sb.Clear()
                .Append("[")
                .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"))
                .Append("][")
                .Append(type.ToString())
                .Append("] ")
                .Append(message);
            Console.WriteLine(sb.ToString());
            Console.ResetColor();
        }

        public static void Info(string message) => LogPut(Type.Info, message);

        public static void Warning(string message) => LogPut(Type.Warning, message);

        public static void Error(string message) => LogPut(Type.Error, message);
    }
}
