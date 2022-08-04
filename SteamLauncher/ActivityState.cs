namespace SteamLauncher
{
    public class ActivityState
    {
        public string ActivityName { get; set; }

        public string CurrentState { get; set; }

        public bool IsCancelled { get; private set; }

        public string CancelledReason { get; private set; }

        public ActivityState(string activityName)
        {
            ActivityName = activityName;
        }

        public ActivityState(string activityName, string currentState)
        {
            ActivityName = activityName;
            CurrentState = currentState;
        }

        public void Cancel(string cancelReason)
        {
            IsCancelled = true;
            CancelledReason = cancelReason;
        }

        public void ResetCancel()
        {
            IsCancelled = false;
            CancelledReason = "";
        }
    }
}
