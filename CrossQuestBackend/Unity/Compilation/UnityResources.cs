using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CrossQuestBackend.Unity.Models;

namespace CrossQuestBackend.Unity.Compilation;

public static class UnityResources
{
    public static string BootConfig()
    {
        return $@"gfx-enable-gfx-jobs=1
gfx-enable-native-gfx-jobs=1
gfx-threading-mode=4
wait-for-native-debugger=0
hdr-display-enabled=0
xrsdk-pre-init-library=UnityOpenXR
xr-meta-enabled=1
xr-vulkan-extension-fragment-density-map-enabled=1
xr-latelatching-enabled=0
xr-latelatchingdebug-enabled=0
xr-low-latency-audio-enabled=1
xr-require-backbuffer-textures=0
xr-keyboard-overlay-enabled=1
xr-pipeline-cache-enabled=1
xr-skip-B10G11R11-special-casing=1
xr-hide-memoryless-render-texture=1
xr-skip-audio-buffer-size-check=1
xr-usable-core-mask-enabled=1
androidStartInFullscreen=1
androidRenderOutsideSafeArea=0
build-guid=201592bc64a74fd2aa4a2632c86769d7";
    }
    
    public static string Manifest()
    {
        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<manifest xmlns:android=""http://schemas.android.com/apk/res/android"" android:compileSdkVersion=""32"" android:compileSdkVersionCodename=""12"" android:installLocation=""auto"" package=""com.beatgames.beatsaber"" platformBuildVersionCode=""32"" platformBuildVersionName=""12"">
    <supports-screens android:anyDensity=""true"" android:largeScreens=""true"" android:normalScreens=""true"" android:smallScreens=""true"" android:xlargeScreens=""true""/>
    <uses-permission android:name=""android.permission.INTERNET""/>
    <uses-permission android:name=""android.permission.ACCESS_NETWORK_STATE""/>
    <uses-permission android:name=""android.permission.READ_EXTERNAL_STORAGE""/>
    <uses-feature android:name=""android.hardware.vr.headtracking"" android:required=""true"" android:version=""1""/>
    <uses-feature android:name=""oculus.software.overlay_keyboard"" android:required=""false""/>
    <uses-feature android:glEsVersion=""0x00030000""/>
    <uses-feature android:name=""android.hardware.touchscreen"" android:required=""false""/>
    <uses-feature android:name=""android.hardware.touchscreen.multitouch"" android:required=""false""/>
    <uses-feature android:name=""android.hardware.touchscreen.multitouch.distinct"" android:required=""false""/>
    <queries>
        <package android:name=""com.oculus.store""/>
    </queries>
    <application android:allowBackup=""false"" android:appComponentFactory=""androidx.core.app.CoreComponentFactory"" android:debuggable=""true"" android:extractNativeLibs=""true"" android:icon=""@mipmap/app_icon"" android:label=""@string/app_name"" android:networkSecurityConfig=""@xml/network_sec_config"">
        <meta-data android:name=""unityplayer.SkipPermissionsDialog"" android:value=""false""/>
        <meta-data android:name=""com.oculus.supportedDevices"" android:value=""quest2|questpro|quest3|quest3s""/>
        <meta-data android:name=""unity.splash-mode"" android:value=""0""/>
        <meta-data android:name=""unity.splash-enable"" android:value=""true""/>
        <meta-data android:name=""unity.launch-fullscreen"" android:value=""true""/>
        <meta-data android:name=""unity.render-outside-safearea"" android:value=""false""/>
        <meta-data android:name=""unity.auto-report-fully-drawn"" android:value=""true""/>
        <meta-data android:name=""unity.auto-set-game-state"" android:value=""true""/>
        <meta-data android:name=""unity.strip-engine-code"" android:value=""false""/>
        <activity android:configChanges=""mcc|mnc|locale|touchscreen|keyboard|keyboardHidden|navigation|orientation|screenLayout|uiMode|screenSize|smallestScreenSize|density|layoutDirection|fontScale"" android:enabled=""true"" android:excludeFromRecents=""true"" android:exported=""true"" android:hardwareAccelerated=""false"" android:launchMode=""singleTask"" android:name=""com.unity3d.player.UnityPlayerGameActivity"" android:resizeableActivity=""false"" android:screenOrientation=""landscape"" android:theme=""@style/Theme.AppCompat.DayNight.NoActionBar"">
            <intent-filter>
                <category android:name=""android.intent.category.LAUNCHER""/>
                <category android:name=""com.oculus.intent.category.VR""/>
                <action android:name=""android.intent.action.MAIN""/>
            </intent-filter>
            <meta-data android:name=""com.oculus.vr.focusaware"" android:value=""true""/>
        </activity>
        <meta-data android:name=""com.oculus.ossplash"" android:value=""true""/>
        <meta-data android:name=""com.oculus.ossplash.type"" android:value=""mono""/>
        <meta-data android:name=""com.oculus.ossplash.colorspace"" android:value=""P3""/>
        <meta-data android:name=""com.oculus.ossplash.background"" android:value=""black""/>
        <provider android:authorities=""com.beatgames.beatsaber.androidx-startup"" android:exported=""false"" android:name=""androidx.startup.InitializationProvider"">
            <meta-data android:name=""androidx.emoji2.text.EmojiCompatInitializer"" android:value=""androidx.startup""/>
            <meta-data android:name=""androidx.lifecycle.ProcessLifecycleInitializer"" android:value=""androidx.startup""/>
        </provider>
    </application>
    <uses-permission android:name=""android.permission.WRITE_EXTERNAL_STORAGE""/>
    <uses-permission android:name=""android.permission.MANAGE_EXTERNAL_STORAGE""/>
</manifest>";
    }
    
    public static ScriptingAssemblies ScriptingAssemblies(List<string> unityAssemblies, List<string> userAssemblies)
    {
        const int UnityAssemblyType = 2;
        const int UserAssemblyType = 16;
        List<string> names = [];
        List<int> types = [];

        foreach (var unityAssembly in unityAssemblies)
        {
            names.Add(Path.GetFileName(unityAssembly));
            types.Add(UnityAssemblyType);
        }

        foreach (var userAssembly in userAssemblies)
        {
            names.Add(Path.GetFileName(userAssembly));
            types.Add(UserAssemblyType);
        }

        return new ScriptingAssemblies(names, types);
    }

    public static async Task RuntimeInitializeOnLoads(string linkerOutputPath, string unityDataPath, string outputPath)
    {
        var dotnetRunPath = Path.Join(unityDataPath, "netcorerun/netcorerun");
        var arguments = new List<string>()
        {
            Path.Join(unityDataPath, "BuildPlayerDataGenerator", "BuildPlayerDataGenerator.exe"),
            "-s=" + linkerOutputPath,
            "-rn=\"RuntimeInitializeOnLoads.json\"",
            "-o=" + outputPath
        };
        
        foreach (var file in Directory.GetFiles(linkerOutputPath).Where(it => it.EndsWith(".dll")))
        {
            arguments.Add("-a=" + file);
        }
        
        await ProcessCaller.ProcessAsync(dotnetRunPath, String.Join(" ", arguments));
    }
}