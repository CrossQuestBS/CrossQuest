using System.Collections.Generic;
using System.IO;
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
        
        private static Dictionary<OSPlatform, string> UnityExecutables = new ()
        {
            { OSPlatform.Windows, @"Unity.exe" },
            { OSPlatform.Linux, @"Unity" },
            { OSPlatform.OSX, @"Contents/MacOS/Unity" },
            { UnknownPlatform, ""}
        };
        
        private static Dictionary<OSPlatform, string> AndroidPlayerPaths = new ()
        {
            { OSPlatform.Windows, @"PlaybackEngines/AndroidPlayer" },
            { OSPlatform.Linux, @"PlaybackEngines/AndroidPlayer" },
            { OSPlatform.OSX, @"PlaybackEngines/AndroidPlayer" },
            { UnknownPlatform, ""}
        };
        
        private static Dictionary<OSPlatform, string> AdbPaths = new ()
        {
            { OSPlatform.Windows, @"/SDK/platform-tools/adb.exe" },
            { OSPlatform.Linux, @"/SDK/platform-tools/adb" },
            { OSPlatform.OSX, @"/SDK/platform-tools/adb" },
            { UnknownPlatform, ""}
        };
        
        private static Dictionary<OSPlatform, string> ApkSignerPaths = new ()
        {
            { OSPlatform.Windows, @"/SDK/build-tools/34.0.0/apksigner.exe" },
            { OSPlatform.Linux, @"/SDK/build-tools/34.0.0/apksigner" },
            { OSPlatform.OSX, @"/SDK/build-tools/34.0.0/apksigner" },
            { UnknownPlatform, ""}
        };
        
        public static OSPlatform CurrentPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? OSPlatform.Windows
            : (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? OSPlatform.OSX
                : (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OSPlatform.Linux : UnknownPlatform));

        public static string UnityHub => UnityHubDefaultPath[CurrentPlatform];

        public static string UnityExecutable(string unityPath) =>
            Path.Join(unityPath, UnityExecutables[CurrentPlatform]);
        
        public static string AndroidPlayer(string unityPath)
        {
            var prefixPath = CurrentPlatform == OSPlatform.OSX ? Directory.GetParent(unityPath).FullName : unityPath;
            
            return Path.Join(prefixPath, AndroidPlayerPaths[CurrentPlatform]);
        }

        public static string AdBPath(string androidPlayer) => Path.Join(androidPlayer, AdbPaths[CurrentPlatform]);
        public static string ApkSignerPath(string androidPlayer) => Path.Join(androidPlayer, ApkSignerPaths[CurrentPlatform]);
    }
}