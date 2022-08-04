using System;
using Microsoft.Management.Infrastructure;

namespace SteamLauncher.ProcWatch
{
    public class CimWatcherEventArgs : EventArgs
    {
        public CimSubscriptionResult CimSubscriptionResult { get; }

        public CimWatcherEventArgs(CimSubscriptionResult cimSubscriptionResult)
        {
            CimSubscriptionResult = cimSubscriptionResult;
        }
    }
}