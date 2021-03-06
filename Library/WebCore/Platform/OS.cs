using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using WebCore.Platform.Posix;
using RuntimeEnvironment = Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment;

namespace WebCore.Platform
{
    public static class OS
    {
        public static readonly bool Is32Bit = IntPtr.Size == sizeof(int);

        public static readonly bool Is64Bit = Environment.Is64BitOperatingSystem;

        public static readonly bool CanPrefetch = IsWindows8OrNewer() || IsPosix;

        public static readonly bool IsDocker = string.Equals(Environment.GetEnvironmentVariable("IN_DOCKER"), "true", StringComparison.OrdinalIgnoreCase) || Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != null;

        public static readonly bool IsPosix = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static readonly bool IsMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static readonly string HostName = Environment.GetEnvironmentVariable("CUMPUTERNAME") ?? Environment.GetEnvironmentVariable("HOSTNAME") ?? Dns.GetHostName();

        public static string Name => IsLinux ? $"linux{(Is64Bit ? "-x64" : "-x32")}{(IsDocker ? " on docker" : "")}" : IsMacOS ? $"macOS{(Is64Bit ? "-x64" : "-x32")}{(IsDocker ? " on docker" : "")}" : $"windows{(Is64Bit ? "-x64" : "-x32")}{(IsDocker ? " on docker" : "")}";

        public static string Version => IsWindows ? Environment.OSVersion.ToString() : string.Concat(RuntimeEnvironment.OperatingSystem, " ", RuntimeEnvironment.OperatingSystemVersion);

        internal static ConcurrentDictionary<string, string> Data = new ConcurrentDictionary<string, string>();

        public static ulong GetCurrentThreadId()
        {
            if (IsPosix == false) return Win32ThreadsMethods.GetCurrentThreadId();
            if (IsLinux) return (ulong)Syscall.syscall0(PerPlatformValues.SyscallNumbers.SYS_gettid);
            return Posix.macOS.macSyscall.pthread_self(); // OSX
        }

        public static string GetDeviceId()
        {
            if (Data.TryGetValue(nameof(GetDeviceId), out string v)) return v;
            string machineName = Environment.MachineName, macAddress = GetMacAddress(), osVersion = Version;
            var s = new StringBuilder();
            s.Append(machineName);
            s.Append(macAddress);
            s.Append(osVersion);
            if (IsWindows)
            {
                // Get Registry @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography"
                var machineGuid = Regedit.GetValueInLocalMachine(@"SOFTWARE\Microsoft\Cryptography", "MachineGuid")?.ToString();
                s.Append(machineGuid);
                var motherboardSerialNumber = ManagementObject.Search("Win32_BaseBoard", "SerialNumber");
                s.Append(motherboardSerialNumber);
                var systemUUID = ManagementObject.Search("Win32_ComputerSystemProduct", "UUID");
                s.Append(systemUUID);
                var systemDriveSerialNumber = GetSystemDriveSerialNumber();
                s.Append(systemDriveSerialNumber);
            }
            else if (IsLinux)
            {
                var machineID = Posix.Linux.Environment.MachineID;
                s.Append(machineID);
                var productUUID = Posix.Linux.Environment.ProductUUID;
                s.Append(productUUID);
                var motherboardSerialNumber = Posix.Linux.Environment.MotherboardSerialNumber;
                s.Append(motherboardSerialNumber);
                var systemDriveSerialNumber = Posix.Linux.Environment.SystemDriveSerialNumber;
                s.Append(systemDriveSerialNumber);
            }
            else if (IsMacOS)
            {
                var platformSerialNumber = new Posix.macOS.PlatformSerialNumber().GetValue();
                s.Append(platformSerialNumber);
                var systemDriveSerialNumber = new Posix.macOS.SystemDriveSerialNumber().GetValue();
                s.Append(systemDriveSerialNumber);
            }
            v = s.ToString().Md5();
            Data.TryAdd(nameof(GetDeviceId), v);
            return v;
        }

        public static string GetMacAddress(bool excludeWireless = false)
        {
            if (Data.TryGetValue($"{nameof(GetMacAddress)}{excludeWireless}", out string v)) return v;
            var values = NetworkInterface.GetAllNetworkInterfaces()
                .Where(x => !excludeWireless || x.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
                .Select(x => x.GetPhysicalAddress().ToString())
                .Where(x => !string.IsNullOrWhiteSpace(x) && x != "000000000000")
                .Select(x => x.FormatMacAddress())
                .ToList();
            if (values.Count > 0)
            {
                values.Sort();
                v = string.Join(",", values.ToArray());
            }
            if (v == null) return v;
            Data.TryAdd($"{nameof(GetMacAddress)}{excludeWireless}", v);
            return v;
        }

        public static string GetMotherboardSerialNumber(bool isWindows = true)
        {
            if (isWindows)
            {
                return ManagementObject.Search("Win32_BaseBoard", "SerialNumber");
            }
            else if (IsLinux)
            {
                return Posix.Linux.Environment.MotherboardSerialNumber;
            }
            else if (IsMacOS)
            {
                return new Posix.macOS.PlatformSerialNumber().GetValue();
            }
            return null;
        }

        public static string GetSystemDriveSerialNumber(bool isWindows = true)
        {
            if (isWindows)
            {
                var systemLogicalDiskDeviceId = Environment.GetFolderPath(Environment.SpecialFolder.System).Substring(0, 2);

                var queryString = $"SELECT * FROM Win32_LogicalDisk where DeviceId = '{systemLogicalDiskDeviceId}'";
                using (var searcher = new System.Management.ManagementObjectSearcher(queryString))
                {
                    foreach (var disk in searcher.Get().OfType<System.Management.ManagementObject>())
                    {
                        foreach (var partition in disk.GetRelated("Win32_DiskPartition").OfType<System.Management.ManagementObject>())
                        {
                            foreach (var drive in partition.GetRelated("Win32_DiskDrive").OfType<System.Management.ManagementObject>())
                            {
                                if (drive["SerialNumber"] is string serialNumber) return serialNumber.Trim();
                            }
                        }
                    }
                }
            }
            else if (IsLinux)
            {
                return new Posix.Linux.SystemDriveSerialNumber().GetValue();
            }
            else if (IsMacOS)
            {
                return new Posix.macOS.SystemDriveSerialNumber().GetValue();
            }
            return null;
        }

        public static bool IsWindows8OrNewer()
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

        /// <summary>
        /// Windows System.Management.ManagementObjectSearcher
        /// </summary>
        public static class ManagementObject
        {
            /// <summary>
            /// Gets the component value.
            /// </summary>
            /// <param name="className">The class name.</param>
            /// <param name="propertyName">The property name.</param>
            /// <returns>The component value.</returns>
            public static string Search(string className, string propertyName, string defaultValue = null)
            {
                var values = new List<string>();
                try
                {
                    using (var managementObjectSearcher = new System.Management.ManagementObjectSearcher($"SELECT {propertyName} FROM {className}"))
                    {
                        using (var managementObjectCollection = managementObjectSearcher.Get())
                        {
                            foreach (var managementObject in managementObjectCollection)
                            {
                                try
                                {
                                    if (managementObject[propertyName] is string value) values.Add(value);
                                }
                                finally
                                {
                                    managementObject.Dispose();
                                }
                            }
                        }
                    }
                }
                catch
                {
                    return defaultValue;
                }
                values.Sort();
                return values.Count > 0 ? string.Join(",", values.ToArray()) : defaultValue;
            }
        }

        /// <summary>
        /// Windows ?????????
        /// </summary>
        public static class Regedit
        {
            public static readonly string CurrentUser = Microsoft.Win32.Registry.CurrentUser.Name;
            public static readonly string LocalMachine = Microsoft.Win32.Registry.LocalMachine.Name;
            /// <summary>
            /// ????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????? null???
            /// </summary>
            /// <param name="keyName">??????????????????????????????HKEY_CURRENT_USER?????????????????????????????????????????????</param>
            /// <param name="valueName">??????/??????????????????</param>
            /// <param name="defaultValue">??? name ???????????????????????????</param>
            /// <returns>????????? keyName ???????????????????????????????????? null????????????????????? valueName ??????????????????????????????????????? valueName???????????? defaultValue???</returns>
            public static object GetValueInLocalMachine(string keyName, string valueName, object defaultValue = null)
            {
                try
                {
                    using (var registry = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry64))
                    {
                        using (var subKey = registry.OpenSubKey(keyName)) return subKey.GetValue(valueName)?.ToString();
                    }
                }
                catch
                {
                    return defaultValue;
                }
            }
            /// <summary>
            /// ????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????? null???
            /// </summary>
            public static object GetValue(string keyName, string valueName, object defaultValue = null) => Microsoft.Win32.Registry.GetValue(keyName, valueName, defaultValue);
            /// <summary>
            /// ??????????????????????????????????????????/?????????????????????????????????????????????????????????
            /// </summary>
            /// <param name="keyName">??????????????????????????????HKEY_CURRENT_USER?????????????????????????????????????????????</param>
            /// <param name="valueName">??????/??????????????????</param>
            /// <param name="value">??????????????????</param>
            public static void SetValue(string keyName, string valueName, object value) => Microsoft.Win32.Registry.SetValue(keyName, valueName, value);
            /// <summary>
            /// ????????????????????????????????????????????????????????????????????????????????????/?????????????????????????????????????????????????????????
            /// </summary>
            /// <param name="keyName">??????????????????????????????HKEY_CURRENT_USER?????????????????????????????????????????????</param>
            /// <param name="valueName">??????/??????????????????</param>
            /// <param name="value">??????????????????</param>
            /// <param name="valueKind">????????????????????????????????????????????????</param>
            public static void SetValue(string keyName, string valueName, object value, RegeditValueKind valueKind) => Microsoft.Win32.Registry.SetValue(keyName, valueName, Enum.TryParse<Microsoft.Win32.RegistryValueKind>(valueKind.ToString(), out var kind) ? kind : Microsoft.Win32.RegistryValueKind.String);
        }
        /// <summary>
        /// Windows ????????? - ????????????????????????????????????????????????
        /// </summary>
        public enum RegeditValueKind
        {
            //
            // ??????:
            //     A null-terminated string. This value is equivalent to the Windows API registry
            //     data type REG_SZ.
            String = 1,
            //
            // ??????:
            //     A null-terminated string that contains unexpanded references to environment variables,
            //     such as %PATH%, that are expanded when the value is retrieved. This value is
            //     equivalent to the Windows API registry data type REG_EXPAND_SZ.
            ExpandString = 2,
            //
            // ??????:
            //     Binary data in any form. This value is equivalent to the Windows API registry
            //     data type REG_BINARY.
            Binary = 3,
            //
            // ??????:
            //     A 32-bit binary number. This value is equivalent to the Windows API registry
            //     data type REG_DWORD.
            DWord = 4,
            //
            // ??????:
            //     An array of null-terminated strings, terminated by two null characters. This
            //     value is equivalent to the Windows API registry data type REG_MULTI_SZ.
            MultiString = 7,
            //
            // ??????:
            //     A 64-bit binary number. This value is equivalent to the Windows API registry
            //     data type REG_QWORD.
            QWord = 11
        }
    }
}
