using System.Runtime.InteropServices;

namespace CrossQuest;

public class PlatformService
{
    private static OSPlatform UnknownPlatform = OSPlatform.Create("Unknown");

    public static OSPlatform CurrentPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? OSPlatform.Windows
        : (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? OSPlatform.OSX
            : (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OSPlatform.Linux : UnknownPlatform));
}