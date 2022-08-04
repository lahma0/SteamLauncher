using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using SteamLauncher.Attributes;
using SteamLauncher.DataStore;
using SteamLauncher.DataStore.SelectiveUse;
using SteamLauncher.DataStore.VTablesStore;
using SteamLauncher.Tools;
using SteamLauncher.UI.Framework;

namespace SteamLauncher.UI.ViewModels
{
    public class SettingsViewModel : ViewModelFramework //, IDataErrorInfo
    {
        public SettingsViewModel()
        {
            
        }

        #region SaveConfigCommand

        private ICommand _saveConfigCommand;

        public ICommand SaveConfigCommand =>
            _saveConfigCommand ??= new CommandHandler<ICloseable>(SaveConfig, o => CanSaveConfig);

        public bool CanSaveConfig => true;

        /// <summary>
        /// Writes/saves the ViewModel config values to the config file.
        /// </summary>
        private void SaveConfig(ICloseable window)
        {
            Settings.Config.LogLevel = LogLevel;
            Settings.Config.UniversalSteamLaunching = UniversalSteamLaunching;
            Settings.Config.PreventSteamFocusStealing = PreventSteamFocusStealing;
            Settings.Config.TotalSecondsToPreventSteamFocus = TotalSecondsToPreventSteamFocus;
            Settings.Config.ProcessStartTimeoutSec = ProcessStartTimeoutSec;
            Settings.Config.ProcessWatcherPollingIntervalSec = ProcessWatcherPollingIntervalSec;
            Settings.Config.DosBoxScummVmProxySleepMs = DosBoxScummVmProxySleepMs;
            Settings.Config.DosBoxExePath = DosBoxExePath;
            Settings.Config.ScummVmExePath = ScummVmExePath;
            
            Settings.Config.AutoUpdateVTables = AutoUpdateVTables;

            Settings.Config.Filtering = Filtering;
            Settings.Config.Filtering.FilterMode = FilterMode;
            Settings.Config.Filtering.Filters = Filters.Copy();

            //VtFile.Instance.GetIClientShortcutsIndex = _getIClientShortcutsIndex;
            //VtFile.Instance.BetaGetIClientShortcutsIndex = _betaGetIClientShortcutsIndex;

            // Create a deep copy of reference-type objects to assign to Settings.Config
            Settings.Config.CustomPlatformNames = CustomPlatformNames.Copy();
            Settings.Config.LauncherToExeDefinitions = LauncherToExeDefinitions.Copy();

            // Write config file to disk
            Settings.Config.Save();

            // Close the Settings window
            window?.Close();
        }

        #endregion SaveConfigCommand

        #region SetDefaultsCommand

        /// <summary>
        /// Reset all ViewModel config values to their defaults. Values still must be written/saved to be applied.
        /// </summary>
        private void SetDefaults()
        {
            //var defaultConfig = Config.GetDefaultConfig();
            var defaultConfig = DefaultConfig;
            LogLevel = defaultConfig.LogLevel;
            UniversalSteamLaunching = defaultConfig.UniversalSteamLaunching;
            PreventSteamFocusStealing = defaultConfig.PreventSteamFocusStealing;
            TotalSecondsToPreventSteamFocus = defaultConfig.TotalSecondsToPreventSteamFocus;
            ProcessStartTimeoutSec = defaultConfig.ProcessStartTimeoutSec;
            ProcessWatcherPollingIntervalSec = defaultConfig.ProcessWatcherPollingIntervalSec;
            DosBoxScummVmProxySleepMs = defaultConfig.DosBoxScummVmProxySleepMs;
            DosBoxExePath = defaultConfig.DosBoxExePath;
            ScummVmExePath = defaultConfig.ScummVmExePath;
            AutoUpdateVTables = defaultConfig.AutoUpdateVTables;

            Filtering = defaultConfig.Filtering;
            FilterMode = defaultConfig.Filtering.FilterMode;
            Filters = defaultConfig.Filtering.Filters;

            //var defaultOffsets = VtFile.GetDefaultInstance();
            //AutoUpdateVTables = defaultOffsets.AutoUpdateOnline;
            //_getIClientShortcutsIndex = defaultOffsets.GetIClientShortcutsIndex;
            //_betaGetIClientShortcutsIndex = defaultOffsets.BetaGetIClientShortcutsIndex;


            // Do not need to create a deep copy here bc defaultConfig is a newly instantiated object
            CustomPlatformNames = defaultConfig.CustomPlatformNames;
            LauncherToExeDefinitions = defaultConfig.LauncherToExeDefinitions;
        }

        private ICommand _setDefaultsCommand;

        public ICommand SetDefaultsCommand =>
            _setDefaultsCommand ??= new CommandHandler(SetDefaults, () => CanSetDefaults);

        public bool CanSetDefaults => true;

        #endregion SetDefaultsCommand

        #region SelectDosBoxPathCommand

        private ICommand _selectDosBoxPathCommand;

        public ICommand SelectDosBoxPathCommand =>
            _selectDosBoxPathCommand ??= new CommandHandler<IDialog>(SelectDosBoxPath, o => true);

        /// <summary>
        /// Requests the UI to spawn an 'Open File Dialog' to allow the user to select a new path for the DOSBox EXE.
        /// </summary>
        /// <param name="dlg">Sets the parent window of the dialog.</param>
        private void SelectDosBoxPath(IDialog dlg)
        {
            var path = Utilities.GetAbsolutePath(DosBoxExePath);
            var initialDir = Path.GetDirectoryName(path);
            var filename = Path.GetFileName(path);
            if (!File.Exists(DosBoxExePath))
            {
                // If DOSBox EXE path is invalid, set dialog's dir to LB path and default filename to 'DOSBox.exe'.
                filename = "DOSBox.exe";
                initialDir = Info.LaunchBoxDir;
            }

            var paths = dlg.OpenFileDialog(filename: filename,
                                              initialDirectory: initialDir,
                                              defaultExt: "exe",
                                              title: "Select DOSBox Executable");

            if (paths == null || !paths.Any() || !File.Exists(paths[0])) 
                return;

            var relativePath = Utilities.GetRelativePath(paths[0], Info.LaunchBoxDir);
            DosBoxExePath = relativePath;
        }

        #endregion SelectDosBoxPathCommand

        #region SelectScummVmPathCommand

        private ICommand _selectScummVmPathCommand;
        /// <summary>
        /// Requests the UI to spawn an 'Open File Dialog' to allow the user to select a new path for the ScummVM EXE.
        /// </summary>
        public ICommand SelectScummVmPathCommand =>
            _selectScummVmPathCommand ??= new CommandHandler<IDialog>(dlg =>
            {
                var path = Utilities.GetAbsolutePath(ScummVmExePath);
                var initialDir = Path.GetDirectoryName(path);
                var filename = Path.GetFileName(path);
                if (!File.Exists(ScummVmExePath))
                {
                    // If ScummVM EXE path is invalid, set dialog's dir to LB path & default filename to 'scummvm.exe'.
                    filename = "scummvm.exe";
                    initialDir = Info.LaunchBoxDir;
                }

                var paths = dlg?.OpenFileDialog(filename: filename,
                                                initialDirectory: initialDir,
                                                defaultExt: "exe",
                                                title: "Select ScummVM Executable");

                if (paths == null || !paths.Any() || !File.Exists(paths[0]))
                    return;

                var relativePath = Utilities.GetRelativePath(paths[0], Info.LaunchBoxDir);
                ScummVmExePath = relativePath;
            }, o => true);

        #endregion SelectScummVmPathCommand

        private PropertyInfo _currentSettingPropertyInfo;
        /// <summary>
        /// When the mouse hovers over a panel related to a particular setting, this is assigned that setting's
        /// PropertyInfo.
        /// </summary>
        public PropertyInfo CurrentSettingPropertyInfo
        {
            get => _currentSettingPropertyInfo;
            set
            {
                SetField(ref _currentSettingPropertyInfo, value);
                if (value == null)
                {
                    CurrentSettingName = null;
                    CurrentSettingDescription = null;
                    CurrentSettingFootnote = null;
                    return;
                }

                var displayAttribute = value.GetCustomAttribute<DisplayAttribute>();
                var footerAttribute = value.GetCustomAttribute<FooterAttribute>();
                CurrentSettingName = displayAttribute?.Name;
                CurrentSettingDescription = displayAttribute?.Description;

                if (footerAttribute == null)
                {
                    var defaultValue = DefaultConfig.GetPropertyValue(value.Name);
                    CurrentSettingFootnote = $"Default Value: {defaultValue}";
                }
                else
                {
                    CurrentSettingFootnote = footerAttribute.Footer;
                }
                //else if (footerAttribute.Footer == "VTablesInfo")
                //{
                //    var defaultValue = DefaultVTablesFile.GetPropertyValue(value.Name);
                //    CurrentSettingFootnote = $"Default Value: {defaultValue}";
                //}

            }
        }

        //private string _currentSettingPropertyName;
        ///// <summary>
        ///// When the mouse hovers over a panel related to a particular setting, this will be set to that setting's
        ///// property name.
        ///// </summary>
        //public string CurrentSettingPropertyName
        //{
        //    get => _currentSettingPropertyName;
        //    set
        //    {
        //        SetField(ref _currentSettingPropertyName, value);
        //        if (string.IsNullOrWhiteSpace(value))
        //        {
        //            CurrentSettingName = null;
        //            CurrentSettingDescription = null;
        //            CurrentSettingFootnote = null;
        //            return;
        //        }
        //    }
        //}

        private string _currentSettingName;
        /// <summary>
        /// When the mouse hovers over a panel related to a particular setting, this is assigned that setting's name.
        /// </summary>
        public string CurrentSettingName
        {
            get => _currentSettingName;
            private set => SetField(ref _currentSettingName, value);
        }

        private string _currentSettingDescription;
        /// <summary>
        /// When the mouse hovers over a panel related to a particular setting, this is assigned that setting's
        /// description.
        /// </summary>
        public string CurrentSettingDescription
        {
            get => _currentSettingDescription;
            private set => SetField(ref _currentSettingDescription, value);
        }


        private string _currentSettingFootnote;
        /// <summary>
        /// When the mouse hovers over a panel related to a particular setting, this is assigned that setting's
        /// footnote attribute if it exists. Otherwise, it is assigned that setting's default value.
        /// </summary>
        public string CurrentSettingFootnote
        {
            get => _currentSettingFootnote;
            set => SetField(ref _currentSettingFootnote, value);
        }

        /// <summary>
        /// Provides default Config values used in the 'MouseOver' descriptions that appear in the Settings UI.
        /// </summary>
        public static Config DefaultConfig { get; } = Config.LoadXmlOrDefaults(disableLoadFromFile: true);

        ///// <summary>
        ///// Provides default VTablesInfo values used in the 'MouseOver' descriptions that appear in the Settings UI.
        ///// </summary>
        //public static VtFile DefaultVTablesFile { get; } = VtFile.GetDefaultInstance();

        private TraceLevel _logLevel = Settings.Config.LogLevel;
        [Display(Name = "Log Level",
                 Description = "Defines the verbosity of log output: Off -> Error -> Warning -> Info -> Verbose.")]
        public TraceLevel LogLevel
        {
            get => _logLevel;
            set => SetField(ref _logLevel, value);
        }

        private bool _universalSteamLaunching = Settings.Config.UniversalSteamLaunching;
        [Display(Name = "Universal Steam Launching", 
                 Description = "Defines whether the plugin should intercept all game launches by default, " +
                               "redirecting them to launch through Steam.")]
        public bool UniversalSteamLaunching
        {
            get => _universalSteamLaunching;
            set => SetField(ref _universalSteamLaunching, value);
        }

        #region SteamFocusStealing

        private bool _preventSteamFocusStealing = Settings.Config.PreventSteamFocusStealing;
        [Display(Name = "Prevent Steam Focus Stealing",
                 Description = "Whether to prevent Steam from stealing focus from LB/BB after a game exits.")]
        public bool PreventSteamFocusStealing
        {
            get => _preventSteamFocusStealing;
            set => SetField(ref _preventSteamFocusStealing, value);
        }

        private int _totalSecondsToPreventSteamFocus = Settings.Config.TotalSecondsToPreventSteamFocus;
        [Display(Name = "Total Seconds to Prevent Steam Focus",
                 Description = "On most machines, this will not need to be enabled, but in rare cases, Steam will forcibly " +
                     "steal focus from LB/BB upon exit of a game. This typically happens about 3-4 seconds after " +
                     "the game window closes and the LB/BB window regains focus. If 'Prevent Steam Focus Stealing' " +
                     "is enabled, this value defines the number of seconds Steam will be prevented from stealing " +
                     "focus. If set too high, this can prevent intentional activation of the Steam window.")]
        public int TotalSecondsToPreventSteamFocus
        {
            get => _totalSecondsToPreventSteamFocus;
            set
            {
                SetField(ref _totalSecondsToPreventSteamFocus, value);
                OnPropertyChanged(nameof(IsTotalSecondsToPreventSteamFocusInValidRange));
            }
        }

        public static Range<int> TotalSecondsToPreventSteamFocusValidRange =>
            Config.TotalSecondsToPreventSteamFocusValidRange;

        public bool IsTotalSecondsToPreventSteamFocusInValidRange =>
            TotalSecondsToPreventSteamFocusValidRange.ContainsValue(TotalSecondsToPreventSteamFocus);

        #endregion SteamFocusStealing

        #region ProcessStartTimeout

        private int _processStartTimeoutSec = Settings.Config.ProcessStartTimeoutSec;
        [Display(Name = "Process Start Timeout",
                 Description = "When UniversalSteamLaunching is enabled, this defines how many seconds the plugin will wait " + 
                     "for Steam to launch a process before deciding that the action has timed out.")]
        public int ProcessStartTimeoutSec
        {
            get => _processStartTimeoutSec;
            set
            {
                SetField(ref _processStartTimeoutSec, value);
                OnPropertyChanged(nameof(IsProcessStartTimeoutSecInValidRange));
            }
        }

        public static Range<int> ProcessStartTimeoutSecValidRange => Config.ProcessStartTimeoutSecValidRange;

        public bool IsProcessStartTimeoutSecInValidRange =>
            ProcessStartTimeoutSecValidRange.ContainsValue(ProcessStartTimeoutSec);

        #endregion ProcessStartTimeout

        #region ProcessWatcherPollingInterval

        private int _processWatcherPollingIntervalSec = Settings.Config.ProcessWatcherPollingIntervalSec;
        [Display(Name = "Process Watcher Polling Interval",
                 Description = "When UniversalSteamLaunching is enabled, this defines the interval between updates by " + 
                     "ProcessWatcher, a module used to monitor processes which are launched by Steam.")]
        public int ProcessWatcherPollingIntervalSec
        {
            get => _processWatcherPollingIntervalSec;
            set
            {
                SetField(ref _processWatcherPollingIntervalSec, value);
                OnPropertyChanged(nameof(IsProcessWatcherPollingIntervalSecInValidRange));
            }
        }

        public static Range<int> ProcessWatcherPollingIntervalSecValidRange =>
            Config.ProcessWatcherPollingIntervalSecValidRange;

        public bool IsProcessWatcherPollingIntervalSecInValidRange =>
            ProcessWatcherPollingIntervalSecValidRange.ContainsValue(ProcessWatcherPollingIntervalSec);

        #endregion ProcessWatcherPollingInterval

        #region DosBoxScummVmProxySleep

        private int _dosBoxScummVmProxySleepMs = Settings.Config.DosBoxScummVmProxySleepMs;
        [Display(Name = "DOSBox/ScummVM Proxy Sleep",
                 Description = "The DOSBox or ScummVM exe must be temporarily replaced with the plugin's proxy exe " +
                               "when using Universal Steam Launching. During this process, a thread performs several " +
                               "file operations (copies, renames, etc) and must wait briefly on the file system to " +
                               "complete these tasks before continuing. This sets the length of that delay in ms " +
                               "(occurs once per launch). In most cases, a value smaller than the default (400ms) " +
                               "would work fine, but the difference is so small as to be imperceptible by the user.")]
        public int DosBoxScummVmProxySleepMs
        {
            get => _dosBoxScummVmProxySleepMs;
            set
            {
                SetField(ref _dosBoxScummVmProxySleepMs, value);
                OnPropertyChanged(nameof(IsDosBoxScummVmProxySleepMsInValidRange));
            }
        }

        public static Range<int> DosBoxScummVmProxySleepMsValidRange => Config.DosBoxScummVmProxySleepMsValidRange;

        public bool IsDosBoxScummVmProxySleepMsInValidRange =>
            DosBoxScummVmProxySleepMsValidRange.ContainsValue(DosBoxScummVmProxySleepMs);

        #endregion DosBoxScummVmProxySleep

        #region Misc

        private string _dosBoxExePath = Settings.Config.DosBoxExePath;
        [Display(Name = "DOSBox Exe Path",
                 Description = "The relative path to the DOSBox exe included with LaunchBox. This should not need " +
                               "to be changed unless LaunchBox has changed the location of the DOSBox exe within " +
                               "its directory structure.")]
        [CustomValidation(typeof(SettingsViewModel), nameof(FileExistsValidation))]
        public string DosBoxExePath
        {
            get => _dosBoxExePath;
            set => SetField(ref _dosBoxExePath, value);
        }

        private string _scummVmExePath = Settings.Config.ScummVmExePath;
        [Display(Name = "ScummVM Exe Path",
                 Description = "The relative path to the ScummVM exe included with LaunchBox. This should not need " +
                               "to be changed unless LaunchBox has changed the location of the ScummVM exe within " +
                               "its directory structure.")]
        [CustomValidation(typeof(SettingsViewModel), nameof(FileExistsValidation))]
        public string ScummVmExePath
        {
            get => _scummVmExePath;
            set => SetField(ref _scummVmExePath, value);
        }

        #region VTableInfo

        private bool _autoUpdateVTables = Settings.Config.AutoUpdateVTables;
        [Display(Name = "Auto Update VTables Data Online",
                 Description = "If this setting is enabled, the plugin automatically updates any applicable vtable " +
                               "data using an online database maintained by the plugin author.")]
        [Footer("VTablesInfo")]
        public bool AutoUpdateVTables
        {
            get => _autoUpdateVTables;
            set => SetField(ref _autoUpdateVTables, value);
        }

        [Display(Name = "Online DB Last Updated",
                 Description = "The 'last updated' date/time of the online DB.")]
        [Footer("")]
        public DateTime? OnlineDbLastUpdatedLocalDateTime => Settings.VTables.OnlineDbLastUpdatedLocalDateTime;

        public string OnlineDbLastUpdatedLocalFormattedString =>
            OnlineDbLastUpdatedLocalDateTime == null ? "Online DB Last Modified: Never Updated"
                : $"Online DB Last Modified: {OnlineDbLastUpdatedLocalDateTime}";

        public void ForceUpdate()
        {
            Settings.VTables.OnlineUpdate(true);
            OnPropertyChanged(nameof(OnlineDbLastUpdatedLocalDateTime));
            OnPropertyChanged(nameof(OnlineDbLastUpdatedLocalFormattedString));
        }

        private ICommand _forceUpdateCommand;

        [Display(Name = "Force Update",
                 Description = "Checks if an updated version of vtables are available in the online DB and forces an " +
                               "update if available.")]
        [Footer("")]
        public ICommand ForceUpdateCommand => _forceUpdateCommand ??= new CommandHandler(ForceUpdate, () => true);

        #endregion

        #endregion

        #region PlatformNames

        // Create a deep copy of reference-type object so Settings.Config and ConfigViewModel are not linked [.Copy()]
        private List<Platform> _customPlatformNames = Settings.Config.CustomPlatformNames.Copy();
        [Display(Name = "Custom Platform Names",
                 Description = "User-defined list of custom platform names to replace predefined platform names provided by " +
                               "LaunchBox. Custom platform names will be seen in your 'Currently Playing' status in Steam. " +
                               "Example: SteamUser is currently playing 'Super Mario World (SNES)'.")]
        [Footer("To omit the platform name, leave 'Custom Platform Name' empty.")]
        public List<Platform> CustomPlatformNames
        {
            get => _customPlatformNames;
            set => SetField(ref _customPlatformNames, value);
        }

        #endregion

        #region GameLaunchers

        // Create a deep copy of reference-type object so Settings.Config and ConfigViewModel are not linked [.Copy()]
        private List<LauncherToExe> _launcherToExeDefinitions = Settings.Config.LauncherToExeDefinitions.Copy();
        [Display(Name = "Launcher to Exe Definitions", 
                 Description = "A list of game launchers and the EXEs they are responsible for launching. For the " +
                               "plugin to operate, it must track when Steam games start/end. Some games use a " +
                               "launcher EXE to start the game EXE, after which the launcher EXE exits. In these " +
                               "cases, the plugin assumes the game exited since it only has knowledge of the " +
                               "launcher EXE. This list associates a game's launcher EXE with its main EXE so the " +
                               "plugin can correctly deduce when the game has exited. This functionality can be " +
                               "extended for more advanced tasks such as launching games through their client/store, " +
                               "loaders/patchers, virtualized/portable/sandboxed EXEs, etc.")]
        [Footer("Acceptable formats: full path, relative path to LB dir, exe filename.")]
        public List<LauncherToExe> LauncherToExeDefinitions
        {
            get => _launcherToExeDefinitions;
            set => SetField(ref _launcherToExeDefinitions, value);
        }

        #endregion

        #region Filtering

        private Filtering _filtering = Settings.Config.Filtering;

        [Display(Name = "Selective Use Filters",
                 Description = "Allow customizing when SL is/isn't enabled using a combination of the 3 following " +
                               "settings. #1. The filters list will dictate when SL is enabled/disabled. " +
                               "#2. The 'Filter Mode' will dictate when/how these filters will be applied. #3. In " +
                               "a LB game's properties, a new entry can be created under 'Custom Fields' with a key " +
                               "named 'SLEnabled'. Setting this entry's value to '1' causes SL to ALWAYS " +
                               "intercept this game's launches. Setting this entry's value to '0' causes SL to NEVER " +
                               "intercept this game's launches. This setting always overrides all filters and filter " +
                               "modes.")]
        [Footer("'SLEnabled' Custom Field entries override all filters and filter mode settings.")]
        public Filtering Filtering
        {
            get => _filtering;
            set => SetField(ref _filtering, value);
        }

        private FilterMode _filterMode = Settings.Config.Filtering.FilterMode;
        [Display(Name = "Filter Mode",
                 Description = "Defines the way in which the user supplied filters are utilized. OFF: Disable this feature entirely. " +
                               "BLACKLIST: SteamLauncher is always ENABLED unless a matching filter is found. " +
                               "WHITELIST: SteamLauncher is always DISABLED unless a matching filter is found.")]
        [Footer("'Ignore Custom Fields' causes the value of all 'SLEnabled' Custom Field entries to be ignored.")]
        public FilterMode FilterMode
        {
            get => _filterMode;
            set => SetField(ref _filterMode, value);
        }

        private List<Filter> _filters = Settings.Config.Filtering.Filters.Copy();
        [Display(Name = "Filters List",
                 Description = "These filters will dictate when SL is enabled/disabled based on the selected " +
                               "'Filter Mode'. The 'Filter String' is NOT case sensitive and it supports wildcards. " +
                               "The '*' wildcard matches one or more characters and the '?' wildcard matches any " +
                               "single character. The 'Filter Type' dictates what field the filter string is " +
                               "compared against.")]
        [Footer("Filter descriptions are entirely optional and can be left blank if desired.")]
        public List<Filter> Filters
        {
            get => Filtering.Filters;
            set => SetField(ref _filters, value);
        }

        //private FilterType _filterType = Settings.Config.Filtering.FilterType;
        //[Display(Name = "Filter Mode",
        //         Description = "Defines how this feature is globally applied. OFF: Disable this feature entirely. " +
        //                       "BLACKLIST: The plugin is always enabled unless a matching filter is found. " +
        //                       "WHITELIST: The plugin is always disabled unless a matching filter is found.")]
        //public FilterMode FilterType
        //{
        //    get => _filterMode;
        //    set => SetField(ref _filterMode, value);
        //}

        #endregion

        //public bool IsSteamRunning => SteamProcessInfo.GetSteamProcess() != null;

        //private SteamStatus _steamStatus;
        //public SteamStatus SteamStatus
        //{
        //    get => _steamStatus;
        //    set => SetField(ref _steamStatus, value);
        //}

        public SteamStatusTimerViewModel SteamStatusTimer { get; } = new SteamStatusTimerViewModel();

        #region DataValidation

        public static ValidationResult FileExistsValidation(object value, ValidationContext context)
        {
            var vm = (SettingsViewModel) context.ObjectInstance;
            if (!File.Exists((string) value))
                return new ValidationResult("File not found.", new List<string> { context.MemberName });

            return ValidationResult.Success;
        }

        public static ValidationResult IsNumericValidation(object value, ValidationContext context)
        {
            var vm = (SettingsViewModel)context.ObjectInstance;
            var result = int.TryParse(((string) value), out var i);
            if (!result)
                return new ValidationResult("Value must be a number.", new List<string> { context.MemberName });

            return ValidationResult.Success;
        }

        #endregion
    }

    //public class SettingsVmForDesignTime : SettingsViewModel
    //{
    //    public new string CurrentSettingName => "Launcher to Exe Definitions";

    //    public new string CurrentSettingDescription =>
    //        "A list of game launchers and the EXEs they are responsible for launching. For the " +
    //        "plugin to operate, it must track when Steam games start/end. Some games use a " +
    //        "launcher EXE to start the game EXE, after which the launcher EXE exits. In these " +
    //        "cases, the plugin assumes the game exited since it only has knowledge of the " +
    //        "launcher EXE. This list associates a game's launcher EXE with its main EXE so the " +
    //        "plugin can correctly deduce when the game has exited. This functionality can be " +
    //        "extended for more advanced tasks such as launching games through their client/store, " +
    //        "loaders/patchers, virtualized/portable/sandboxed EXEs, etc.";

    //    public new string CurrentSettingFootnote => "A couple of entries are added by default (DOSBox/ScummVM/etc).";
    //}
}
