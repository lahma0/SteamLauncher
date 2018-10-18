using SteamLauncher.SteamClient.Attributes;
using SteamLauncher.SteamClient.Interop;
using System;
using System.Runtime.InteropServices;
using SteamLauncher.Tools;

namespace SteamLauncher.SteamClient.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// Contains IClientShortcuts ('CLIENTSHORTCUTS_INTERFACE_VERSION001') delegates which correspond to their native SteamClient DLL functions.
    /// </summary>
    public class IClientShortcuts : SteamInterfaceWrapper
    {
        public IClientShortcuts(IntPtr interfacePtr) : base(interfacePtr) { }

        #region VTableIndex(0)
        [VTableIndex(0), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetUniqueLocalAppIdDelegate(IntPtr thisPtr);
        #endregion
        public UInt32 GetUniqueLocalAppId() => GetDelegate<GetUniqueLocalAppIdDelegate>()(InterfacePtr);

        #region VTableIndex(3)
        [VTableIndex(3), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetShortcutCountDelegate(IntPtr thisPtr);
        #endregion
        public UInt32 GetShortcutCount() => GetDelegate<GetShortcutCountDelegate>()(InterfacePtr);

        #region VTableIndex(4)
        [VTableIndex(4), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetShortcutAppIdByIndexDelegate(IntPtr thisPtr, UInt32 index);
        #endregion
        public UInt32 GetShortcutAppIdByIndex(UInt32 index) => GetDelegate<GetShortcutAppIdByIndexDelegate>()(InterfacePtr, index);

        #region VTableIndex(5)
        [VTableIndex(5), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetShortcutAppNameByIndexDelegate(IntPtr thisPtr, UInt32 index);
        #endregion
        public string GetShortcutAppNameByIndex(UInt32 index) => DecodeUtf8String(GetDelegate<GetShortcutAppNameByIndexDelegate>()(InterfacePtr, index));

        #region VTableIndex(6)
        [VTableIndex(6), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetShortcutExeByIndexDelegate(IntPtr thisPtr, UInt32 index);
        #endregion
        public string GetShortcutExeByIndex(UInt32 index) => DecodeUtf8String(GetDelegate<GetShortcutExeByIndexDelegate>()(InterfacePtr, index));

        #region VTableIndex(15)
        [VTableIndex(15), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetShortcutAppNameByAppIdDelegate(IntPtr thisPtr, UInt32 unAppId);
        #endregion
        public string GetShortcutAppNameByAppId(UInt32 unAppId) => DecodeUtf8String(GetDelegate<GetShortcutAppNameByAppIdDelegate>()(InterfacePtr, unAppId));

        #region VTableIndex(16)
        [VTableIndex(16), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetShortcutExeByAppIdDelegate(IntPtr thisPtr, UInt32 unAppId);
        #endregion
        public string GetShortcutExeByAppId(UInt32 unAppId) => DecodeUtf8String(GetDelegate<GetShortcutExeByAppIdDelegate>()(InterfacePtr, unAppId));

        #region VTableIndex(17)
        [VTableIndex(17), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetShortcutStartDirByAppIdDelegate(IntPtr thisPtr, UInt32 unAppId);
        #endregion
        public string GetShortcutStartDirByAppId(UInt32 unAppId) => DecodeUtf8String(GetDelegate<GetShortcutStartDirByAppIdDelegate>()(InterfacePtr, unAppId));

        #region VTableIndex(18)
        [VTableIndex(18), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetShortcutIconByAppIdDelegate(IntPtr thisPtr, UInt32 unAppId);
        #endregion
        public string GetShortcutIconByAppId(UInt32 unAppId) => DecodeUtf8String(GetDelegate<GetShortcutIconByAppIdDelegate>()(InterfacePtr, unAppId));

        #region VTableIndex(19)
        [VTableIndex(19), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetShortcutPathByAppIdDelegate(IntPtr thisPtr, UInt32 unAppId);
        #endregion
        public string GetShortcutPathByAppId(UInt32 unAppId) => DecodeUtf8String(GetDelegate<GetShortcutPathByAppIdDelegate>()(InterfacePtr, unAppId));

        #region VTableIndex(20)
        [VTableIndex(20), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetShortcutCommandLineDelegate(IntPtr thisPtr, UInt32 unAppId);
        #endregion
        public string GetShortcutCommandLine(UInt32 unAppId) => DecodeUtf8String(GetDelegate<GetShortcutCommandLineDelegate>()(InterfacePtr, unAppId));

        #region VTableIndex(21)
        [VTableIndex(21), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetShortcutUserTagCountByAppIdDelegate(IntPtr thisPtr, UInt32 unAppId);
        #endregion
        public UInt32 GetShortcutUserTagCountByAppId(UInt32 unAppId) => GetDelegate<GetShortcutUserTagCountByAppIdDelegate>()(InterfacePtr, unAppId);

        #region VTableIndex(22)
        [VTableIndex(22), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetShortcutUserTagByAppIdDelegate(IntPtr thisPtr, UInt32 unAppId, UInt32 unTagNum);
        #endregion
        public string GetShortcutUserTagByAppId(UInt32 unAppId, UInt32 unTagNum) => 
            DecodeUtf8String(GetDelegate<GetShortcutUserTagByAppIdDelegate>()(InterfacePtr, unAppId, unTagNum));

        #region VTableIndex(24)
        [VTableIndex(24), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BIsShortcutHiddenByAppIdDelegate(IntPtr thisPtr, UInt32 unAppId);
        #endregion
        public bool BIsShortcutHiddenByAppId(UInt32 unAppId) => GetDelegate<BIsShortcutHiddenByAppIdDelegate>()(InterfacePtr, unAppId);

        #region VTableIndex(29)
        [VTableIndex(29), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate UInt32 GetShortcutLastPlayedTimeDelegate(IntPtr thisPtr, UInt32 unAppId);
        #endregion
        public UInt32 GetShortcutLastPlayedTime(UInt32 unAppId) => GetDelegate<GetShortcutLastPlayedTimeDelegate>()(InterfacePtr, unAppId);

        #region VTableIndex(30)
        [VTableIndex(30), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate UInt32 AddShortcutDelegate(IntPtr thisPtr, string szAppName, string szExePath, string szIconPath, string szUnknown1, string szCommandLine);
        // private delegate UInt32 AddShortcutDelegate(IntPtr thisPtr, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8StringMarshaler))] string szAppName, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8StringMarshaler))] string szExePath, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8StringMarshaler))] string szIconPath, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8StringMarshaler))] string szUnknown1, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8StringMarshaler))] string szCommandLine);
        #endregion
        public UInt32 AddShortcut(string szAppName, string szExePath, string szIconPath = "", string szUnknown1 = "", string szCommandLine = "") =>
            GetDelegate<AddShortcutDelegate>()(InterfacePtr, szAppName.ToUtf8(), szExePath.ToUtf8(), szIconPath.ToUtf8(), szUnknown1.ToUtf8(), szCommandLine.ToUtf8());

        #region VTableIndex(31)
        [VTableIndex(31), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate UInt32 AddTemporaryShortcutDelegate(IntPtr thisPtr, string szAppName, string szExePath, string szIconPath);
        #endregion
        public UInt32 AddTemporaryShortcut(string szAppName, string szExePath, string szIconPath) =>
            GetDelegate<AddTemporaryShortcutDelegate>()(InterfacePtr, szAppName.ToUtf8(), szExePath.ToUtf8(), szIconPath.ToUtf8());

        #region VTableIndex(33)
        [VTableIndex(33), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate void SetShortcutFromFullPathDelegate(IntPtr thisPtr, UInt32 unAppId, string szPath);
        #endregion
        public void SetShortcutFromFullPath(UInt32 unAppId, string szPath) => GetDelegate<SetShortcutFromFullPathDelegate>()(InterfacePtr, unAppId, szPath.ToUtf8());

        #region VTableIndex(34)
        [VTableIndex(34), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate void SetShortcutAppNameDelegate(IntPtr thisPtr, UInt32 unAppId, string szAppName);
        #endregion
        public void SetShortcutAppName(UInt32 unAppId, string szAppName) => GetDelegate<SetShortcutAppNameDelegate>()(InterfacePtr, unAppId, szAppName.ToUtf8());

        #region VTableIndex(35)
        [VTableIndex(35), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate void SetShortcutExeDelegate(IntPtr thisPtr, UInt32 unAppId, string szExePath);
        #endregion
        public void SetShortcutExe(UInt32 unAppId, string szExePath) => GetDelegate<SetShortcutExeDelegate>()(InterfacePtr, unAppId, szExePath.ToUtf8());

        #region VTableIndex(36)
        [VTableIndex(36), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate void SetShortcutStartDirDelegate(IntPtr thisPtr, UInt32 unAppId, string szPath);
        #endregion
        public void SetShortcutStartDir(UInt32 unAppId, string szPath) => GetDelegate<SetShortcutStartDirDelegate>()(InterfacePtr, unAppId, szPath.ToUtf8());

        #region VTableIndex(37)
        [VTableIndex(37), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate void SetShortcutIconDelegate(IntPtr thisPtr, UInt32 unAppId, string szIconPath);
        #endregion
        public void SetShortcutIcon(UInt32 unAppId, string szIconPath) => GetDelegate<SetShortcutIconDelegate>()(InterfacePtr, unAppId, szIconPath.ToUtf8());

        #region VTableIndex(38)
        [VTableIndex(38), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate void SetShortcutCommandLineDelegate(IntPtr thisPtr, UInt32 unAppId, string szCommandLine);
        #endregion
        public void SetShortcutCommandLine(UInt32 unAppId, string szCommandLine) =>
            GetDelegate<SetShortcutCommandLineDelegate>()(InterfacePtr, unAppId, szCommandLine.ToUtf8());

        #region VTableIndex(39)
        [VTableIndex(39), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void ClearShortcutUserTagsDelegate(IntPtr thisPtr, UInt32 unAppId);
        #endregion
        public void ClearShortcutUserTags(UInt32 unAppId) => GetDelegate<ClearShortcutUserTagsDelegate>()(InterfacePtr, unAppId);

        #region VTableIndex(40)
        [VTableIndex(40), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate void AddShortcutUserTagDelegate(IntPtr thisPtr, UInt32 unAppId, string szTag);
        #endregion
        public void AddShortcutUserTag(UInt32 unAppId, string szTag) => GetDelegate<AddShortcutUserTagDelegate>()(InterfacePtr, unAppId, szTag.ToUtf8());

        #region VTableIndex(41)
        [VTableIndex(41), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate void RemoveShortcutUserTagDelegate(IntPtr thisPtr, UInt32 unAppId, string szTag);
        #endregion
        public void RemoveShortcutUserTag(UInt32 unAppId, string szTag) => GetDelegate<RemoveShortcutUserTagDelegate>()(InterfacePtr, unAppId, szTag.ToUtf8());

        #region VTableIndex(42)
        [VTableIndex(42), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void SetShortcutHiddenDelegate(IntPtr thisPtr, UInt32 unAppId, bool arg1);
        #endregion
        public void SetShortcutHidden(UInt32 unAppId, bool arg1) => GetDelegate<SetShortcutHiddenDelegate>()(InterfacePtr, unAppId, arg1);

        #region VTableIndex(43)
        [VTableIndex(43), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void SetAllowDesktopConfigDelegate(IntPtr thisPtr, UInt32 unAppId, bool arg1);
        #endregion
        public void SetAllowDesktopConfig(UInt32 unAppId, bool arg1) => GetDelegate<SetAllowDesktopConfigDelegate>()(InterfacePtr, unAppId, arg1);

        #region VTableIndex(44)
        [VTableIndex(44), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void SetAllowOverlayDelegate(IntPtr thisPtr, UInt32 unAppId, bool arg1);
        #endregion
        public void SetAllowOverlay(UInt32 unAppId, bool arg1) => GetDelegate<SetAllowOverlayDelegate>()(InterfacePtr, unAppId, arg1);
        
        #region VTableIndex(47)
        [VTableIndex(47), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void RemoveShortcutDelegate(IntPtr thisPtr, UInt32 unAppId);
        #endregion
        public void RemoveShortcut(UInt32 unAppId) => GetDelegate<RemoveShortcutDelegate>()(InterfacePtr, unAppId);

        #region VTableIndex(48)
        [VTableIndex(48), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void RemoveAllTemporaryShortcutsDelegate(IntPtr thisPtr);
        #endregion
        public void RemoveAllTemporaryShortcuts() => GetDelegate<RemoveAllTemporaryShortcutsDelegate>()(InterfacePtr);

        #region VTableIndex(49)
        [VTableIndex(49), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void LaunchShortcutDelegate(IntPtr thisPtr, UInt32 unAppId);
        #endregion
        public void LaunchShortcut(UInt32 unAppId) => GetDelegate<LaunchShortcutDelegate>()(InterfacePtr, unAppId);
    }
}
