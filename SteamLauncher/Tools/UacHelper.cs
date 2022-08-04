//using System;
//using System.ComponentModel;
//using System.Diagnostics;
//using System.Runtime.InteropServices;
//using System.Security.Principal;
//using System.Text;
//using Microsoft.Win32;

//namespace SteamLauncher.Tools
//{
//    public static class UacHelper
//    {
//        private const string UAC_REGISTRY_KEY = @"Software\Microsoft\Windows\CurrentVersion\Policies\System";
//        private const string UAC_REGISTRY_VALUE = "EnableLUA";

//        private const uint STANDARD_RIGHTS_REQUIRED = 0xF0000;
//        private const uint TOKEN_ASSIGN_PRIMARY = 0x1;
//        private const uint TOKEN_DUPLICATE = 0x2;
//        private const uint TOKEN_IMPERSONATE = 0x4;
//        private const uint TOKEN_QUERY = 0x8;
//        private const uint TOKEN_QUERY_SOURCE = 0x10;
//        private const uint TOKEN_ADJUST_GROUPS = 0x40;
//        private const uint TOKEN_ADJUST_PRIVILEGES = 0x20;
//        private const uint TOKEN_ADJUST_SESSIONID = 0x100;
//        private const uint TOKEN_ADJUST_DEFAULT = 0x80;
//        private const uint STANDARD_RIGHTS_READ = 0x00020000;
//        private const uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);
//        private const uint TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED |
//                                               TOKEN_ASSIGN_PRIMARY |
//                                               TOKEN_DUPLICATE |
//                                               TOKEN_IMPERSONATE |
//                                               TOKEN_QUERY |
//                                               TOKEN_QUERY_SOURCE |
//                                               TOKEN_ADJUST_PRIVILEGES |
//                                               TOKEN_ADJUST_GROUPS |
//                                               TOKEN_ADJUST_SESSIONID |
//                                               TOKEN_ADJUST_DEFAULT);

//        private const int DOMAIN_ADMIN = 0x200;

//        public const int SECURITY_IMPERSONATION = 2;
//        public const int TOKEN_IMPERSONATION = 2;

//        [DllImport("advapi32.dll", SetLastError = true)]
//        [return: MarshalAs(UnmanagedType.Bool)]
//        private static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

//        [DllImport("advapi32.dll", SetLastError = true)]
//        [return: MarshalAs(UnmanagedType.Bool)]
//        private static extern bool DuplicateTokenEx(IntPtr hTok,
//                                                   UInt32 desiredAccess,
//                                                   IntPtr secAttPtr,
//                                                   int impLvl,
//                                                   int tokType,
//                                                   out IntPtr tokenHandle);

//        [DllImport("kernel32.dll", SetLastError = true)]
//        private static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

//        [DllImport("advapi32.dll", SetLastError = true)]
//        [return: MarshalAs(UnmanagedType.Bool)]
//        private static extern bool CreateWellKnownSid(WELL_KNOWN_SID_TYPE wellKnownSidType,
//                                                      IntPtr domainSid,
//                                                      IntPtr pSid,
//                                                      ref uint cbSid);

//        [DllImport("advapi32.dll", SetLastError = true)]
//        [return: MarshalAs(UnmanagedType.Bool)]
//        private static extern bool CheckTokenMembership(IntPtr tokenHandle, IntPtr sidToCheck, out bool IsMember);

//        [DllImport("kernel32.dll", SetLastError = true)]
//        private static extern int GetCurrentProcessId();

//        [DllImport("kernel32.dll", SetLastError = true)]
//        [return: MarshalAs(UnmanagedType.Bool)]
//        private static extern bool CloseHandle(IntPtr hObject);

//        [DllImport("kernel32.dll", SetLastError = true)]
//        public static extern bool QueryFullProcessImageName([In] IntPtr hProcess,
//                                                            [In] int dwFlags,
//                                                            [Out] StringBuilder lpExeName,
//                                                            ref int lpdwSize);

//        [DllImport("advapi32.dll", SetLastError = true)]
//        private static extern bool GetTokenInformation(IntPtr tokenHandle,
//                                                       TokenInformationClass tokenInformationClass,
//                                                       IntPtr tokenInformation,
//                                                       uint tokenInformationLength,
//                                                       out uint returnLength);

//        private enum TokenInformationClass
//        {
//            TokenUser = 1,
//            TokenGroups,
//            TokenPrivileges,
//            TokenOwner,
//            TokenPrimaryGroup,
//            TokenDefaultDacl,
//            TokenSource,
//            TokenType,
//            TokenImpersonationLevel,
//            TokenStatistics,
//            TokenRestrictedSids,
//            TokenSessionId,
//            TokenGroupsAndPrivileges,
//            TokenSessionReference,
//            TokenSandBoxInert,
//            TokenAuditPolicy,
//            TokenOrigin,
//            TokenElevationType,
//            TokenLinkedToken,
//            TokenElevation,
//            TokenHasRestrictions,
//            TokenAccessInformation,
//            TokenVirtualizationAllowed,
//            TokenVirtualizationEnabled,
//            TokenIntegrityLevel,
//            TokenUiAccess,
//            TokenMandatoryPolicy,
//            TokenLogonSid,
//            MaxTokenInfoClass
//        }

//        private enum TokenElevationType
//        {
//            TokenElevationTypeDefault = 1,
//            TokenElevationTypeFull,
//            TokenElevationTypeLimited
//        }

//        [Flags]
//        internal enum ProcessAccessFlags : uint
//        {
//            All = 0x001F0FFF,
//            Terminate = 0x00000001,
//            CreateThread = 0x00000002,
//            VirtualMemoryOperation = 0x00000008,
//            VirtualMemoryRead = 0x00000010,
//            VirtualMemoryWrite = 0x00000020,
//            DuplicateHandle = 0x00000040,
//            CreateProcess = 0x000000080,
//            SetQuota = 0x00000100,
//            SetInformation = 0x00000200,
//            QueryInformation = 0x00000400,
//            QueryLimitedInformation = 0x00001000,
//            Synchronize = 0x00100000
//        }

//        internal enum WELL_KNOWN_SID_TYPE
//        {
//            WinNullSid = 0,
//            WinWorldSid = 1,
//            WinLocalSid = 2,
//            WinCreatorOwnerSid = 3,
//            WinCreatorGroupSid = 4,
//            WinCreatorOwnerServerSid = 5,
//            WinCreatorGroupServerSid = 6,
//            WinNtAuthoritySid = 7,
//            WinDialupSid = 8,
//            WinNetworkSid = 9,
//            WinBatchSid = 10,
//            WinInteractiveSid = 11,
//            WinServiceSid = 12,
//            WinAnonymousSid = 13,
//            WinProxySid = 14,
//            WinEnterpriseControllersSid = 15,
//            WinSelfSid = 16,
//            WinAuthenticatedUserSid = 17,
//            WinRestrictedCodeSid = 18,
//            WinTerminalServerSid = 19,
//            WinRemoteLogonIdSid = 20,
//            WinLogonIdsSid = 21,
//            WinLocalSystemSid = 22,
//            WinLocalServiceSid = 23,
//            WinNetworkServiceSid = 24,
//            WinBuiltinDomainSid = 25,
//            WinBuiltinAdministratorsSid = 26,
//            WinBuiltinUsersSid = 27,
//            WinBuiltinGuestsSid = 28,
//            WinBuiltinPowerUsersSid = 29,
//            WinBuiltinAccountOperatorsSid = 30,
//            WinBuiltinSystemOperatorsSid = 31,
//            WinBuiltinPrintOperatorsSid = 32,
//            WinBuiltinBackupOperatorsSid = 33,
//            WinBuiltinReplicatorSid = 34,
//            WinBuiltinPreWindows2000CompatibleAccessSid = 35,
//            WinBuiltinRemoteDesktopUsersSid = 36,
//            WinBuiltinNetworkConfigurationOperatorsSid = 37,
//            WinAccountAdministratorSid = 38,
//            WinAccountGuestSid = 39,
//            WinAccountKrbtgtSid = 40,
//            WinAccountDomainAdminsSid = 41,
//            WinAccountDomainUsersSid = 42,
//            WinAccountDomainGuestsSid = 43,
//            WinAccountComputersSid = 44,
//            WinAccountControllersSid = 45,
//            WinAccountCertAdminsSid = 46,
//            WinAccountSchemaAdminsSid = 47,
//            WinAccountEnterpriseAdminsSid = 48,
//            WinAccountPolicyAdminsSid = 49,
//            WinAccountRasAndIasServersSid = 50,
//            WinNTLMAuthenticationSid = 51,
//            WinDigestAuthenticationSid = 52,
//            WinSChannelAuthenticationSid = 53,
//            WinThisOrganizationSid = 54,
//            WinOtherOrganizationSid = 55,
//            WinBuiltinIncomingForestTrustBuildersSid = 56,
//            WinBuiltinPerfMonitoringUsersSid = 57,
//            WinBuiltinPerfLoggingUsersSid = 58,
//            WinBuiltinAuthorizationAccessSid = 59,
//            WinBuiltinTerminalServerLicenseServersSid = 60
//        }

//        public static bool IsUacEnabled
//        {
//            get
//            {
//                using (var uacKey = Registry.LocalMachine.OpenSubKey(UAC_REGISTRY_KEY, false))
//                {
//                    if (uacKey == null)
//                        throw new ApplicationException("Unable to determine if UAC is enabled.");

//                    var result = uacKey.GetValue(UAC_REGISTRY_VALUE).Equals(1);
//                    return result;
//                }
//            }
//        }

//        public static bool IsCurrentProcessElevated()
//        {
//            return IsProcessElevated(Process.GetCurrentProcess());
//        }

//        public static bool IsProcessElevated(Process process)
//        {
//            return IsProcessElevated(process.Id);
//        }

//        private static bool IsProcessElevated(int processId)
//        {
//            IntPtr hPriToken = IntPtr.Zero, hImpToken = IntPtr.Zero;
//            var hProcess = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, processId);
//            if (hProcess == IntPtr.Zero) hProcess = OpenProcess(ProcessAccessFlags.QueryInformation, false, processId); // < Vista
//            var haveToken = OpenProcessToken(hProcess, TOKEN_DUPLICATE, out hPriToken);
//            if (haveToken)
//            {
//                haveToken = DuplicateTokenEx(hPriToken,
//                                             TOKEN_QUERY,
//                                             IntPtr.Zero,
//                                             SECURITY_IMPERSONATION,
//                                             TOKEN_IMPERSONATION,
//                                             out hImpToken);
//                CloseHandle(hPriToken);
//            }
//            if (hProcess != IntPtr.Zero) CloseHandle(hProcess);
//            if (haveToken)
//            {
//                uint cbSid = 0;
//                bool isMember = false;
//                CreateWellKnownSid(WELL_KNOWN_SID_TYPE.WinBuiltinAdministratorsSid,
//                                   IntPtr.Zero,
//                                   IntPtr.Zero,
//                                   ref cbSid);
//                IntPtr pSid = Marshal.AllocCoTaskMem(Convert.ToInt32(cbSid));
//                var succeed = pSid != IntPtr.Zero &&
//                              CreateWellKnownSid(WELL_KNOWN_SID_TYPE.WinBuiltinAdministratorsSid,
//                                                 IntPtr.Zero,
//                                                 pSid,
//                                                 ref cbSid);
//                succeed = succeed && CheckTokenMembership(hImpToken, pSid, out isMember);
//                Marshal.FreeCoTaskMem(pSid);
//                CloseHandle(hImpToken);
//                return succeed && isMember;
//            }
//            return false;
//        }

//        //public static bool IsProcessElevated(Process process)
//        //{
//        //    if (!IsUacEnabled)
//        //    {
//        //        var identity = WindowsIdentity.GetCurrent();
//        //        var principal = new WindowsPrincipal(identity);
//        //        var result = principal.IsInRole(WindowsBuiltInRole.Administrator)
//        //                     || principal.IsInRole(0x200); //Domain Administrator
//        //        return result;
//        //    }

//        //    if (!OpenProcessToken(process.Handle, TOKEN_READ, out var tokenHandle))
//        //    {
//        //        throw new ApplicationException("Could not get process token.  Win32 Error Code: " +
//        //                                       Marshal.GetLastWin32Error());
//        //    }

//        //    try
//        //    {
//        //        var elevationResult = TokenElevationType.TokenElevationTypeDefault;
//        //        int elevationResultSize;

//        //        // Uncertain behavior on rarely encountered platforms... this tries to cover those possibilities
//        //        try { elevationResultSize = Marshal.SizeOf(typeof(TokenElevationType)); }
//        //        catch { elevationResultSize = Marshal.SizeOf((int)elevationResult); }

//        //        UInt32 returnedSize = 0;

//        //        var elevationTypePtr = Marshal.AllocHGlobal(elevationResultSize);
//        //        try
//        //        {
//        //            var success = GetTokenInformation(tokenHandle, TokenInformationClass.TokenElevationType,
//        //                                               elevationTypePtr, (UInt32)elevationResultSize,
//        //                                               out returnedSize);
//        //            if (success)
//        //            {
//        //                elevationResult = (TokenElevationType)Marshal.ReadInt32(elevationTypePtr);
//        //                var isProcessAdmin = (elevationResult == TokenElevationType.TokenElevationTypeFull);
//        //                return isProcessAdmin;
//        //            }
//        //            else
//        //            {
//        //                throw new ApplicationException("Unable to determine the current elevation.");
//        //            }
//        //        }
//        //        finally
//        //        {
//        //            if (elevationTypePtr != IntPtr.Zero)
//        //                Marshal.FreeHGlobal(elevationTypePtr);
//        //        }
//        //    }
//        //    finally
//        //    {
//        //        if (tokenHandle != IntPtr.Zero)
//        //            CloseHandle(tokenHandle);
//        //    }

//        //}
//    }
//}
