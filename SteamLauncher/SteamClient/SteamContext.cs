using SteamLauncher.SteamClient.Interfaces;
using SteamLauncher.SteamClient.Native;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SteamLauncher.Logging;

namespace SteamLauncher.SteamClient
{
    public sealed class SteamContext
    {
        // non-beta Steam offset for 'GetIClientShortcuts'
        private readonly int _vTableOffsetGetIClientShortcuts = 52 * IntPtr.Size;

        // beta Steam offset for 'GetIClientShortcuts'
        private readonly int _betaVtableOffsetGetIClientShortcuts = 53 * IntPtr.Size;

        // non-beta ClientEngine vtable length
        private const int CLIENTENGINE_VTABLE_LEN = 71;

        // beta ClientEngine vtable length
        private const int BETA_CLIENTENGINE_VTABLE_LEN = 72;

        #region Unmanaged Pointers

        /// <summary>
        /// A handle to the Steam Client DLL.
        /// </summary>
        private IntPtr SteamClientDllHandle { get; set; } = IntPtr.Zero;

        /// <summary>
        /// A pointer to the IClientEngine interface.
        /// </summary>
        private IntPtr ClientEngineInterfacePtr { get; set; } = IntPtr.Zero;

        /// <summary>
        /// A pointer to the IClientShortcuts interface.
        /// </summary>
        private IntPtr ClientShortcutsInterfacePtr { get; set; } = IntPtr.Zero;

        private IClientShortcuts _clientShortcuts = null;

        public IClientShortcuts ClientShortcuts
        {
            get
            {
                // Checks if Steam has been reloaded since the plugin was initialized. If so, sets flag for reinitialization.
                if (_clientShortcuts != null)
                {
                    var currentSteamPid = SteamProcessInfo.SteamProcess.Id;
                    if (currentSteamPid != LastKnownSteamPid)
                    {
                        Logger.Warning("Steam interfaces needs to be reinitialized because Steam was closed and/or restarted. " + 
                                       $"Current Steam PID: {currentSteamPid}; Last Known Steam PID: {LastKnownSteamPid}");
                        ResetInit();
                    }
                }

                if (_clientShortcuts == null)
                {
                    Logger.Info($"Initializing {nameof(ClientShortcuts)}...");
                    if (!IsLoaded(ClientShortcutsInterfacePtr) && !InitClientShortcutsInterface())
                        return null;

                    _clientShortcuts = new IClientShortcuts(ClientShortcutsInterfacePtr);
                    LastKnownSteamPid = SteamProcessInfo.GetSteamPid();
                    Logger.Info($"Setting {nameof(LastKnownSteamPid)}: {LastKnownSteamPid}");
                }

                return _clientShortcuts;
            }
        }

        ///// <summary>
        ///// A pointer to the IClientShortcuts virtual function table.
        ///// </summary>
        //private IntPtr ClientShortcutsVTable { get; set; } = IntPtr.Zero;

        //// SteamClient interface is not used for anything right now, so this is not currently needed
        //private IntPtr SteamClientInterface { get; set; } = IntPtr.Zero;

        #endregion

        #region Other Properties

        private int Pipe { get; set; } = 0;

        private int User { get; set; } = 0;

        private int LastKnownSteamPid { get; set; } = 0;

        #endregion
        
        #region Singleton Constructor/Destructor

        private static readonly Lazy<SteamContext> lazy = new Lazy<SteamContext>(() => new SteamContext());

        public static SteamContext Instance => lazy.Value;

        private SteamContext()
        {
            Logger.Info($"Instantiating {nameof(SteamContext)} singleton...");
        }

        ~SteamContext()
        {
            Logger.Info($"Executing {nameof(SteamContext)} destructor...");
            ReleaseIpc();
        }

        #endregion

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

        /// <summary>
        /// An instance of the <see cref="SteamNative.GetIClientShortcuts"/> delegate.
        /// </summary>
        private SteamNative.GetIClientShortcuts _callGetIClientShortcuts;

        /// <summary>
        /// Retrieves an unmanaged pointer to the 'GetIClientShortcuts' method inside of the ClientEngine virtual function table.
        /// </summary>
        /// <param name="user">An int defining a connection to an existing global Steam user.</param>
        /// <param name="pipe">The Steam communication pipe acquired by calling <see cref="CreateSteamPipe"/>.</param>
        /// <returns>An unmanaged ptr to ClientEngine interface method 'GetIClientShortcuts', or 0 upon failure.</returns>
        private IntPtr GetIClientShortcuts(int user, int pipe)
        {
            if (_callGetIClientShortcuts == null || ClientEngineInterfacePtr == IntPtr.Zero)
                throw new InvalidOperationException($"Steam Client library is not initialized ({nameof(GetIClientShortcuts)})");

            return _callGetIClientShortcuts(ClientEngineInterfacePtr, user, pipe, "CLIENTSHORTCUTS_INTERFACE_VERSION001");
        }

        #endregion

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

        private void ResetInit()
        {
            SteamClientDllHandle = IntPtr.Zero;
            ClientEngineInterfacePtr = IntPtr.Zero;
            ClientShortcutsInterfacePtr = IntPtr.Zero;
            _clientShortcuts = null;
            //ClientShortcutsVTable = IntPtr.Zero;
            //SteamClientInterface = IntPtr.Zero;

            _callCreateInterface = null;
            _callCreateSteamPipe = null;
            _callBReleaseSteamPipe = null;
            _callConnectToGlobalUser = null;
            _callReleaseUser = null;

            User = 0;
            Pipe = 0;
        }

        #endregion

        #region Steam Client Library Initialization

        private bool InitSteam()
        {
            if (SteamProcessInfo.SteamProcess == null)
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
                Logger.Error("Failed to load the Steam Client DLL. Aborting initialization.", 1);
                return false;
            }

            _callCreateInterface = SysNative.GetExportFunction<SteamNative.CreateInterface>(clientDllHandle, "CreateInterface");
            if (_callCreateInterface == null)
            {
                Logger.Error("Failed to retrieve the 'CreateInterface' export function. Aborting initialization.", 1);
                return false;
            }

            _callCreateSteamPipe = SysNative.GetExportFunction<SteamNative.CreateSteamPipe>(clientDllHandle, "Steam_CreateSteamPipe");
            if (_callCreateSteamPipe == null)
            {
                Logger.Error("Failed to retrieve the 'Steam_CreateSteamPipe' export function. Aborting initialization.", 1);
                return false;
            }

            _callBReleaseSteamPipe = SysNative.GetExportFunction<SteamNative.BReleaseSteamPipe>(clientDllHandle, "Steam_BReleaseSteamPipe");
            if (_callBReleaseSteamPipe == null)
            {
                Logger.Error("Failed to retrieve the 'Steam_BReleaseSteamPipe' export function. Aborting initialization.", 1);
                return false;
            }

            _callConnectToGlobalUser = SysNative.GetExportFunction<SteamNative.ConnectToGlobalUser>(clientDllHandle, "Steam_ConnectToGlobalUser");
            if (_callConnectToGlobalUser == null)
            {
                Logger.Error("Failed to retrieve the 'Steam_ConnectToGlobalUser' export function. Aborting initialization.", 1);
                return false;
            }

            _callReleaseUser = SysNative.GetExportFunction<SteamNative.ReleaseUser>(clientDllHandle, "Steam_ReleaseUser");
            if (_callReleaseUser == null)
            {
                Logger.Error("Failed to retrieve the 'Steam_ReleaseUser' export function. Aborting initialization.", 1);
                return false;
            }

            Pipe = CreateSteamPipe();
            if (!IsValidPipe(Pipe))
            {
                Logger.Error("Failed to create a Steam pipe (IPC). Aborting initialization.", 1);
                return false;
            }

            User = ConnectToGlobalUser(Pipe);
            if (!IsValidUser(User))
            {
                Logger.Error("Failed to connect to Steam global user (IPC). Aborting initialization.", 1);
                return false;
            }

            SteamClientDllHandle = clientDllHandle;
            Logger.Info("Steam initialization succeeded!", 1);

            return true;
        }

        private bool InitClientEngineInterface()
        {
            if (IsLoaded(ClientEngineInterfacePtr))
            {
                Logger.Info("ClientEngine is already initialized.");
                return true;
            }

            if (!InitSteam())
                return false;

            Logger.Info("Beginning initialization of the ClientEngine interface...");

            // Loop tries to be resilient against IClientEngine version # increases in future Steam updates
            // When the version # last changed, this CreateInterface call failed; a simple loop like this would have prevented that
            for (var i = 5; i < 9; i++)
            {
                var engineInterfacePtr = IntPtr.Zero;
                var engineVersion = $"CLIENTENGINE_INTERFACE_VERSION{i:D3}";
                try
                {
                    engineInterfacePtr = CreateInterface(engineVersion, IntPtr.Zero);
                }
                catch
                {
                    // suppress exception (invalid ClientEngine interface version)
                }

                if (!IsLoaded(engineInterfacePtr)) continue;

                Logger.Info($"ClientEngine initialization succeded! ({engineVersion})", 1);
                ClientEngineInterfacePtr = engineInterfacePtr;

                return true;
            }

            Logger.Error("Failed to create the ClientEngine interface. Aborting initialization.", 1);
            return false;
        }

        private bool InitClientShortcutsInterface()
        {
            if (IsLoaded(ClientShortcutsInterfacePtr))
            {
                Logger.Info("ClientShortcuts is already initialized.");
                return true;
            }

            if (!InitClientEngineInterface())
                return false;

            Logger.Info("Beginning initialization of the ClientShortcuts interface...");

            var engineInterfaceAddr = Marshal.ReadIntPtr(ClientEngineInterfacePtr);
            if (!IsLoaded(engineInterfaceAddr))
            {
                Logger.Error("Failed to retrieve a valid ClientEngine interface address. Aborting initialization.", 1);
                return false;
            }

            var vTableOffsetGetIClientShortcuts = _vTableOffsetGetIClientShortcuts;

            // Steam beta update in Feb 2018 added 1 entry to the ClientEngine vtable; this sloppy code checks for a string 
            // always present following the vtable; if the string is found at (lenOfBetaVTable + 1), it uses the beta offset, 
            // otherwise, it behaves normally
            var postVtableBetaOffset = IntPtr.Size * (BETA_CLIENTENGINE_VTABLE_LEN + 1);
            var numOfBytesToRead = 4;
            var postVTableBytes = new byte[numOfBytesToRead];
            Marshal.Copy(engineInterfaceAddr + postVtableBetaOffset, postVTableBytes, 0, numOfBytesToRead);
            if (postVTableBytes.SequenceEqual(new byte[] {0x6d, 0x5f, 0x76, 0x65}))
            {
                vTableOffsetGetIClientShortcuts = _betaVtableOffsetGetIClientShortcuts;
                Logger.Warning("Steam beta client detected. Using modified 'GetIClientShortcuts' offset.", 1);
            }

            var getIClientShortcutsAddr = Marshal.ReadIntPtr(engineInterfaceAddr, vTableOffsetGetIClientShortcuts);
            if (!IsLoaded(getIClientShortcutsAddr))
            {
                Logger.Error("Failed to retrieve a valid ClientShortcuts interface address. Aborting initialization.", 1);
                return false;
            }

            _callGetIClientShortcuts = Marshal.GetDelegateForFunctionPointer<SteamNative.GetIClientShortcuts>(getIClientShortcutsAddr);
            if (_callGetIClientShortcuts == null)
            {
                Logger.Error("Failed to retrieve a valid delegate for the GetIClientShortcuts function pointer. Aborting initialization.", 1);
                return false;
            }

            var shortcutsInterfacePtr = GetIClientShortcuts(User, Pipe);
            if (!IsLoaded(shortcutsInterfacePtr))
            {
                Logger.Error("The call to the native function GetIClientShortcuts failed. Aborting initialization.", 1);
                return false;
            }

            ClientShortcutsInterfacePtr = shortcutsInterfacePtr;

            Logger.Info("ClientShortcuts initialization succeeded!", 1);

            return true;
        }

        #endregion
        
        #region Unused/Unneeded Stuff

        //// SteamClient interface is not used for anything right now, so this is not currently needed
        //private bool InitISteamClient()
        //{
        //    if (!InitSteam())
        //        return false;

        //    if (IsLoaded(SteamClientInterface))
        //        return true;

        //    SteamClientInterface = CreateInterface("SteamClient017", IntPtr.Zero);
        //    if (!IsLoaded(SteamClientInterface))
        //        return false;

        //    return true;
        //}

        #endregion
        
        #region Cleanup

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

        #endregion
    }
}
