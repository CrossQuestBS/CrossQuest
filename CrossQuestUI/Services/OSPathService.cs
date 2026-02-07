using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CrossQuestUI.Services
{
    public static class OSPathService
    {
        private static OSPlatform UnknownPlatform = OSPlatform.Create("Unknown");
        
        private static Dictionary<OSPlatform, string> UnityHubDefaultPath = new ()
        {
            { OSPlatform.Windows, @"C:\Program Files\Unity Hub\Unity Hub.exe" },
            { OSPlatform.Linux, @"~/Applications/Unity\ Hub.AppImage" },
            { OSPlatform.OSX, @"/Applications/Unity\ Hub.app/Contents/MacOS/Unity\ Hub" },
            { UnknownPlatform, ""}
        };
        
        public static OSPlatform CurrentPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? OSPlatform.Windows
            : (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? OSPlatform.OSX
                : (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OSPlatform.Linux : UnknownPlatform));

        public static string UnityHub => UnityHubDefaultPath[CurrentPlatform];
    }
}