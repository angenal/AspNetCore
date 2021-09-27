using WebCore.IO;

namespace WebCore.Platform.Posix.Linux
{
    /// <summary>
    /// Provides information about, and means to manipulate, the current environment and platform. This class cannot be inherited.
    /// </summary>
    public class Environment
    {
        /// <summary></summary>
        public static string CPUInfo => new FileMD5("/proc/cpuinfo", true).GetValue();
        /// <summary></summary>
        public static string MachineID => new FileMD5(new[] { "/var/lib/dbus/machine-id", "/etc/machine-id" }).GetValue();
        /// <summary></summary>
        public static string ProductUUID => new FileMD5("/sys/class/dmi/id/product_uuid").GetValue();
        /// <summary></summary>
        public static string MotherboardSerialNumber => new FileMD5("/sys/class/dmi/id/board_serial").GetValue();
        /// <summary></summary>
        public static string SystemDriveSerialNumber => new SystemDriveSerialNumber().GetValue();
    }
}
