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
        /// Windows 注册表
        /// </summary>
        public static class Regedit
        {
            public static readonly string CurrentUser = Microsoft.Win32.Registry.CurrentUser.Name;
            public static readonly string LocalMachine = Microsoft.Win32.Registry.LocalMachine.Name;
            /// <summary>
            /// 检索与指定的注册表项中的指定名称关联的值。如果在指定的项中未找到该名称，则返回您提供的默认值；或者，如果指定的项不存在，则返回 null。
            /// </summary>
            /// <param name="keyName">以有效注册表根（如“HKEY_CURRENT_USER”）开头的键的完整注册表路径。</param>
            /// <param name="valueName">名称/值对的名称。</param>
            /// <param name="defaultValue">当 name 不存在时返回的值。</param>
            /// <returns>如果由 keyName 指定的子项不存在，则返回 null；否则，返回与 valueName 关联的值；或者，如果未找到 valueName，则返回 defaultValue。</returns>
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
            /// 检索与指定的注册表项中的指定名称关联的值。如果在指定的项中未找到该名称，则返回您提供的默认值；或者，如果指定的项不存在，则返回 null。
            /// </summary>
            public static object GetValue(string keyName, string valueName, object defaultValue = null) => Microsoft.Win32.Registry.GetValue(keyName, valueName, defaultValue);
            /// <summary>
            /// 设置指定的注册表项的指定名称/值对。如果指定的项不存在，则创建该项。
            /// </summary>
            /// <param name="keyName">以有效注册表根（如“HKEY_CURRENT_USER”）开头的键的完整注册表路径。</param>
            /// <param name="valueName">名称/值对的名称。</param>
            /// <param name="value">要存储的值。</param>
            public static void SetValue(string keyName, string valueName, object value) => Microsoft.Win32.Registry.SetValue(keyName, valueName, value);
            /// <summary>
            /// 通过使用指定的注册表数据类型，设置该指定的注册表项的名称/值对。如果指定的项不存在，则创建该项。
            /// </summary>
            /// <param name="keyName">以有效注册表根（如“HKEY_CURRENT_USER”）开头的键的完整注册表路径。</param>
            /// <param name="valueName">名称/值对的名称。</param>
            /// <param name="value">要存储的值。</param>
            /// <param name="valueKind">存储数据时使用的注册表数据类型。</param>
            public static void SetValue(string keyName, string valueName, object value, RegeditValueKind valueKind) => Microsoft.Win32.Registry.SetValue(keyName, valueName, Enum.TryParse<Microsoft.Win32.RegistryValueKind>(valueKind.ToString(), out var kind) ? kind : Microsoft.Win32.RegistryValueKind.String);
        }
        /// <summary>
        /// Windows 注册表 - 存储数据时使用的注册表数据类型。
        /// </summary>
        public enum RegeditValueKind
        {
            //
            // 摘要:
            //     A null-terminated string. This value is equivalent to the Windows API registry
            //     data type REG_SZ.
            String = 1,
            //
            // 摘要:
            //     A null-terminated string that contains unexpanded references to environment variables,
            //     such as %PATH%, that are expanded when the value is retrieved. This value is
            //     equivalent to the Windows API registry data type REG_EXPAND_SZ.
            ExpandString = 2,
            //
            // 摘要:
            //     Binary data in any form. This value is equivalent to the Windows API registry
            //     data type REG_BINARY.
            Binary = 3,
            //
            // 摘要:
            //     A 32-bit binary number. This value is equivalent to the Windows API registry
            //     data type REG_DWORD.
            DWord = 4,
            //
            // 摘要:
            //     An array of null-terminated strings, terminated by two null characters. This
            //     value is equivalent to the Windows API registry data type REG_MULTI_SZ.
            MultiString = 7,
            //
            // 摘要:
            //     A 64-bit binary number. This value is equivalent to the Windows API registry
            //     data type REG_QWORD.
            QWord = 11
        }
    }
}
