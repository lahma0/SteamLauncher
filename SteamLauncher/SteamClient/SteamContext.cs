using SteamLauncher.DataStore;
using SteamLauncher.Logging;
using SteamLauncher.SteamClient.Interfaces;
using SteamLauncher.SteamClient.Native;
using System;
using System.IO;
using System.Runtime.InteropServices;
using SteamLauncher.DataStore.VTablesStore;

namespace SteamLauncher.SteamClient
{
    public sealed class SteamContext : IDisposable
    {
        public string ClientEngineInterfaceName => "IClientEngine";

        public string ClientShortcutsInterfaceName => "IClientShortcuts";

        public string GetIClientShortcutsName => "GetIClientShortcuts";

        private VTable _clientShortcutsVTable = null;

        public VTable ClientShortcutsVTable
        {
            get
            {
                if (_clientShortcutsVTable != null)
                {
                    var currentSteamPid = SteamProcessInfo.GetSteamProcess().Id;
                    if (currentSteamPid != LastKnownSteamPid)
                    {
                        Logger.Info($"Steam interfaces needs to be reinitialized because Steam was closed " +
                                        $"and/or restarted. Current Steam PID: {currentSteamPid}; Last Known " +
                                        $"Steam PID: {LastKnownSteamPid}");
                        ResetInit();
                    }
                }

                if (_clientShortcutsVTable == null)
                {
                    Logger.Info($"Initializing {nameof(ClientShortcutsVTable)}...");
                    if (!InitClientShortcutsInterface())
                        return null;

                    _clientShortcutsVTable = Settings.VTables.GetVTable(ClientShortcutsInterfaceName);
                    LastKnownSteamPid = SteamProcessInfo.GetSteamPid();
                    Logger.Info($"Setting {nameof(LastKnownSteamPid)}: {LastKnownSteamPid}");
                }

                return _clientShortcutsVTable;
            }
        }

        #region Unmanaged Pointers

        /// <summary>
        /// A handle to the Steam Client DLL.
        /// </summary>
        private IntPtr SteamClientDllHandle { get; set; } = IntPtr.Zero;

        #endregion Unmanaged Pointers

        #region Other Properties

        private int Pipe { get; set; } = 0;

        private int User { get; set; } = 0;

        private int LastKnownSteamPid { get; set; } = 0;

        #endregion Other Properties

        #region Singleton Constructor/Destructor

        private static readonly Lazy<SteamContext> Lazy = new Lazy<SteamContext>(() => new SteamContext());

        public static SteamContext Instance => Lazy.Value;

        private SteamContext()
        {
            Logger.Info($"Instantiating {nameof(SteamContext)} singleton...");
            //InitClientShortcutsInterface();
        }

        #endregion Singleton Constructor/Destructor

        #region Delegate Implementations

        /// <summary>
        /// An instance of the <see cref="SteamNative.CreateInterface"/> delegate.
        /// </summary>
        private SteamNative.CreateInterface _callCreateInterface;

        /// <summary>
        /// Gets an unmanaged handle to an instance of the specified Steam interface.
        /// </summary>
        /// <param name="version">A string defining the desired interface and version.</param>
        /// <param name="returnCode">An IntPtr value to return if the call fails.</param>
        /// <returns>A handle to the unmanaged Steam interface, or the provided <paramref name="returnCode"/> value upon failure.</returns>
        private IntPtr CreateInterface(string version, IntPtr returnCode)
        {
            if (_callCreateInterface == null)
                throw new InvalidOperationException($"Steam Client library is not initialized ({nameof(CreateInterface)}).");

            return _callCreateInterface(version, returnCode);
        }

        /// <summary>
        /// An instance of the <see cref="SteamNative.CreateSteamPipe"/> delegate.
        /// </summary>
        private SteamNative.CreateSteamPipe _callCreateSteamPipe;

        /// <summary>
        /// Creates a communication pipe with Steam to facilitate execution of additional Client functions.
        /// </summary>
        /// <returns>An int representing a specific Steam communication pipe.</returns>
        private int CreateSteamPipe()
        {
            if (_callCreateSteamPipe == null)
                throw new InvalidOperationException($"Steam Client library is not initialized ({nameof(CreateSteamPipe)}).");

            return _callCreateSteamPipe();
        }

        /// <summary>
        /// An instance of the <see cref="SteamNative.BReleaseSteamPipe"/> delegate.
        /// </summary>
        private SteamNative.BReleaseSteamPipe _callBReleaseSteamPipe;

        /// <summary>
        /// Releases the previously acquired communication pipe to the Steam Client.
        /// </summary>
        /// <param name="pipe">The Steam communication pipe acquired by calling <see cref="CreateSteamPipe"/>.</param>
        /// <returns>true if the provided pipe value was valid and released successfully; otherwise, false.</returns>
        private bool BReleaseSteamPipe(int pipe)
        {
            if (_callBReleaseSteamPipe == null)
                throw new InvalidOperationException($"Steam Client library is not initialized ({nameof(BReleaseSteamPipe)}).");

            return _callBReleaseSteamPipe(pipe);
        }

        /// <summary>
        /// An instance of the <see cref="SteamNative.ConnectToGlobalUser"/> delegate.
        /// </summary>
        private SteamNative.ConnectToGlobalUser _callConnectToGlobalUser;

        /// <summary>
        /// Connects to an existing global Steam user.
        /// </summary>
        /// <param name="pipe">The Steam communication pipe acquired by calling <see cref="CreateSteamPipe"/>.</param>
        /// <returns>An int defining a connection to an existing global Steam user; returns 0 if call failed for any reason.</returns>
        private int ConnectToGlobalUser(int pipe)
        {
            if (_callConnectToGlobalUser == null)
                throw new InvalidOperationException($"Steam Client library is not initialized ({nameof(ConnectToGlobalUser)}).");

            return _callConnectToGlobalUser(pipe);
        }

        /// <summary>
        /// An instance of the <see cref="SteamNative.ReleaseUser"/> delegate.
        /// </summary>
        private SteamNative.ReleaseUser _callReleaseUser;

        /// <summary>
        /// Releases the ongoing connection to a global Steam user; this should be called prior to terminating a communication pipe.
        /// </summary>
        /// <param name="pipe">The Steam communication pipe acquired by calling <see cref="CreateSteamPipe"/>.</param>
        /// <param name="user">An int defining a connection to an existing global Steam user.</param>
        private void ReleaseUser(int pipe, int user)
        {
            if (_callReleaseUser == null)
                throw new InvalidOperationException($"Steam Client library is not initialized ({nameof(ReleaseUser)}).");

            _callReleaseUser(pipe, user);
        }

        #endregion Delegate Implementations

        #region Utility Functions

        /// <summary>
        /// Checks if the provided handle is a non-zero value (the address itself is NOT validated).
        /// </summary>
        /// <param name="handle">The IntPtr value to be checked.</param>
        /// <returns>true if the handle is a non-zero value; otherwise, false.</returns>
        private bool IsLoaded(IntPtr handle) => (handle != IntPtr.Zero);

        /// <summary>
        /// Checks if the provided value is within the range of valid Steam Client user values.
        /// </summary>
        /// <param name="user">An int defining a connection to an existing global Steam user.</param>
        /// <returns>true if the provided value is within the valid range for Steam Client user values; otherwise, returns false.</returns>
        private bool IsValidUser(int user) => (user != 0 && user != -1);

        /// <summary>
        /// Checks if the provided value is within the range of valid Steam communication pipe values.
        /// </summary>
        /// <param name="pipe">The Steam communication pipe acquired by calling <see cref="CreateSteamPipe"/>.</param>
        /// <returns>true if the provided value is within the valid range for Steam communication pipe values; otherwise, returns false.</returns>
        private bool IsValidPipe(int pipe) => (pipe != 0);

        public void ResetInit()
        {
            SteamClientDllHandle = IntPtr.Zero;
            Settings.VTables?.GetVTable(ClientEngineInterfaceName)?.Detach();
            Settings.VTables?.GetVTable(ClientShortcutsInterfaceName)?.Detach();
            _clientShortcutsVTable = null;

            _callCreateInterface = null;
            _callCreateSteamPipe = null;
            _callBReleaseSteamPipe = null;
            _callConnectToGlobalUser = null;
            _callReleaseUser = null;

            User = 0;
            Pipe = 0;
        }

        #endregion Utility Functions

        #region Steam Client Library Initialization

        private bool InitSteam()
        {
            SteamProcessInfo.GetSteamProcessAsync().GetAwaiter().GetResult();
            if (SteamProcessInfo.GetSteamProcess() == null)
            {
                Logger.Error("The Steam process could not be located or started. Aborting Steam initialization.");
                return false;
            }

            if (IsLoaded(SteamClientDllHandle))
            {
                Logger.Info("Steam is already initialized.");
                return true;
            }

            Logger.Info("Beginning initialization of Steam...");

            SysNative.SetDllDirectory(SteamProcessInfo.SteamInstallPath + ";" + Path.Combine(SteamProcessInfo.SteamInstallPath, "bin"));
            var clientDllHandle = SysNative.LoadLibraryEx(SteamProcessInfo.SteamClientDllPath, IntPtr.Zero, SysNative.LOAD_WITH_ALTERED_SEARCH_PATH);
            if (!IsLoaded(clientDllHandle))
            {
                Logger.Error("Failed to load the Steam Client DLL. Aborting initialization.");
                return false;
            }

            _callCreateInterface = SysNative.GetExportFunction<SteamNative.CreateInterface>(clientDllHandle, "CreateInterface");
            if (_callCreateInterface == null)
            {
                Logger.Error("Failed to retrieve the 'CreateInterface' export function. Aborting initialization.");
                return false;
            }

            _callCreateSteamPipe = SysNative.GetExportFunction<SteamNative.CreateSteamPipe>(clientDllHandle, "Steam_CreateSteamPipe");
            if (_callCreateSteamPipe == null)
            {
                Logger.Error("Failed to retrieve the 'Steam_CreateSteamPipe' export function. Aborting initialization.");
                return false;
            }

            _callBReleaseSteamPipe = SysNative.GetExportFunction<SteamNative.BReleaseSteamPipe>(clientDllHandle, "Steam_BReleaseSteamPipe");
            if (_callBReleaseSteamPipe == null)
            {
                Logger.Error("Failed to retrieve the 'Steam_BReleaseSteamPipe' export function. Aborting initialization.");
                return false;
            }

            _callConnectToGlobalUser = SysNative.GetExportFunction<SteamNative.ConnectToGlobalUser>(clientDllHandle, "Steam_ConnectToGlobalUser");
            if (_callConnectToGlobalUser == null)
            {
                Logger.Error("Failed to retrieve the 'Steam_ConnectToGlobalUser' export function. Aborting initialization.");
                return false;
            }

            _callReleaseUser = SysNative.GetExportFunction<SteamNative.ReleaseUser>(clientDllHandle, "Steam_ReleaseUser");
            if (_callReleaseUser == null)
            {
                Logger.Error("Failed to retrieve the 'Steam_ReleaseUser' export function. Aborting initialization.");
                return false;
            }

            Pipe = CreateSteamPipe();
            if (!IsValidPipe(Pipe))
            {
                Logger.Error("Failed to create a Steam pipe (IPC). Aborting initialization.");
                return false;
            }

            User = ConnectToGlobalUser(Pipe);
            if (!IsValidUser(User))
            {
                Logger.Error("Failed to connect to Steam global user (IPC). Aborting initialization.");
                return false;
            }

            SteamClientDllHandle = clientDllHandle;
            Logger.Info("Steam initialization succeeded!");

            return true;
        }

        /// <summary>
        /// Initializes 'IClientEngine' interface.
        /// </summary>
        /// <returns></returns>
        private bool InitClientEngineInterface()
        {
            //if (IsLoaded(ClientEngineInterfacePtr))
            if (Settings.VTables.GetVTable(ClientEngineInterfaceName).IsAttached)
            {
                Logger.Info("ClientEngine is already initialized.");
                return true;
            }

            if (!InitSteam())
                return false;

            Logger.Info("Beginning initialization of the ClientEngine interface...");

            var engineInterfacePtr = IntPtr.Zero;
            try
            {
                engineInterfacePtr = CreateInterface(
                    Settings.VTables.GetVTable(ClientEngineInterfaceName).InterfaceVersion,
                    IntPtr.Zero);
            }
            catch
            {
                // suppress exception
            }

            if (IsLoaded(engineInterfacePtr))
            {
                Logger.Info("ClientEngine initialization succeeded!");
                Settings.VTables.GetVTable(ClientEngineInterfaceName).Attach(engineInterfacePtr);
                return true;
            }

            Logger.Error("Failed to create the ClientEngine interface. Aborting initialization.");
            return false;
        }

        /// <summary>
        /// Initializes 'IClientShortcuts' interface.
        /// </summary>
        /// <returns>If successful, true; otherwise, false.</returns>
        private bool InitClientShortcutsInterface()
        {
            if (Settings.VTables.GetVTable(ClientShortcutsInterfaceName).IsAttached)
            {
                Logger.Info("ClientShortcuts is already initialized.");
                return true;
            }

            if (!InitClientEngineInterface())
                return false;

            Logger.Info("Beginning initialization of the ClientShortcuts interface...");

            var clientShortcutsInterfacePtr = GetShortcutsInterfacePtr(
                Settings.VTables.GetVTable(ClientEngineInterfaceName).VTablePtr);

            if (!IsLoaded(clientShortcutsInterfacePtr))
            {
                Logger.Error("Failed to retrieve a valid ClientShortcuts interface address. " +
                                "Aborting initialization.");
                return false;
            }

            try
            {
                Settings.VTables.GetVTable(ClientShortcutsInterfaceName).Attach(clientShortcutsInterfacePtr);
            }
            catch
            {
                Logger.Error("Failed to initialize ClientShortcuts interface!");
                return false;
            }

            Logger.Info("ClientShortcuts initialization succeeded!");
            return true;
        }


        /// <summary>
        /// Uses the vtable index stored in VTablesInfo to retrieve a ptr to the 'GetIClientShortcuts' vtable entry in
        /// the 'IClientEngine' vtable.
        /// </summary>
        /// <param name="clientEngineVtablePtr">The address of the IClientEngine vtable.</param>
        /// <returns>If found, a pointer to the IClientShortcuts interface; otherwise, IntPtr.Zero.</returns>
        private IntPtr GetShortcutsInterfacePtr(IntPtr clientEngineVtablePtr)
        {
            try
            {
                var clientShortcutsInterfacePtr = (IntPtr)Settings.VTables.
                    GetVtEntry(ClientEngineInterfaceName, GetIClientShortcutsName).
                    Invoke(User, Pipe, Settings.VTables.GetVTable(ClientShortcutsInterfaceName).InterfaceVersion);

                return clientShortcutsInterfacePtr;
            }
            catch
            {
                // suppress exception
            }

            Logger.Error($"Failed to retrieve a valid pointer to {GetIClientShortcutsName}.");
            return IntPtr.Zero;
        }

        #endregion Steam Client Library Initialization

        #region Cleanup

        private bool _disposed = false;

        ~SteamContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {

                }

                Logger.Info($"Disposing {nameof(SteamContext)} unmanaged resources.");
                ReleaseIpc();
                ResetInit();
                _disposed = true;
            }
            else
            {

            }
        }

        private void ReleaseIpc()
        {
            if (IsValidUser(User))
            {
                if (_callReleaseUser != null)
                    ReleaseUser(Pipe, User);

                if (_callBReleaseSteamPipe != null)
                    BReleaseSteamPipe(Pipe);
            }

            Pipe = User = 0;
        }

        #endregion Cleanup
    }
}
