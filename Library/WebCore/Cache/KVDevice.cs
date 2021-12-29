using FASTER.core;
using System.Runtime.InteropServices;

namespace WebCore.Cache
{
    /// <summary>
    /// Used to various devices parameter, Cannot use LocalStorageDevice from non-Windows OS platform
    /// </summary>
    public enum KVDeviceType
    {
        LocalMemoryDevice,
        LocalStorageDevice,
        ManagedLocalStorageDevice
    }
    /// <summary>
    /// Faster Key/Value is a fast concurrent persistent log and key-value store with cache for larger-than-memory data.
    /// https://github.com/microsoft/FASTER
    /// </summary>
    public class KVDevice
    {
        /// <summary>
        /// Create a memory device
        /// </summary>
        /// <param name="testDeviceType">Used to various devices parameter, Cannot use LocalStorageDevice from non-Windows OS platform</param>
        /// <param name="filename">File name (or prefix) with path, if LocalMemoryDevice to use default virtual path for the device</param>
        /// <param name="capacity">Default 128 MB (1L << 27) is enough for our cases</param>
        /// <param name="sz_segment">Default 4 MB (1L << 22) size of each segment</param>
        /// <param name="parallelism">Number of IO processing threads</param>
        /// <param name="sector_size">Sector size for device (default 64)</param>
        /// <param name="latencyMs">Induced callback latency in ms (for testing purposes)</param>
        /// <returns></returns>
        public static IDevice CreateMemoryDevice(KVDeviceType testDeviceType, string filename = "/userspace/ram/storage",
            long capacity = 1L << 27, long sz_segment = 1L << 22, int parallelism = 2, uint sector_size = 64, int latencyMs = 0)
        {
            IDevice device = null;
            // Cannot use LocalStorageDevice from non-Windows OS platform
            if (testDeviceType == KVDeviceType.LocalStorageDevice && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                testDeviceType = KVDeviceType.LocalMemoryDevice;

            switch (testDeviceType)
            {
                case KVDeviceType.LocalStorageDevice:
                    device = new LocalStorageDevice(filename, false, false, true, capacity, false, false);
                    break;
                case KVDeviceType.ManagedLocalStorageDevice:
                    device = new ManagedLocalStorageDevice(filename, false, false, capacity, false);
                    break;
                case KVDeviceType.LocalMemoryDevice:
                    device = new LocalMemoryDevice(capacity, sz_segment, parallelism, latencyMs, sector_size, filename);
                    break;
            }

            return device;
        }
    }
}
