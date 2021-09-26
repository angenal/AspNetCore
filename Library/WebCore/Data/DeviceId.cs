using DeviceId;

namespace WebCore.Data
{
    public sealed class DeviceId
    {
        public static string Get() => new DeviceIdBuilder()
            .AddMachineName()
            .AddOsVersion()
            .OnWindows(x => x.AddMachineGuid())
            .OnLinux(linux => linux
                .AddMotherboardSerialNumber()
                .AddSystemDriveSerialNumber())
            .OnMac(mac => mac
                .AddSystemDriveSerialNumber()
                .AddPlatformSerialNumber())
            .ToString();
    }
}
