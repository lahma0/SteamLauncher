using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace SteamLauncher.Logging
{
    public static class Logger
    {
        public static void Error(string message, int indentLevel = 0, [CallerFilePath] string callerPath = null, [CallerMemberName] string memberName = null)
        {
            WriteEntry(message, indentLevel, "ERROR", callerPath, memberName);
        }

        public static void Error(Exception ex, int indentLevel = 0, [CallerFilePath] string callerPath = null, [CallerMemberName] string memberName = null)
        {
            WriteEntry(ex.Message, indentLevel, "ERROR", callerPath, memberName);
        }

        public static void Warning(string message, int indentLevel = 0, [CallerFilePath] string callerPath = null, [CallerMemberName] string memberName = null)
        {
            WriteEntry(message, indentLevel, "WARNING", callerPath, memberName);
        }

        public static void Info(string message, int indentLevel = 0, [CallerFilePath] string callerPath = null, [CallerMemberName] string memberName = null)
        {
            WriteEntry(message, indentLevel, "INFO", callerPath, memberName);
        }

        private static void WriteEntry(string message, int indentLevel, string type, string callerPath, string memberName)
        {
            var msg = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{type}]";
            if (!string.IsNullOrWhiteSpace(callerPath))
                msg += $" [{Path.GetFileNameWithoutExtension(callerPath)}]";
            if (!string.IsNullOrWhiteSpace(memberName))
                msg += $" [{memberName}]";

            msg += $" - {message}";

            Trace.IndentLevel = indentLevel;
            Trace.WriteLine(msg);
        }

        public static void EnableDebugLog(string logFilePath, int maxFileLen = 1000000, int maxFileCount = 4, FileMode fileMode = FileMode.Append)
        {
            // Setup debug logging
            var fileStream = new FileStreamWithBackup(logFilePath, maxFileLen, maxFileCount, fileMode);
            var textWriter = new TextWriterTraceListener(fileStream);
            Trace.Listeners.Add(textWriter);
            Trace.AutoFlush = true;
        }
    }
}
