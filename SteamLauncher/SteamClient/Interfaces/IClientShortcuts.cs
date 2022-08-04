using SteamLauncher.SteamClient.Attributes;
using SteamLauncher.SteamClient.Interop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SteamLauncher.SteamClient.Interfaces.CustTypes;
using SteamLauncher.Tools;

namespace SteamLauncher.SteamClient.Interfaces
{
    ///// <inheritdoc />
    ///// <summary>
    ///// Contains IClientShortcuts ('CLIENTSHORTCUTS_INTERFACE_VERSION001') delegates which correspond to their native
    ///// SteamClient DLL functions.
    ///// </summary>
    //public class IClientShortcuts : SteamInterfaceWrapper
    //{
    //    public IClientShortcuts(IntPtr interfacePtr, bool useOldVtableIndex=false) : 
    //        base(interfacePtr, useOldVtableIndex) { }

    //    #region VTableIndex(0)
    //    [VTableIndex(0), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate UInt32 GetUniqueLocalAppIdDelegate(IntPtr thisPtr);
    //    #endregion
    //    public UInt32 GetUniqueLocalAppId() => GetDelegate<GetUniqueLocalAppIdDelegate>()(InterfacePtr);

    //    #region VTableIndex(1)
    //    [VTableIndex(1), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate void GetGameIdForAppIdDelegate(IntPtr thisPtr, ref UInt64 retValue, UInt32 unAppId);
    //    #endregion
    //    public CGameID GetGameIdForAppId(UInt32 unAppId)
    //    {
    //        UInt64 retValue = 0;
    //        GetDelegate<GetGameIdForAppIdDelegate>()(InterfacePtr, ref retValue, unAppId);
    //        return new CGameID(retValue);
    //    }

    //    #region VTableIndex(2)
    //    [VTableIndex(2), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate UInt32 GetAppIdForGameIdDelegate(IntPtr thisPtr, UInt64 gameId);
    //    #endregion
    //    public UInt32 GetAppIdForGameId(CGameID gameId) =>
    //        GetDelegate<GetAppIdForGameIdDelegate>()(InterfacePtr, gameId.ConvertToUint64());

    //    #region VTableIndex(3)
    //    [VTableIndex(3), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate UInt32 GetShortcutCountDelegate(IntPtr thisPtr);
    //    #endregion
    //    public UInt32 GetShortcutCount() => GetDelegate<GetShortcutCountDelegate>()(InterfacePtr);

    //    #region VTableIndex(4)
    //    [VTableIndex(4), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate UInt32 GetShortcutAppIdByIndexDelegate(IntPtr thisPtr, UInt32 index);
    //    #endregion
    //    public UInt32 GetShortcutAppIdByIndex(UInt32 index) =>
    //        GetDelegate<GetShortcutAppIdByIndexDelegate>()(InterfacePtr, index);

    //    #region VTableIndex(5)
    //    [VTableIndex(5), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate IntPtr GetShortcutAppNameByIndexDelegate(IntPtr thisPtr, UInt32 index);
    //    #endregion
    //    public string GetShortcutAppNameByIndex(UInt32 index) =>
    //        Marshal.PtrToStringUTF8(GetDelegate<GetShortcutAppNameByIndexDelegate>()(InterfacePtr, index));

    //    #region VTableIndex(6)
    //    [VTableIndex(6), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate IntPtr GetShortcutExeByIndexDelegate(IntPtr thisPtr, UInt32 index);
    //    #endregion
    //    public string GetShortcutExeByIndex(UInt32 index) =>
    //        Marshal.PtrToStringUTF8(GetDelegate<GetShortcutExeByIndexDelegate>()(InterfacePtr, index));

    //    #region VTableIndex(7)
    //    [VTableIndex(7), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate UInt32 GetShortcutUserTagCountByIndexDelegate(IntPtr thisPtr, UInt32 index);
    //    #endregion
    //    /// <summary>
    //    /// Gets the number of total user tags assigned to an existing Steam shortcut. <remarks><c>NOTE: Since the Sept
    //    /// 2019 Steam Library overhaul/redesign, this function will not appear to work correctly. While it will return
    //    /// the correct number of user tags that are stored in the shortcuts.vdf file, any modifications to user tags
    //    /// performed through the UI will not be seen by this cmd (as collection/tag names are no longer stored/queried
    //    /// from the shortcuts.vdf file).</c></remarks>
    //    /// </summary>
    //    /// <param name="index">The index of the Steam shortcut to query.</param>
    //    /// <returns>The total number of user tags assigned to the specified shortcut.</returns>
    //    public UInt32 GetShortcutUserTagCountByIndex(UInt32 index) =>
    //        GetDelegate<GetShortcutUserTagCountByIndexDelegate>()(InterfacePtr, index);

    //    #region VTableIndex(8)
    //    [VTableIndex(8), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate IntPtr GetShortcutUserTagByIndexDelegate(IntPtr thisPtr, 
    //                                                              UInt32 shortcutIndex, 
    //                                                              UInt32 tagIndex);
    //    #endregion
    //    /// <summary>
    //    /// Gets the user tag of a shortcut at the index specified by <paramref name="tagIndex"/>. <remarks><c>NOTE:
    //    /// Since the Sept 2019 Steam Library overhaul/redesign, this function will not appear to work correctly. While
    //    /// it will return the correct user tags as they are stored in the shortcuts.vdf file, any modifications to user
    //    /// tags performed through the UI will not be seen by this cmd (as collection/tag names are no longer
    //    /// stored/queried from the shortcuts.vdf file).</c></remarks>
    //    /// </summary>
    //    /// <param name="shortcutIndex">The index of the Steam shortcut to query.</param>
    //    /// <param name="tagIndex">The index of the user tag to lookup.</param>
    //    /// <returns>The user tag string.</returns>
    //    public string GetShortcutUserTagByIndex(UInt32 shortcutIndex, UInt32 tagIndex) =>
    //        Marshal.PtrToStringUTF8(
    //            GetDelegate<GetShortcutUserTagByIndexDelegate>()(InterfacePtr, shortcutIndex, tagIndex));

    //    #region VTableIndex(9)
    //    [VTableIndex(9), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate bool BIsShortcutRemoteByIndexDelegate(IntPtr thisPtr, UInt32 index);
    //    #endregion
    //    public bool BIsShortcutRemoteByIndex(UInt32 index) =>
    //        GetDelegate<BIsShortcutRemoteByIndexDelegate>()(InterfacePtr, index);

    //    #region VTableIndex(10)
    //    [VTableIndex(10), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate bool BIsTemporaryShortcutByIndexDelegate(IntPtr thisPtr, UInt32 index);
    //    #endregion
    //    public bool BIsTemporaryShortcutByIndex(UInt32 index) =>
    //        GetDelegate<BIsTemporaryShortcutByIndexDelegate>()(InterfacePtr, index);

    //    #region VTableIndex(11)
    //    [VTableIndex(11), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate bool BIsOpenVrShortcutByIndexDelegate(IntPtr thisPtr, UInt32 index);
    //    #endregion
    //    public bool BIsOpenVrShortcutByIndex(UInt32 index) =>
    //        GetDelegate<BIsOpenVrShortcutByIndexDelegate>()(InterfacePtr, index);

    //    #region VTableIndex(12)
    //    [VTableIndex(12), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate bool BIsDevkitShortcutByIndexDelegate(IntPtr thisPtr, UInt32 index);
    //    #endregion
    //    public bool BIsDevkitShortcutByIndex(UInt32 index) =>
    //        GetDelegate<BIsDevkitShortcutByIndexDelegate>()(InterfacePtr, index);

    //    #region VTableIndex(13)
    //    [VTableIndex(13), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate void GetDevkitGameIdByIndexDelegate(IntPtr thisPtr, ref UInt64 retValue, UInt32 index);
    //    #endregion
    //    public CGameID GetDevkitGameIdByIndex(UInt32 index)
    //    {
    //        UInt64 retValue = 0;
    //        GetDelegate<GetDevkitGameIdByIndexDelegate>()(InterfacePtr, ref retValue, index);
    //        return new CGameID(retValue);
    //    }

    //    #region VTableIndex(14)
    //    [VTableIndex(14), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate UInt32 GetDevkitAppIdByDevkitGameIdDelegate(IntPtr thisPtr, UInt64 devkitGameId);
    //    #endregion
    //    public UInt32 GetDevkitAppIdByDevkitGameId(CGameID devkitGameId) =>
    //        GetDelegate<GetDevkitAppIdByDevkitGameIdDelegate>()(InterfacePtr, devkitGameId.ConvertToUint64());

    //    #region VTableIndex(15)
    //    [VTableIndex(15), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate UInt32 GetOverrideAppIdDelegate(IntPtr thisPtr, UInt32 unAppId);
    //    #endregion
    //    public UInt32 GetOverrideAppId(UInt32 unAppId) =>
    //        GetDelegate<GetOverrideAppIdDelegate>()(InterfacePtr, unAppId);

    //    #region VTableIndex(16)
    //    [VTableIndex(16), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate IntPtr GetShortcutAppNameByAppIdDelegate(IntPtr thisPtr, UInt32 unAppId);
    //    #endregion
    //    public string GetShortcutAppNameByAppId(UInt32 unAppId) =>
    //        Marshal.PtrToStringUTF8(GetDelegate<GetShortcutAppNameByAppIdDelegate>()(InterfacePtr, unAppId));

    //    #region VTableIndex(17)
    //    [VTableIndex(17), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate IntPtr GetShortcutExeByAppIdDelegate(IntPtr thisPtr, UInt32 unAppId);
    //    #endregion
    //    public string GetShortcutExeByAppId(UInt32 unAppId) =>
    //        Marshal.PtrToStringUTF8(GetDelegate<GetShortcutExeByAppIdDelegate>()(InterfacePtr, unAppId));

    //    #region VTableIndex(18)
    //    [VTableIndex(18), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate IntPtr GetShortcutStartDirByAppIdDelegate(IntPtr thisPtr, UInt32 unAppId);
    //    #endregion
    //    public string GetShortcutStartDirByAppId(UInt32 unAppId) =>
    //        Marshal.PtrToStringUTF8(GetDelegate<GetShortcutStartDirByAppIdDelegate>()(InterfacePtr, unAppId));

    //    #region VTableIndex(19)
    //    [VTableIndex(19), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate IntPtr GetShortcutIconByAppIdDelegate(IntPtr thisPtr, UInt32 unAppId);
    //    #endregion
    //    public string GetShortcutIconByAppId(UInt32 unAppId) =>
    //        Marshal.PtrToStringUTF8(GetDelegate<GetShortcutIconByAppIdDelegate>()(InterfacePtr, unAppId));

    //    #region VTableIndex(20)
    //    [VTableIndex(20), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate IntPtr GetShortcutPathByAppIdDelegate(IntPtr thisPtr, UInt32 unAppId);
    //    #endregion
    //    public string GetShortcutPathByAppId(UInt32 unAppId) =>
    //        Marshal.PtrToStringUTF8(GetDelegate<GetShortcutPathByAppIdDelegate>()(InterfacePtr, unAppId));

    //    #region VTableIndex(21)
    //    [VTableIndex(21), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate IntPtr GetShortcutCommandLineDelegate(IntPtr thisPtr, UInt32 unAppId);
    //    #endregion
    //    public string GetShortcutCommandLine(UInt32 unAppId) =>
    //        Marshal.PtrToStringUTF8(GetDelegate<GetShortcutCommandLineDelegate>()(InterfacePtr, unAppId));

    //    #region VTableIndex(22)
    //    [VTableIndex(22), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate UInt32 GetShortcutUserTagCountByAppIdDelegate(IntPtr thisPtr, UInt32 unAppId);
    //    #endregion
    //    /// <summary>
    //    /// Gets the number of total user tags assigned to an existing Steam shortcut. <remarks><c>NOTE: Since the Sept
    //    /// 2019 Steam Library overhaul/redesign, this function will not appear to work correctly. While it will return
    //    /// the correct number of user tags that are stored in the shortcuts.vdf file, any modifications to user tags
    //    /// performed through the UI will not be seen by this cmd (as collection/tag names are no longer stored/queried
    //    /// from the shortcuts.vdf file).</c></remarks>
    //    /// </summary>
    //    /// <param name="unAppId">The AppID of the Steam shortcut to query.</param>
    //    /// <returns>The total number of user tags assigned to the specified shortcut.</returns>
    //    public UInt32 GetShortcutUserTagCountByAppId(UInt32 unAppId) =>
    //        GetDelegate<GetShortcutUserTagCountByAppIdDelegate>()(InterfacePtr, unAppId);

    //    #region VTableIndex(23)
    //    [VTableIndex(23), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate IntPtr GetShortcutUserTagByAppIdDelegate(IntPtr thisPtr, UInt32 unAppId, UInt32 unTagNum);
    //    #endregion
    //    /// <summary>
    //    /// Gets the user tag of a shortcut by the tag number specified by <paramref name="unTagNum"/>.
    //    /// <remarks><c>NOTE: Since the Sept 2019 Steam Library overhaul/redesign, this function will not appear to work
    //    /// correctly. While it will return the correct user tags as they are stored in the shortcuts.vdf file, any
    //    /// modifications to user tags performed through the UI will not be seen by this cmd (as collection/tag names
    //    /// are no longer stored/queried from the shortcuts.vdf file).</c></remarks>
    //    /// </summary>
    //    /// <param name="unAppId">The AppID of the Steam shortcut to query.</param>
    //    /// <param name="unTagNum">The number of the user tag to lookup.</param>
    //    /// <returns>The user tag string.</returns>
    //    public string GetShortcutUserTagByAppId(UInt32 unAppId, UInt32 unTagNum) => 
    //        Marshal.PtrToStringUTF8(GetDelegate<GetShortcutUserTagByAppIdDelegate>()(InterfacePtr, unAppId, unTagNum));

    //    #region VTableIndex(24)
    //    [VTableIndex(24), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate bool BIsShortcutRemoteByAppIdDelegate(IntPtr thisPtr, UInt32 unAppId);
    //    #endregion
    //    public bool BIsShortcutRemoteByAppId(UInt32 unAppId) =>
    //        GetDelegate<BIsShortcutRemoteByAppIdDelegate>()(InterfacePtr, unAppId);

    //    #region VTableIndex(25)
    //    [VTableIndex(25), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate bool BIsShortcutHiddenByAppIdDelegate(IntPtr thisPtr, UInt32 unAppId);
    //    #endregion
    //    /// <summary>
    //    /// Checks if an existing Steam shortcut is hidden. <remarks><c>NOTE: Since the Sept 2019 Steam Library
    //    /// overhaul/redesign, this function always returns a value of True, regardless of whether the shortcut has been
    //    /// hidden using the API or UI. This makes this function worthless until it is fixed by Valve.</c></remarks>
    //    /// </summary>
    //    /// <param name="unAppId">The AppID of the Steam shortcut to query.</param>
    //    /// <returns>True if hidden; False if NOT hidden.</returns>
    //    public bool BIsShortcutHiddenByAppId(UInt32 unAppId) =>
    //        GetDelegate<BIsShortcutHiddenByAppIdDelegate>()(InterfacePtr, unAppId);

    //    #region VTableIndex(26)
    //    [VTableIndex(26), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate bool BIsTemporaryShortcutByAppIdDelegate(IntPtr thisPtr, UInt32 unAppId);
    //    #endregion
    //    public bool BIsTemporaryShortcutByAppId(UInt32 unAppId) =>
    //        GetDelegate<BIsTemporaryShortcutByAppIdDelegate>()(InterfacePtr, unAppId);

    //    #region VTableIndex(27)
    //    [VTableIndex(27), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate bool BIsOpenVrShortcutByAppIdDelegate(IntPtr thisPtr, UInt32 unAppId);
    //    #endregion
    //    public bool BIsOpenVrShortcutByAppId(UInt32 unAppId) =>
    //        GetDelegate<BIsOpenVrShortcutByAppIdDelegate>()(InterfacePtr, unAppId);

    //    #region VTableIndex(28)
    //    [VTableIndex(28), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate bool BAllowDesktopConfigByAppIdDelegate(IntPtr thisPtr, UInt32 unAppId);
    //    #endregion
    //    public bool BAllowDesktopConfigByAppId(UInt32 unAppId) =>
    //        GetDelegate<BAllowDesktopConfigByAppIdDelegate>()(InterfacePtr, unAppId);

    //    #region VTableIndex(29)
    //    [VTableIndex(29), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate bool BAllowOverlayByAppIdDelegate(IntPtr thisPtr, UInt32 unAppId);
    //    #endregion
    //    public bool BAllowOverlayByAppId(UInt32 unAppId) =>
    //        GetDelegate<BAllowOverlayByAppIdDelegate>()(InterfacePtr, unAppId);

    //    #region VTableIndex(30)
    //    [VTableIndex(30), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate UInt32 GetShortcutLastPlayedTimeDelegate(IntPtr thisPtr, UInt32 unAppId);
    //    #endregion
    //    public UInt32 GetShortcutLastPlayedTime(UInt32 unAppId) =>
    //        GetDelegate<GetShortcutLastPlayedTimeDelegate>()(InterfacePtr, unAppId);

    //    #region VTableIndex(31)
    //    [VTableIndex(31), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
    //    private delegate UInt32 AddShortcutDelegate(IntPtr thisPtr, 
    //                                                [MarshalAs(UnmanagedType.LPUTF8Str)] string szAppName, 
    //                                                [MarshalAs(UnmanagedType.LPUTF8Str)] string szExePath, 
    //                                                [MarshalAs(UnmanagedType.LPUTF8Str)] string szIconPath, 
    //                                                [MarshalAs(UnmanagedType.LPUTF8Str)] string szProbStartDir, 
    //                                                [MarshalAs(UnmanagedType.LPUTF8Str)] string szCommandLine);
    //    #endregion
    //    /// <summary>
    //    /// Adds a new shortcut to Steam and returns its AppID.
    //    /// </summary>
    //    /// <remarks>Paths should NOT be enclosed in double quotes (Steam adds them automatically).</remarks>
    //    /// <param name="szAppName">The shortcut name.</param>
    //    /// <param name="szExePath">The path to the shortcut's EXE.</param>
    //    /// <param name="szIconPath">The path of a PNG file to be used as the shortcut's icon.</param>
    //    /// <param name="szStartDir">The shortcut's startup/working directory.</param>
    //    /// <param name="szCommandLine">The command line parameters to be used when launching the EXE.</param>
    //    /// <returns>The AppID of the newly created shortcut.</returns>
    //    public UInt32 AddShortcut(string szAppName,
    //                              string szExePath,
    //                              string szIconPath = "",
    //                              string szStartDir = "",
    //                              string szCommandLine = "") =>
    //        GetDelegate<AddShortcutDelegate>()(InterfacePtr,
    //                                           szAppName,
    //                                           szExePath.Trim('"'),
    //                                           szIconPath.Trim('"'),
    //                                           szStartDir.Trim('"'),
    //                                           szCommandLine);

    //    #region VTableIndex(32)
    //    [VTableIndex(32), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
    //    private delegate UInt32 AddTemporaryShortcutDelegate(IntPtr thisPtr,
    //                                                         [MarshalAs(UnmanagedType.LPUTF8Str)] string szAppName,
    //                                                         [MarshalAs(UnmanagedType.LPUTF8Str)] string szExePath,
    //                                                         [MarshalAs(UnmanagedType.LPUTF8Str)] string szIconPath);
    //    #endregion
    //    /// <summary>
    //    /// Adds a new shortcut to Steam and returns its AppID.
    //    /// </summary>
    //    /// <remarks>Paths should NOT be enclosed in double quotes (Steam adds them automatically).</remarks>
    //    /// <param name="szAppName">The shortcut name.</param>
    //    /// <param name="szExePath">The path to the shortcut's EXE.</param>
    //    /// <param name="szIconPath">The path of a PNG file to be used as the shortcut's icon.</param>
    //    /// <returns>The AppID of the newly created shortcut.</returns>
    //    public UInt32 AddTemporaryShortcut(string szAppName, string szExePath, string szIconPath) =>
    //        GetDelegate<AddTemporaryShortcutDelegate>()(InterfacePtr,
    //                                                    szAppName,
    //                                                    szExePath.Trim('"'),
    //                                                    szIconPath.Trim('"'));

    //    #region VTableIndex(33)
    //    [VTableIndex(33), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
    //    private delegate UInt32 AddOpenVrShortcutDelegate(IntPtr thisPtr,
    //                                                         [MarshalAs(UnmanagedType.LPUTF8Str)] string szAppName,
    //                                                         [MarshalAs(UnmanagedType.LPUTF8Str)] string szExePath,
    //                                                         [MarshalAs(UnmanagedType.LPUTF8Str)] string szIconPath);
    //    #endregion
    //    public UInt32 AddOpenVrShortcut(string szAppName, string szExePath, string szIconPath) =>
    //        GetDelegate<AddOpenVrShortcutDelegate>()(InterfacePtr,
    //                                                 szAppName,
    //                                                 szExePath.Trim('"'),
    //                                                 szIconPath.Trim('"'));

    //    #region VTableIndex(34)
    //    [VTableIndex(34), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
    //    private delegate void SetShortcutFromFullPathDelegate(IntPtr thisPtr,
    //                                                          UInt32 unAppId,
    //                                                          [MarshalAs(UnmanagedType.LPUTF8Str)] string szPath);
    //    #endregion
    //    /// <summary>
    //    /// Sets the path to the EXE file for an existing Steam shortcut. The shortcut's 'Start Directory' will be set
    //    /// to the directory of the EXE file and the shortcut's icon will be set to the EXE's icon.
    //    /// </summary>
    //    /// <remarks>Paths should NOT be enclosed in double quotes (Steam adds them automatically).</remarks>
    //    /// <param name="unAppId">The AppID of the Steam shortcut to modify.</param>
    //    /// <param name="szPath">The path of the EXE file.</param>
    //    public void SetShortcutFromFullPath(UInt32 unAppId, string szPath) =>
    //        GetDelegate<SetShortcutFromFullPathDelegate>()(InterfacePtr, unAppId, szPath.Trim('"'));

    //    #region VTableIndex(35)
    //    [VTableIndex(35), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
    //    private delegate void SetShortcutAppNameDelegate(IntPtr thisPtr,
    //                                                     UInt32 unAppId,
    //                                                     [MarshalAs(UnmanagedType.LPUTF8Str)] string szAppName);
    //    #endregion
    //    public void SetShortcutAppName(UInt32 unAppId, string szAppName) =>
    //        GetDelegate<SetShortcutAppNameDelegate>()(InterfacePtr, unAppId, szAppName);

    //    #region VTableIndex(36)
    //    [VTableIndex(36), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
    //    private delegate void SetShortcutExeDelegate(IntPtr thisPtr,
    //                                                 UInt32 unAppId,
    //                                                 [MarshalAs(UnmanagedType.LPUTF8Str)] string szExePath);
    //    #endregion
    //    /// <summary>
    //    /// Sets the path to the EXE file for an existing Steam shortcut.
    //    /// </summary>
    //    /// <remarks>Paths with spaces must be enclosed in double quotes.</remarks>
    //    /// <param name="unAppId">The AppID of the Steam shortcut to modify.</param>
    //    /// <param name="szExePath">The path of the EXE file.</param>
    //    public void SetShortcutExe(UInt32 unAppId, string szExePath) =>
    //        GetDelegate<SetShortcutExeDelegate>()(InterfacePtr, unAppId, szExePath.InDblQuotes());

    //    #region VTableIndex(37)
    //    [VTableIndex(37), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
    //    private delegate void SetShortcutStartDirDelegate(IntPtr thisPtr,
    //                                                      UInt32 unAppId,
    //                                                      [MarshalAs(UnmanagedType.LPUTF8Str)] string szPath);
    //    #endregion
    //    /// <summary>
    //    /// Sets the working directory of a Steam shortcut.
    //    /// </summary>
    //    /// <remarks>Paths with spaces must be enclosed in double quotes.</remarks>
    //    /// <param name="unAppId">The AppID of the Steam shortcut to modify.</param>
    //    /// <param name="szPath">The path to be used as the new startup directory.</param>
    //    public void SetShortcutStartDir(UInt32 unAppId, string szPath) =>
    //        GetDelegate<SetShortcutStartDirDelegate>()(InterfacePtr, unAppId, szPath.InDblQuotes());

    //    #region VTableIndex(38)
    //    [VTableIndex(38), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
    //    private delegate void SetShortcutIconDelegate(IntPtr thisPtr,
    //                                                  UInt32 unAppId,
    //                                                  [MarshalAs(UnmanagedType.LPUTF8Str)] string szIconPath);
    //    #endregion
    //    /// <summary>
    //    /// Assigns a new icon to a Steam shortcut by providing a path to a PNG icon file.
    //    /// </summary>
    //    /// <remarks>Paths with spaces must be enclosed in double quotes.</remarks>
    //    /// <param name="unAppId">The AppID of the Steam shortcut to modify.</param>
    //    /// <param name="szIconPath">The path of a PNG file to be used as the new icon for the shortcut.</param>
    //    public void SetShortcutIcon(UInt32 unAppId, string szIconPath) =>
    //        GetDelegate<SetShortcutIconDelegate>()(InterfacePtr, unAppId, szIconPath.InDblQuotes());

    //    #region VTableIndex(39)
    //    [VTableIndex(39), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
    //    private delegate void SetShortcutCommandLineDelegate(IntPtr thisPtr,
    //                                                         UInt32 unAppId,
    //                                                         [MarshalAs(UnmanagedType.LPUTF8Str)] string szCommandLine);
    //    #endregion
    //    public void SetShortcutCommandLine(UInt32 unAppId, string szCommandLine) =>
    //        GetDelegate<SetShortcutCommandLineDelegate>()(InterfacePtr, unAppId, szCommandLine);

    //    #region VTableIndex(40)
    //    [VTableIndex(40), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate void ClearShortcutUserTagsDelegate(IntPtr thisPtr, UInt32 unAppId);
    //    #endregion
    //    /// <summary>
    //    /// Removes all existing user tags from a Steam shortcut. <remarks><c>NOTE: Since the Sept 2019 Steam Library
    //    /// overhaul/redesign, this function will not appear to work correctly. While it will correctly modify the
    //    /// shortcuts.vdf file, the Steam UI will not reflect the changes made.</c></remarks>
    //    /// </summary>
    //    /// <param name="unAppId">The AppID of the Steam shortcut to modify.</param>
    //    public void ClearShortcutUserTags(UInt32 unAppId) =>
    //        GetDelegate<ClearShortcutUserTagsDelegate>()(InterfacePtr, unAppId);
        
    //    #region VTableIndex(41)
    //    [VTableIndex(41), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
    //    private delegate void AddShortcutUserTagDelegate(IntPtr thisPtr,
    //                                                     UInt32 unAppId,
    //                                                     [MarshalAs(UnmanagedType.LPUTF8Str)] string szTag);
    //    #endregion
    //    /// <summary>
    //    /// Adds a new user tag to a Steam shortcut. <remarks><c>NOTE: Since the Sept 2019 Steam Library
    //    /// overhaul/redesign, this function will not appear to work correctly. While it will correctly modify the
    //    /// shortcuts.vdf file, the Steam UI will not reflect the changes made.</c></remarks>
    //    /// </summary>
    //    /// <param name="unAppId">The AppID of the Steam shortcut to modify.</param>
    //    /// <param name="szTag">The user tag to add to the shortcut.</param>
    //    public void AddShortcutUserTag(UInt32 unAppId, string szTag) =>
    //        GetDelegate<AddShortcutUserTagDelegate>()(InterfacePtr, unAppId, szTag);

    //    #region VTableIndex(42)
    //    [VTableIndex(42), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
    //    private delegate void RemoveShortcutUserTagDelegate(IntPtr thisPtr,
    //                                                        UInt32 unAppId,
    //                                                        [MarshalAs(UnmanagedType.LPUTF8Str)] string szTag);
    //    #endregion
    //    /// <summary>
    //    /// Removes an existing user tag from a Steam shortcut. <remarks><c>NOTE: Since the Sept 2019 Steam Library
    //    /// overhaul/redesign, this function will not appear to work correctly. While it will correctly modify the
    //    /// shortcuts.vdf file, the Steam UI will not reflect the changes made.</c></remarks>
    //    /// </summary>
    //    /// <param name="unAppId">The AppID of the Steam shortcut to modify.</param>
    //    /// <param name="szTag">The user tag to remove from the shortcut.</param>
    //    public void RemoveShortcutUserTag(UInt32 unAppId, string szTag) =>
    //        GetDelegate<RemoveShortcutUserTagDelegate>()(InterfacePtr, unAppId, szTag);

    //    // New vtable entry added with new Steam Library UI (around Sept 2019)
    //    #region VTableIndex(43)
    //    [VTableIndex(43), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
    //    private delegate void ClearAndSetShortcutUserTagsDelegate(IntPtr thisPtr, UInt32 unAppId, IntPtr szTagArray);
    //    #endregion
    //    /// <summary>
    //    /// Clears all of an existing Steam shortcut's user tags and assigns 0 or more new user tags according to the
    //    /// values in <paramref name="szTagArray"/>. <remarks><c>NOTE: Since the Sept 2019 Steam Library
    //    /// overhaul/redesign, this function will not appear to work correctly. While it will correctly modify the
    //    /// shortcuts.vdf file, the Steam UI will not reflect the changes made.</c></remarks>
    //    /// </summary>
    //    /// <param name="unAppId">The AppID of the Steam shortcut to modify.</param>
    //    /// <param name="szTagArray">A list of new user tags. Can be null/empty to assign no new user tags.</param>
    //    public void ClearAndSetShortcutUserTags(UInt32 unAppId, IList<string> szTagArray) =>
    //        GetDelegate<ClearAndSetShortcutUserTagsDelegate>()(InterfacePtr,
    //                                                           unAppId,
    //                                                           new SteamParamStringArray(szTagArray));

    //    #region VTableIndex(44)
    //    [VTableIndex(44), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate void SetShortcutHiddenDelegate(IntPtr thisPtr, UInt32 unAppId, bool arg1);
    //    #endregion
    //    /// <summary>
    //    /// Set the hidden attribute of an existing Steam shortcut. <remarks><c>NOTE: Since the Sept 2019 Steam Library
    //    /// overhaul/redesign, this function will not appear to work correctly. While it will correctly modify the
    //    /// shortcuts.vdf file, the Steam UI will not reflect the changes made.</c></remarks>
    //    /// </summary>
    //    /// <param name="unAppId">The AppID of the Steam shortcut to modify.</param>
    //    /// <param name="arg1">Set to True to hide shortcut; set to False to NOT hide shortcut.</param>
    //    public void SetShortcutHidden(UInt32 unAppId, bool arg1) =>
    //        GetDelegate<SetShortcutHiddenDelegate>()(InterfacePtr, unAppId, arg1);

    //    #region VTableIndex(45)
    //    [VTableIndex(45), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate void SetAllowDesktopConfigDelegate(IntPtr thisPtr, UInt32 unAppId, bool arg1);
    //    #endregion
    //    public void SetAllowDesktopConfig(UInt32 unAppId, bool arg1) =>
    //        GetDelegate<SetAllowDesktopConfigDelegate>()(InterfacePtr, unAppId, arg1);

    //    #region VTableIndex(46)
    //    [VTableIndex(46), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate void SetAllowOverlayDelegate(IntPtr thisPtr, UInt32 unAppId, bool arg1);
    //    #endregion
    //    public void SetAllowOverlay(UInt32 unAppId, bool arg1) =>
    //        GetDelegate<SetAllowOverlayDelegate>()(InterfacePtr, unAppId, arg1);

    //    #region VTableIndex(47)
    //    [VTableIndex(47), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate void SetOpenVrShortcutDelegate(IntPtr thisPtr, UInt32 unAppId, bool arg1);
    //    #endregion
    //    public void SetOpenVrShortcut(UInt32 unAppId, bool arg1) =>
    //        GetDelegate<SetOpenVrShortcutDelegate>()(InterfacePtr, unAppId, arg1);

    //    #region VTableIndex(48)
    //    [VTableIndex(48), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate void SetDevkitShortcutDelegate(IntPtr thisPtr, UInt32 unAppId, bool arg1);
    //    #endregion
    //    public void SetDevkitShortcut(UInt32 unAppId, bool arg1) =>
    //        GetDelegate<SetDevkitShortcutDelegate>()(InterfacePtr, unAppId, arg1);

    //    #region VTableIndex(49)
    //    [VTableIndex(49), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate void RemoveShortcutDelegate(IntPtr thisPtr, UInt32 unAppId);
    //    #endregion
    //    public void RemoveShortcut(UInt32 unAppId) => GetDelegate<RemoveShortcutDelegate>()(InterfacePtr, unAppId);

    //    #region VTableIndex(50)
    //    [VTableIndex(50), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate void RemoveAllTemporaryShortcutsDelegate(IntPtr thisPtr);
    //    #endregion
    //    public void RemoveAllTemporaryShortcuts() => GetDelegate<RemoveAllTemporaryShortcutsDelegate>()(InterfacePtr);

    //    #region VTableIndex(51)
    //    [VTableIndex(51), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    //    private delegate void LaunchShortcutDelegate(IntPtr thisPtr, UInt32 unAppId);
    //    #endregion
    //    public void LaunchShortcut(UInt32 unAppId) => GetDelegate<LaunchShortcutDelegate>()(InterfacePtr, unAppId);
    //}
}
