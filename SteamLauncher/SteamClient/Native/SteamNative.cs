using System;
using System.Runtime.InteropServices;

namespace SteamLauncher.SteamClient.Native
{
    public struct SteamNative
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate IntPtr CreateInterface(string version, IntPtr returnCode);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int CreateSteamPipe();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool BReleaseSteamPipe(int pipe);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ConnectToGlobalUser(int pipe);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ReleaseUser(int pipe, int user);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        public delegate IntPtr GetIClientShortcuts(IntPtr thisptr, int user, int pipe, string version);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct SteamParamStringArray_t
    {
        public IntPtr stringArrayPtr;
        public int numOfStrings;
    }
}
