using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
using SteamLauncher.Logging;


namespace SteamLauncher.ProcWatch
{
    public class ProcessWatcher : IObserver<CimSubscriptionResult>, IDisposable
    {
        internal enum CimWatcherStatus
        {
            Default, 
            Started, 
            Stopped
        }

        #region Events

        public event EventHandler<ProcessWatcherEventArgs> StatusUpdatedEventHandler;

        #endregion

        #region Fields

        private object _myLock;
        private bool _isDisposed;
        private CimWatcherStatus _cimWatcherStatus;
        private static readonly string ComputerName = null;
        private const string NameSpace = @"root\cimv2";
        private const string QueryDialect = "WQL";
        private CimSession _cimSession;
        private CimAsyncMultipleResults<CimSubscriptionResult> _cimObservable;
        private IDisposable _subscription;

        #endregion

        #region Properties

        public FileInfo ProcessFileInfo { get; }
        public int WaitForStartTimeoutSec { get; }
        private int PollingIntervalSec { get; }
        private bool ProcessStarted { get; set; }
        private bool StartTimeoutTriggered { get; set; }
        private string QueryExpression => $"select * from __InstanceOperationEvent within {PollingIntervalSec} " +
                                          $"where (__class like '%creation%' OR __class like '%deletion%') AND " +
                                          $"TargetInstance ISA 'Win32_Process' AND TargetInstance.Name = " +
                                          $"'{ProcessFileInfo.Name}'";

        #endregion


        public ProcessWatcher(FileInfo processFileInfo, int waitForStartTimeoutSec = 10, int pollingIntervalSec = 2)
        {
            Logger.Info($"Instantiated ProcessWatcher for file '{processFileInfo.Name}' (WaitForStartTimeout: " + 
                        $"{waitForStartTimeoutSec}s).");
            ProcessFileInfo = processFileInfo;
            WaitForStartTimeoutSec = waitForStartTimeoutSec;
            PollingIntervalSec = pollingIntervalSec;
            Initialize();
        }

        public void Initialize()
        {
            _cimWatcherStatus = CimWatcherStatus.Default;
            _myLock = new object();
            _cimSession = CimSession.Create(ComputerName);
            _cimObservable = _cimSession.SubscribeAsync(NameSpace, QueryDialect, QueryExpression);
        }

        public void Start()
        {
            lock (_myLock)
            {
                if (_isDisposed)
                    return;
                //throw new ObjectDisposedException(nameof(ProcessWatcher));

                if (_cimWatcherStatus == CimWatcherStatus.Started)
                    return;

                _subscription = _cimObservable.Subscribe(this);
                _cimWatcherStatus = CimWatcherStatus.Started;
                Logger.Info($"{nameof(ProcessWatcher)} started.");

                if (WaitForStartTimeoutSec > 0)
                    // Fire and forget - Do not wait for completion/return value
                    _ = ScheduleProcessStartTimeoutCheck();
            }
        }

        public void Stop()
        {
            lock (_myLock)
            {
                if (_isDisposed)
                    return;
                //throw new ObjectDisposedException(nameof(ProcessWatcher));

                if (_cimWatcherStatus != CimWatcherStatus.Started)
                    return;

                _subscription?.Dispose();
                _cimWatcherStatus = CimWatcherStatus.Stopped;
                Logger.Info($"{nameof(ProcessWatcher)} stopped.");
            }
        }

        public void OnCompleted()
        {
            Logger.Info($"Cim Push Event: {nameof(OnCompleted)}");
        }

        public void OnError(Exception error)
        {
            Logger.Warning($"Cim Push Event: {nameof(OnError)}: {error.Message}");
        }

        public void OnNext(CimSubscriptionResult value)
        {
            if (StartTimeoutTriggered || _cimWatcherStatus != CimWatcherStatus.Started || value?.Instance == null)
                return;

            Logger.Info($"Cim Push Event: {nameof(OnNext)}: {value.Instance?.CimClass} | " + 
                        $"{value.Instance?.CimInstanceProperties?.LastOrDefault()?.Value}");

            if (value.Instance.CimClass.ToString().ToLower().Contains("creation"))
            {
                ProcessStarted = true;
                OnProcessStatusUpdated(
                    new ProcessWatcherEventArgs(ProcessFileInfo, ProcessWatcherEventArgs.ProcessStatus.Started));
            }
            else if (value.Instance.CimClass.ToString().ToLower().Contains("deletion") && ProcessStarted)
            {
                OnProcessStatusUpdated(
                    new ProcessWatcherEventArgs(ProcessFileInfo, ProcessWatcherEventArgs.ProcessStatus.Stopped));
            }
        }

        public void Dispose()
        {
            _isDisposed = true;
            _subscription?.Dispose();
            _cimSession?.Dispose();
        }

        private async Task ScheduleProcessStartTimeoutCheck()
        {
            Logger.Info($"Scheduling {WaitForStartTimeoutSec} check.");

            await Task.Delay(new TimeSpan(0, 0, WaitForStartTimeoutSec));
            if (_cimWatcherStatus == CimWatcherStatus.Started && !ProcessStarted)
            {
                StartTimeoutTriggered = true;
                Logger.Info($"{nameof(WaitForStartTimeoutSec)} timeout triggered (process did not " +
                            $"start in allotted time).");
                OnProcessStatusUpdated(
                    new ProcessWatcherEventArgs(ProcessFileInfo, 
                                                ProcessWatcherEventArgs.ProcessStatus.WaitForStartTimeout));
            }
        }

        private void OnProcessStatusUpdated(ProcessWatcherEventArgs e)
        {
            if (StatusUpdatedEventHandler != null)
            {
                Logger.Info($"Invoking '{nameof(StatusUpdatedEventHandler)}' for event type '{e.Status}'.");
            }

            Application.Current.Dispatcher.Invoke(() => StatusUpdatedEventHandler?.Invoke(this, e));

            if (e.Status == ProcessWatcherEventArgs.ProcessStatus.WaitForStartTimeout || 
                e.Status == ProcessWatcherEventArgs.ProcessStatus.Stopped)
            {
                Stop();
            }
        }
    }

    //public class ProcessWatcher
    //{
    //    public event EventHandler<ProcessWatcherEventArgs> StatusUpdatedEventHandler;

    //    public FileInfo ProcessFileInfo { get; }

    //    public int WaitForStartTimeoutSec { get; }

    //    private ManagementEventWatcher EventWatcher { get; }

    //    private bool IsEventWatcherEnabled { get; set; }

    //    private bool ProcessStarted { get; set; }

    //    private bool StartTimeoutTriggered { get; set; }

    //    public ProcessWatcher(FileInfo processFileInfo, int waitForStartTimeoutSec = 10, int pollingIntervalSec = 2)
    //    {
    //        Logger.Info($"Instantiated ProcessWatcher for file '{processFileInfo.Name}' (WaitForStartTimeout: {waitForStartTimeoutSec}s).");
    //        ProcessFileInfo = processFileInfo;
    //        WaitForStartTimeoutSec = waitForStartTimeoutSec;
    //        EventWatcher = new ManagementEventWatcher(new WqlEventQuery("__InstanceOperationEvent", new TimeSpan(0, 0, pollingIntervalSec), $"TargetInstance ISA 'Win32_Process' AND TargetInstance.Name = '{ProcessFileInfo.Name}'"));
    //    }

    //    ~ProcessWatcher()
    //    {
    //        // Logger.Info($"Disposing ProcessWatcher for file '{ProcessFileInfo.Name}'.");
    //    }

    //    public void Start()
    //    {
    //        if (IsEventWatcherEnabled)
    //            return;

    //        Logger.Info("EventWatcher enabled.");
    //        EventWatcher.EventArrived += OnEventArrived;
    //        EventWatcher.Start();
    //        IsEventWatcherEnabled = true;


    //        if (WaitForStartTimeoutSec > 0)
    //            // Fire and forget - Do not wait for completion/return value
    //            _ = ScheduleProcessStartTimeoutCheck();
    //    }

    //    public void Stop()
    //    {
    //        if (!IsEventWatcherEnabled)
    //            return;

    //        Logger.Info("EventWatcher disabled.");
    //        EventWatcher.Stop();
    //        EventWatcher.EventArrived -= OnEventArrived;
    //        IsEventWatcherEnabled = false;
    //    }

    //    private void OnEventArrived(object sender, EventArrivedEventArgs e)
    //    {
    //        //Logger.Info($"ProcessWatcher:OnEventArrived triggered!");

    //        if (StartTimeoutTriggered || !IsEventWatcherEnabled)
    //            return;

    //        if (e.NewEvent.ClassPath.ClassName.Contains("InstanceCreationEvent"))
    //        {
    //            Logger.Info($"ProcessWatcher:OnEventArrived contains class name InstanceCreationEvent");
    //            ProcessStarted = true;
    //            OnProcessStatusUpdated(new ProcessWatcherEventArgs(ProcessFileInfo, ProcessWatcherEventArgs.ProcessStatus.Started));
    //        }
    //        else if (e.NewEvent.ClassPath.ClassName.Contains("InstanceDeletionEvent") && ProcessStarted)
    //        {
    //            Logger.Info($"ProcessWatcher:OnEventArrived contains class name InstanceDeletionEvent");
    //            OnProcessStatusUpdated(new ProcessWatcherEventArgs(ProcessFileInfo, ProcessWatcherEventArgs.ProcessStatus.Stopped));
    //        }
    //    }

    //    private async Task ScheduleProcessStartTimeoutCheck()
    //    {
    //        Logger.Info("Scheduling Process Start Timeout Check.");

    //        await Task.Delay(new TimeSpan(0, 0, WaitForStartTimeoutSec));
    //        if (IsEventWatcherEnabled && !ProcessStarted)
    //        {
    //            StartTimeoutTriggered = true;
    //            Logger.Info("ProcessWatcher timeout triggered (process did not start in allotted time).");
    //            OnProcessStatusUpdated(new ProcessWatcherEventArgs(ProcessFileInfo, ProcessWatcherEventArgs.ProcessStatus.WaitForStartTimeout));
    //        }
    //    }

    //    private void OnProcessStatusUpdated(ProcessWatcherEventArgs e)
    //    {
    //        if (StatusUpdatedEventHandler != null)
    //        {
    //            Logger.Info($"Invoking '{nameof(StatusUpdatedEventHandler)}' for event type '{e.Status.ToString()}'.");
    //        }

    //        Application.Current.Dispatcher.Invoke(() => StatusUpdatedEventHandler?.Invoke(this, e));

    //        if (e.Status == ProcessWatcherEventArgs.ProcessStatus.WaitForStartTimeout || e.Status == ProcessWatcherEventArgs.ProcessStatus.Stopped)
    //        {
    //            Stop();
    //        }
    //    }
    //}
}
