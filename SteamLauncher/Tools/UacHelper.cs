using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;

namespace SteamLauncher.Tools
{
    public static class UacHelper
    {
        private const string UAC_REGISTRY_KEY = @"Software\Microsoft\Windows\CurrentVersion\Policies\System";
        private const string UAC_REGISTRY_VALUE = "EnableLUA";

        private const uint STANDARD_RIGHTS_READ = 0x00020000;
        private const uint TOKEN_QUERY = 0x0008;
        private const uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenProcessToken(IntPtr processHandle, UInt32 desiredAccess, out IntPtr tokenHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool GetTokenInformation(IntPtr tokenHandle, TokenInformationClass tokenInformationClass, IntPtr tokenInformation, UInt32 tokenInformationLength, out UInt32 returnLength);

        private enum TokenInformationClass
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin,
            TokenElevationType,
            TokenLinkedToken,
            TokenElevation,
            TokenHasRestrictions,
            TokenAccessInformation,
            TokenVirtualizationAllowed,
            TokenVirtualizationEnabled,
            TokenIntegrityLevel,
            TokenUiAccess,
            TokenMandatoryPolicy,
            TokenLogonSid,
            MaxTokenInfoClass
        }

        private enum TokenElevationType
        {
            TokenElevationTypeDefault = 1,
            TokenElevationTypeFull,
            TokenElevationTypeLimited
        }

        public static bool IsUacEnabled
        {
            get
            {
                using (var uacKey = Registry.LocalMachine.OpenSubKey(UAC_REGISTRY_KEY, false))
                {
                    if (uacKey == null)
                        throw new ApplicationException("Unable to determine if UAC is enabled.");

                    var result = uacKey.GetValue(UAC_REGISTRY_VALUE).Equals(1);
                    return result;
                }
            }
        }

        public static bool IsCurrentProcessElevated()
        {
            return IsProcessElevated(Process.GetCurrentProcess());
        }

        public static bool IsProcessElevated(Process process)
        {
            if (!IsUacEnabled)
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                var result = principal.IsInRole(WindowsBuiltInRole.Administrator)
                             || principal.IsInRole(0x200); //Domain Administrator
                return result;
            }

            if (!OpenProcessToken(process.Handle, TOKEN_READ, out var tokenHandle))
            {
                throw new ApplicationException("Could not get process token.  Win32 Error Code: " +
                                               Marshal.GetLastWin32Error());
            }

            try
            {
                var elevationResult = TokenElevationType.TokenElevationTypeDefault;
                int elevationResultSize;

                // Uncertain behavior on rarely encountered platforms... this tries to cover those possibilities
                try { elevationResultSize = Marshal.SizeOf(typeof(TokenElevationType)); }
                catch { elevationResultSize = Marshal.SizeOf((int)elevationResult); }

                UInt32 returnedSize = 0;

                var elevationTypePtr = Marshal.AllocHGlobal(elevationResultSize);
                try
                {
                    var success = GetTokenInformation(tokenHandle, TokenInformationClass.TokenElevationType,
                                                       elevationTypePtr, (UInt32)elevationResultSize,
                                                       out returnedSize);
                    if (success)
                    {
                        elevationResult = (TokenElevationType)Marshal.ReadInt32(elevationTypePtr);
                        var isProcessAdmin = (elevationResult == TokenElevationType.TokenElevationTypeFull);
                        return isProcessAdmin;
                    }
                    else
                    {
                        throw new ApplicationException("Unable to determine the current elevation.");
                    }
                }
                finally
                {
                    if (elevationTypePtr != IntPtr.Zero)
                        Marshal.FreeHGlobal(elevationTypePtr);
                }
            }
            finally
            {
                if (tokenHandle != IntPtr.Zero)
                    CloseHandle(tokenHandle);
            }

        }
    }
}
