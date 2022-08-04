using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Management.Infrastructure;
using Microsoft.Win32;
using SteamLauncher.Logging;

namespace SteamLauncher.Tools
{
    public static class ProcessExtension
    {
        private const string UAC_REGISTRY_KEY = @"Software\Microsoft\Windows\CurrentVersion\Policies\System";
        private const string UAC_REGISTRY_VALUE = "EnableLUA";

        private const uint STANDARD_RIGHTS_REQUIRED = 0xF0000;
        private const uint TOKEN_ASSIGN_PRIMARY = 0x1;
        private const uint TOKEN_DUPLICATE = 0x2;
        private const uint TOKEN_IMPERSONATE = 0x4;
        private const uint TOKEN_QUERY = 0x8;
        private const uint TOKEN_QUERY_SOURCE = 0x10;
        private const uint TOKEN_ADJUST_GROUPS = 0x40;
        private const uint TOKEN_ADJUST_PRIVILEGES = 0x20;
        private const uint TOKEN_ADJUST_SESSIONID = 0x100;
        private const uint TOKEN_ADJUST_DEFAULT = 0x80;
        private const uint STANDARD_RIGHTS_READ = 0x00020000;
        private const uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);
        private const uint TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED |
                                               TOKEN_ASSIGN_PRIMARY |
                                               TOKEN_DUPLICATE |
                                               TOKEN_IMPERSONATE |
                                               TOKEN_QUERY |
                                               TOKEN_QUERY_SOURCE |
                                               TOKEN_ADJUST_PRIVILEGES |
                                               TOKEN_ADJUST_GROUPS |
                                               TOKEN_ADJUST_SESSIONID |
                                               TOKEN_ADJUST_DEFAULT);

        private const int DOMAIN_ADMIN = 0x200;

        public const int SECURITY_IMPERSONATION = 2;
        public const int TOKEN_IMPERSONATION = 2;

        public const int STILL_ACTIVE = 259;

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DuplicateTokenEx(IntPtr hTok,
                                                    UInt32 desiredAccess,
                                                    IntPtr secAttPtr,
                                                    int impLvl,
                                                    int tokType,
                                                    out IntPtr tokenHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CreateWellKnownSid(WELL_KNOWN_SID_TYPE wellKnownSidType,
                                                      IntPtr domainSid,
                                                      IntPtr pSid,
                                                      ref uint cbSid);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CheckTokenMembership(IntPtr tokenHandle, IntPtr sidToCheck, out bool IsMember);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetCurrentProcessId();

        [DllImport("kernel32.dll")]
        private static extern bool GetExitCodeProcess(IntPtr hProcess, out uint lpExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool QueryFullProcessImageName([In] IntPtr hProcess,
                                                            [In] int dwFlags,
                                                            [Out] StringBuilder lpExeName,
                                                            ref int lpdwSize);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool GetTokenInformation(IntPtr tokenHandle,
                                                       TokenInformationClass tokenInformationClass,
                                                       IntPtr tokenInformation,
                                                       uint tokenInformationLength,
                                                       out uint returnLength);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        private static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        private static extern int ResumeThread(IntPtr hThread);

        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

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

        [Flags]
        internal enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        internal enum WELL_KNOWN_SID_TYPE
        {
            WinNullSid = 0,
            WinWorldSid = 1,
            WinLocalSid = 2,
            WinCreatorOwnerSid = 3,
            WinCreatorGroupSid = 4,
            WinCreatorOwnerServerSid = 5,
            WinCreatorGroupServerSid = 6,
            WinNtAuthoritySid = 7,
            WinDialupSid = 8,
            WinNetworkSid = 9,
            WinBatchSid = 10,
            WinInteractiveSid = 11,
            WinServiceSid = 12,
            WinAnonymousSid = 13,
            WinProxySid = 14,
            WinEnterpriseControllersSid = 15,
            WinSelfSid = 16,
            WinAuthenticatedUserSid = 17,
            WinRestrictedCodeSid = 18,
            WinTerminalServerSid = 19,
            WinRemoteLogonIdSid = 20,
            WinLogonIdsSid = 21,
            WinLocalSystemSid = 22,
            WinLocalServiceSid = 23,
            WinNetworkServiceSid = 24,
            WinBuiltinDomainSid = 25,
            WinBuiltinAdministratorsSid = 26,
            WinBuiltinUsersSid = 27,
            WinBuiltinGuestsSid = 28,
            WinBuiltinPowerUsersSid = 29,
            WinBuiltinAccountOperatorsSid = 30,
            WinBuiltinSystemOperatorsSid = 31,
            WinBuiltinPrintOperatorsSid = 32,
            WinBuiltinBackupOperatorsSid = 33,
            WinBuiltinReplicatorSid = 34,
            WinBuiltinPreWindows2000CompatibleAccessSid = 35,
            WinBuiltinRemoteDesktopUsersSid = 36,
            WinBuiltinNetworkConfigurationOperatorsSid = 37,
            WinAccountAdministratorSid = 38,
            WinAccountGuestSid = 39,
            WinAccountKrbtgtSid = 40,
            WinAccountDomainAdminsSid = 41,
            WinAccountDomainUsersSid = 42,
            WinAccountDomainGuestsSid = 43,
            WinAccountComputersSid = 44,
            WinAccountControllersSid = 45,
            WinAccountCertAdminsSid = 46,
            WinAccountSchemaAdminsSid = 47,
            WinAccountEnterpriseAdminsSid = 48,
            WinAccountPolicyAdminsSid = 49,
            WinAccountRasAndIasServersSid = 50,
            WinNTLMAuthenticationSid = 51,
            WinDigestAuthenticationSid = 52,
            WinSChannelAuthenticationSid = 53,
            WinThisOrganizationSid = 54,
            WinOtherOrganizationSid = 55,
            WinBuiltinIncomingForestTrustBuildersSid = 56,
            WinBuiltinPerfMonitoringUsersSid = 57,
            WinBuiltinPerfLoggingUsersSid = 58,
            WinBuiltinAuthorizationAccessSid = 59,
            WinBuiltinTerminalServerLicenseServersSid = 60
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

        /// <summary>
        /// Suspends the given process.
        /// </summary>
        /// <param name="process">The process to suspend.</param>
        public static void Suspend(this Process process)
        {
            if (string.IsNullOrEmpty(process.ProcessName))
                return;

            foreach (ProcessThread thread in process.Threads)
            {
                var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                SuspendThread(pOpenThread);
                CloseHandle(pOpenThread);
            }
        }

        /// <summary>
        /// Resumes the given process.
        /// </summary>
        /// <param name="process">The process to resume.</param>
        public static void Resume(this Process process)
        {
            if (string.IsNullOrEmpty(process.ProcessName))
                return;

            foreach (ProcessThread thread in process.Threads)
            {
                var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                var suspendCount = 0;
                do
                {
                    suspendCount = ResumeThread(pOpenThread);
                } while (suspendCount > 0);

                CloseHandle(pOpenThread);
            }
        }

        /// <summary>
        /// Returns the command line string used to start the provided Process.
        /// </summary>
        /// <param name="process">The Process to query for its startup command line.</param>
        /// <returns>A string containing the command line.</returns>
        public static string GetCommandLine(this Process process)
        {
            try
            {
                var cimSession = CimSession.Create(null);
                var queryInstances = cimSession.QueryInstances(@"root\cimv2",
                                                               "WQL",
                                                               $"SELECT CommandLine FROM Win32_Process WHERE " +
                                                               $"ProcessId = {process.Id}");

                return queryInstances.FirstOrDefault()?.CimInstanceProperties["CommandLine"]?.Value.ToString();
            }
            catch (CimException ex)
            {
                Logger.Error($"An exception occurred while trying to retrieve the command line of process " +
                             $"'{process.ProcessName}': {ex.Message}");
            }

            return null;
        }
        //public static string GetCommandLine(this Process process)
        //{
        //    using (var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
        //    using (var objects = searcher.Get())
        //    {
        //        return objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
        //    }
        //}

        /// <summary>
        /// Returns the command line string used to start the provided Process. If the command
        /// line cannot be found for any reason, a null value is returned.
        /// </summary>
        /// <param name="process">The Process to query for its startup command line.</param>
        /// <returns>A string containing the command line or null if a problem is encountered.</returns>
        public static string GetCommandLineOrDefault(this Process process)
        {
            try
            {
                return process.GetCommandLine();
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static Process GetProcessByIdOrDefault(int id)
        {
            try
            {
                return Process.GetProcessById(id);
            }
            catch
            {
                return null;
            }
        }

        public static bool IsActive(this Process process)
        {
            var hProcess = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, process.Id);
            if (hProcess == IntPtr.Zero) 
                hProcess = OpenProcess(ProcessAccessFlags.QueryInformation, false, process.Id);

            GetExitCodeProcess(hProcess, out uint exitCode);
            CloseHandle(hProcess);
            return exitCode == STILL_ACTIVE;
        }

        public static bool IsCurrentProcessElevated()
        {
            return Process.GetCurrentProcess().IsElevated();
        }

        public static bool IsElevated(this Process process)
        {
            return IsProcessElevated(process.Id);
        }

        private static bool IsProcessElevated(int processId)
        {
            IntPtr hPriToken = IntPtr.Zero, hImpToken = IntPtr.Zero;
            var hProcess = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, processId);
            if (hProcess == IntPtr.Zero) 
                hProcess = OpenProcess(ProcessAccessFlags.QueryInformation, false, processId); // < Vista
            var haveToken = OpenProcessToken(hProcess, TOKEN_DUPLICATE, out hPriToken);
            if (haveToken)
            {
                haveToken = DuplicateTokenEx(hPriToken,
                                             TOKEN_QUERY,
                                             IntPtr.Zero,
                                             SECURITY_IMPERSONATION,
                                             TOKEN_IMPERSONATION,
                                             out hImpToken);
                CloseHandle(hPriToken);
            }
            if (hProcess != IntPtr.Zero) CloseHandle(hProcess);
            if (haveToken)
            {
                uint cbSid = 0;
                bool isMember = false;
                CreateWellKnownSid(WELL_KNOWN_SID_TYPE.WinBuiltinAdministratorsSid,
                                   IntPtr.Zero,
                                   IntPtr.Zero,
                                   ref cbSid);
                IntPtr pSid = Marshal.AllocCoTaskMem(Convert.ToInt32(cbSid));
                var succeed = pSid != IntPtr.Zero &&
                              CreateWellKnownSid(WELL_KNOWN_SID_TYPE.WinBuiltinAdministratorsSid,
                                                 IntPtr.Zero,
                                                 pSid,
                                                 ref cbSid);
                succeed = succeed && CheckTokenMembership(hImpToken, pSid, out isMember);
                Marshal.FreeCoTaskMem(pSid);
                CloseHandle(hImpToken);
                return succeed && isMember;
            }
            return false;
        }
    }
}
