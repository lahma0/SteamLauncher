using System;
using System.Windows.Input;
using System.Windows.Threading;
using SteamLauncher.SteamClient;
using SteamLauncher.UI.Framework;

namespace SteamLauncher.UI.ViewModels
{
    public class SteamStatusTimerViewModel : ViewModelFramework
    {
        private DispatcherTimer _timer = null;

        public SteamStatusTimerViewModel(int duration = 1000)
        {
            UpdateSteamStatus();
            Duration = duration;
        }

        #region Properties

        private int _duration;
        public int Duration
        {
            get => _duration;
            set => SetField(ref _duration, value);
        }

        private SteamStatus _steamStatus;
        public SteamStatus SteamStatus
        {
            get => _steamStatus;
            set => SetField(ref _steamStatus, value);
        }

        private ICommand _startTimerCommand;
        public ICommand StartTimerCommand => _startTimerCommand ??= new CommandHandler(StartTimer, () => true);

        private ICommand _stopTimerCommand;
        public ICommand StopTimerCommand => _stopTimerCommand ??= new CommandHandler(StopTimer, () => true);

        #endregion

        private void StartTimer()
        {
            _timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(Duration)};
            _timer.Tick += new EventHandler(TimerTick);
            _timer.Start();
        }

        private void StopTimer()
        {
            if (_timer == null) 
                return;

            _timer.Stop();
            _timer = null;
        }

        private void TimerTick(object send, EventArgs e)
        {
            UpdateSteamStatus();
        }

        private void UpdateSteamStatus()
        {
            SteamProcessInfo.UpdateSteamStatus();
            SteamStatus = SteamProcessInfo.SteamProcessStatus;
        }
    }
}
