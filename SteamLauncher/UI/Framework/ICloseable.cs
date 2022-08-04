namespace SteamLauncher.UI.Framework
{
    /// <summary>
    /// Used as an MVVM-friendly way to close a Window/UserControl/Form/Application/etc from a ViewModel.
    /// </summary>
    public interface ICloseable
    {
        void Close();
    }
}
