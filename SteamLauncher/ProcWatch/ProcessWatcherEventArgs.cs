using System;
using System.IO;

namespace SteamLauncher.ProcWatch
{
    public class ProcessWatcherEventArgs : EventArgs
    {
        public ProcessWatcherEventArgs(FileInfo processFileInfo, ProcessStatus status)
        {
            ProcessFileInfo = processFileInfo;
            Status = status;
        }

        public enum ProcessStatus
        {
            Started,
            Stopped,
            WaitForStartTimeout
        }

        public ProcessStatus Status { get; set; }
        public FileInfo ProcessFileInfo { get; set; }
    }
}
