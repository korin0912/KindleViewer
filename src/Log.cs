using System;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;

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

        private static void LogPut(Type type, string message, string filePath, int lineNumber)
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
                .Append(message)
                .Append(" (")
                .Append(Path.GetFileName(filePath))
                .Append(":")
                .Append(lineNumber)
                .Append(")")
            ;
            Console.WriteLine(sb.ToString());
            Console.ResetColor();
        }

        public static void Info(string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) => LogPut(Type.Info, message, filePath, lineNumber);

        public static void Warning(string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) => LogPut(Type.Warning, message, filePath, lineNumber);

        public static void Error(string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) => LogPut(Type.Error, message, filePath, lineNumber);
    }
}
