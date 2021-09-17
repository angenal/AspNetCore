using System;
using System.Net;
using System.Runtime.InteropServices;
using WebCore.Platform.Posix;
using WebCore.Platform.Posix.macOS;

namespace WebCore.Platform
{
    public static class PlatformDetails
    {
        public static readonly bool Is32Bit = IntPtr.Size == sizeof(int);

        public static readonly bool Is64Bit = Environment.Is64BitOperatingSystem;

        public static readonly bool CanPrefetch = IsWindows8OrNewer() || RunningOnPosix;

        public static readonly bool RunningOnPosix = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static readonly bool RunningOnMacOsx = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static readonly bool RunningOnLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static readonly bool RunningOnDocker = string.Equals(Environment.GetEnvironmentVariable("IN_DOCKER"), "true", StringComparison.OrdinalIgnoreCase) || Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != null;

        public static readonly string HostName = Environment.GetEnvironmentVariable("CUMPUTERNAME") ?? Environment.GetEnvironmentVariable("HOSTNAME") ?? Dns.GetHostName();

        public static string MachineName => Environment.MachineName;

        public static string OS => RunningOnLinux ? $"linux{(Is64Bit ? " x64" : " x32")}{(RunningOnDocker ? " on docker" : "")}" : RunningOnMacOsx ? $"macOS{(Is64Bit ? " x64" : " x32")}{(RunningOnDocker ? " on docker" : "")}" : $"windows{(Is64Bit ? " x64" : " x32")}{(RunningOnDocker ? " on docker" : "")}";

        public static ulong GetCurrentThreadId()
        {
            if (RunningOnPosix == false) return Win32ThreadsMethods.GetCurrentThreadId();

            if (RunningOnLinux) return (ulong)Syscall.syscall0(PerPlatformValues.SyscallNumbers.SYS_gettid);

            // OSX
            return macSyscall.pthread_self();
        }

        private static bool IsWindows8OrNewer()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
                return false;

            try
            {
                const string winString = "Windows ";
                var os = RuntimeInformation.OSDescription;

                var idx = os.IndexOf(winString, StringComparison.OrdinalIgnoreCase);
                if (idx < 0)
                    return false;

                var ver = os.Substring(idx + winString.Length);

                if (ver != null)
                {
                    // remove second occurance of '.' (win 10 might be 10.123.456)
                    var index = ver.IndexOf('.', ver.IndexOf('.') + 1);
                    ver = string.Concat(ver.Substring(0, index), ver.Substring(index + 1));

                    decimal output;
                    if (decimal.TryParse(ver, out output))
                    {
                        return output >= 6.19M; // 6.2 is win8, 6.1 win7..
                    }
                }

                return false;
            }
            catch (DllNotFoundException)
            {
                return false;
            }
        }
    }
}
