using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using SteamLauncher.DataStore;

namespace SteamLauncher.Logging
{
    //public enum LogLevel
    //{
    //    Disabled = 0,
    //    Critical = 1,
    //    Error = 2,
    //    Warning = 4,
    //    Information = 8,
    //    Verbose = 16,
    //    //// ReSharper disable InconsistentNaming
    //    //disabled = Disabled,
    //    //DISABLED = Disabled,
    //    //error = Error,
    //    //ERROR = Error,
    //    //warning = Warning,
    //    //WARNING = Warning,
    //    //information = Information,
    //    //INFORMATION = Information,
    //    //verbose = Verbose,
    //    //VERBOSE = Verbose,
    //    //// ReSharper restore InconsistentNaming
    //}

    public static class Logger
    {
        // Default trace level to use until a proper user defined value can be read from the config
        private const TraceLevel DefaultTraceLevel = TraceLevel.Warning;

        static Logger()
        {
            EnableLogFileOutput(Config.LOG_PATH);
        }

        ///// <summary>
        ///// Converts a string regardless of case into its equivalent <see cref="Verbosity"/> enum.
        ///// </summary>
        ///// <param name="verbosityString">The string to interpret as an enum.</param>
        ///// <returns>A <see cref="Verbosity"/> enum value.</returns>
        //public static Verbosity ConvertStringToVerbosityEnum(string verbosityString)
        //{
        //    return (Verbosity)Enum.Parse(typeof(Verbosity), verbosityString.ToUpper());
        //}

        ///// <summary>
        ///// Used to print CRITICAL messages to the log. These messages indicate an unrecoverable error has occurred and
        ///// will most likely result in an immediate halt in the execution of the application.
        ///// </summary>
        ///// <param name="message">The message to print to the log.</param>
        ///// <param name="indentLevel">The indent level to be applied to this message.</param>
        ///// <param name="callerPath">
        ///// The file path of the caller (at the time of compile). This value is filled in automatically via the
        ///// 'CallerFilePath' attribute.
        ///// </param>
        ///// <param name="memberName">
        ///// The method or property name of the caller. This value is filled in automatically via the 'CallerMemberName'
        ///// attribute.
        ///// </param>
        //public static void Critical(string message, int indentLevel = 0, [CallerFilePath] string callerPath = null, [CallerMemberName] string memberName = null)
        //{
        //    WriteEntry(message, indentLevel, TraceEventType.Critical, callerPath, memberName);
        //}

        /// <summary>
        /// Used to print ERROR messages to the log. These messages indicate the existence of a serious problem that
        /// will likely prevent the software from completing one of its core functions or from continuing execution
        /// altogether.
        /// </summary>
        /// <param name="message">The message to print to the log.</param>
        /// <param name="indentLevel">The indent level to be applied to this message.</param>
        /// <param name="callerPath">
        /// The file path of the caller (at the time of compile). This value is filled in automatically via the
        /// 'CallerFilePath' attribute.
        /// </param>
        /// <param name="memberName">
        /// The method or property name of the caller. This value is filled in automatically via the 'CallerMemberName'
        /// attribute.
        /// </param>
        public static void Error(string message, int indentLevel = 0, [CallerFilePath] string callerPath = null, [CallerMemberName] string memberName = null)
        {
            WriteEntry(message, indentLevel, TraceLevel.Error, callerPath, memberName);
        }

        /// <summary>
        /// Used to print WARNING messages to the log. These messages indicate the existence of some unexpected behavior, non-critical error, or the possibility of a future problem.
        /// </summary>
        /// <param name="message">The message to print to the log.</param>
        /// <param name="indentLevel">The indent level to be applied to this message.</param>
        /// <param name="callerPath">
        /// The file path of the caller (at the time of compile). This value is filled in automatically via the
        /// 'CallerFilePath' attribute.
        /// </param>
        /// <param name="memberName">
        /// The method or property name of the caller. This value is filled in automatically via the 'CallerMemberName'
        /// attribute.
        /// </param>
        public static void Warning(string message, int indentLevel = 0, [CallerFilePath] string callerPath = null, [CallerMemberName] string memberName = null)
        {
            WriteEntry(message, indentLevel, TraceLevel.Warning, callerPath, memberName);
        }

        /// <summary>
        /// Used to print INFO messages to the log. These messages contain general information, status updates, and
        /// confirmation that things are working as expected.
        /// </summary>
        /// <param name="message">The message to print to the log.</param>
        /// <param name="indentLevel">The indent level to be applied to this message.</param>
        /// <param name="callerPath">
        /// The file path of the caller (at the time of compile). This value is filled in automatically via the
        /// 'CallerFilePath' attribute.
        /// </param>
        /// <param name="memberName">
        /// The method or property name of the caller. This value is filled in automatically via the 'CallerMemberName'
        /// attribute.
        /// </param>
        public static void Info(string message, int indentLevel = 0, [CallerFilePath] string callerPath = null, [CallerMemberName] string memberName = null)
        {
            WriteEntry(message, indentLevel, TraceLevel.Info, callerPath, memberName);
        }

        /// <summary>
        /// Used to print VERBOSE messages to the log. These messages contain very detailed information about the control
        /// flow of the application and usually are only of interest when diagnosing problems.
        /// </summary>
        /// <param name="message">The message to print to the log.</param>
        /// <param name="indentLevel">The indent level to be applied to this message.</param>
        /// <param name="callerPath">
        /// The file path of the caller (at the time of compile). This value is filled in automatically via the
        /// 'CallerFilePath' attribute.
        /// </param>
        /// <param name="memberName">
        /// The method or property name of the caller. This value is filled in automatically via the 'CallerMemberName'
        /// attribute.
        /// </param>
        public static void Verbose(string message, int indentLevel = 0, [CallerFilePath] string callerPath = null, [CallerMemberName] string memberName = null)
        {
            WriteEntry(message, indentLevel, TraceLevel.Verbose, callerPath, memberName);
        }

        /// <summary>
        /// Handles the formatting and categorization of log messages.
        /// </summary>
        /// <param name="message">The message to print to the log.</param>
        /// <param name="indentLevel">The indent level to be applied to this message.</param>
        /// <param name="traceLevel">The log type/category.</param>
        /// <param name="callerPath">
        /// The file path of the caller (at the time of compile). This value is filled in automatically via the
        /// 'CallerFilePath' attribute.
        /// </param>
        /// <param name="memberName">
        /// The method or property name of the caller. This value is filled in automatically via the 'CallerMemberName'
        /// attribute.
        /// </param>
        private static void WriteEntry(string message, int indentLevel, TraceLevel traceLevel, string callerPath, string memberName)
        {
            var formattedMsg = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{traceLevel.ToString()}]";
            if (!string.IsNullOrWhiteSpace(callerPath))
                formattedMsg += $" [{Path.GetFileNameWithoutExtension(callerPath)}]";
            if (!string.IsNullOrWhiteSpace(memberName))
                formattedMsg += $" [{memberName}]";

            formattedMsg += $" - {message}";
            Trace.IndentLevel = indentLevel;

            // This code may be reached before Config is finished initializing (many times if the config file doesn't
            // exist or contains bad data). We use a default LogLevel here until Config is finished initializing.
            var logLevel = Logger.DefaultTraceLevel;
            if (Settings.Config != null)
                logLevel = Settings.Config.LogLevel;
            
            Trace.WriteLineIf((int)traceLevel <= (int)logLevel, formattedMsg);

            //switch (traceLevel)
            //{
            //    case TraceLevel.Error:
            //        Trace.TraceError(msg);
            //        break;
            //    case TraceLevel.Warning:
            //        Trace.TraceWarning(msg);
            //        break;
            //    case TraceLevel.Info:
            //        Trace.TraceInformation(msg);
            //        break;
            //    case TraceLevel.Verbose:
            //        Trace.WriteLine(msg);
            //        break;
            //}
        }

        /// <summary>
        /// Enables the writing of trace/log message to file.
        /// </summary>
        /// <param name="logFilePath">File path to be used for log output.</param>
        /// <param name="maxFileLen">Max length of the log file before it is archived and a new log file is created.</param>
        /// <param name="maxFileCount">Maximum number of log files to preserve.</param>
        /// <param name="fileMode">File mode to use when enabling log file output.</param>
        public static void EnableLogFileOutput(string logFilePath, int maxFileLen = 1000000, int maxFileCount = 4, FileMode fileMode = FileMode.Append)
        {
            try
            {
                // Setup debug logging
                var fileStream = new FileStreamWithBackup(logFilePath, maxFileLen, maxFileCount, fileMode);
                var textWriter = new TextWriterTraceListener(fileStream);
                //var textWriter = new TextWriterTraceListener(fileStream) {Filter = new LoggerFilter()};
                Trace.Listeners.Add(textWriter);
                Trace.AutoFlush = true;
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while attempting to enable log file output. Exception: {ex.Message}");
            }
            
        }
    }
}
