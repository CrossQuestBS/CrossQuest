using CrossQuestBackend;
using CrossQuestBackend.Android;
using CrossQuestBackend.Android.Models;
using CrossQuestBackend.Game;
using CrossQuestBackend.Unity;
using CrossQuestBackend.Unity.Compilation;

var games = await ResourceDownloader.Games();
var beatSaber = games.First(it => it.Id == "com.beatgames.beatsaber");
var version = beatSaber.ModdableVersionList[0];

var unityInstance = new UnityInstance(version.UnityVersion);
var instance = new GameInstance(beatSaber.Id, version);
await instance.SetupInstance("", unityInstance);

// TODO: Let users decide to download or use their own!
var apkSignerPath = await AndroidToolsDownloader.DownloadApkSigner();
var adb = await AndroidToolsDownloader.DownloadADB();
var apktoolJar = await AndroidToolsDownloader.DownloadApktool();
var ndkPath = await AndroidToolsDownloader.DownloadNDK();

var androidTools = new AndroidTools(ndkPath, apkSignerPath, adb, apktoolJar);

// Mods here (?)

if (!await instance.RunPreIL2CPP(unityInstance))
{
    Console.WriteLine("something went wrong during pre il2cpp step");
    return;
}

if (!await instance.RunIL2CPP(unityInstance, ndkPath))
{
    Console.WriteLine("SOMETHING WENT WRONG during compilation!");
    return;
}

var bootConfig = UnityResources.BootConfig();
var manifest = UnityResources.Manifest();

var tempPath = Path.GetTempPath() + Guid.NewGuid();

Directory.CreateDirectory(tempPath);

var gameApk = Directory.GetFiles(Path.Join(instance.InstancePath, "Oculus"))
    .First(it => it.Contains("beat-saber") && it.EndsWith("apk"));

var extractApkPath = Path.Join(tempPath, "beat-saber");
Console.WriteLine($"Extracting APK to {extractApkPath}");

if (!await ApkService.ExtractApk(androidTools, gameApk, extractApkPath))
{
    Console.WriteLine("Failed to extract APK!");
    return;
}

var jniLibs = Path.Join(instance.InstancePath, "UnityDependencies", "JniLibs", "arm64-v8a");

List<string> jniLibsToCopy = ["lib_burst_generated.so", "libunity.so"];

var libPath = Path.Join(extractApkPath, "lib/arm64-v8a");

#region Copy Libs
var il2cppPathSo = Path.Join(instance.InstancePath, "Build/Native/arm64-v8a/libil2cpp.so");

foreach (var jniLib in jniLibsToCopy)
{
    var fileToCopy = Path.Join(jniLibs, jniLib);
    var toPath = Path.Join(libPath, jniLib);
    File.Copy(fileToCopy, toPath, true);   
}

File.Copy(il2cppPathSo, Path.Join(libPath, "libil2cpp.so"), true);
#endregion

#region Copy Metadata

var globalMetadata = Path.Join(instance.InstancePath, "Build", "Native", "arm64-v8a", "Data", "Metadata", "global-metadata.dat");

var resourcesFolder = Path.Join(instance.InstancePath, "Build", "Native", "arm64-v8a", "Data", "Resources");

File.Copy(globalMetadata, Path.Join(extractApkPath, "assets", "bin", "Data", "Managed", "Metadata", "global-metadata.dat"), true);

foreach (var resourceFile in Directory.GetFiles(resourcesFolder))
{
    var fileName = Path.GetFileName(resourceFile);
    var resourceDir = Path.Join(extractApkPath, "assets", "bin", "Data", "Managed", "Resources");
    
    File.Copy(resourceFile, Path.Join(resourceDir, fileName), true);
}
#endregion

// Required to add a new unity_app_guid to reset il2cpp cache
await File.WriteAllTextAsync(Path.Join(extractApkPath, "assets", "bin", "Data", "unity_app_guid"), Guid.NewGuid().ToString());
// Required for correct permissions
await File.WriteAllTextAsync(Path.Join(extractApkPath, "AndroidManifest.xml"), manifest);
// Required for getting correct boot.config
await File.WriteAllTextAsync(Path.Join(extractApkPath, "assets", "bin", "Data", "boot.config"), bootConfig);
File.Copy(Path.Join(instance.InstancePath, "Resources", "ScriptingAssemblies.json"), Path.Join(extractApkPath, "assets/bin/Data/ScriptingAssemblies.json"), true);

await ApkService.CreateAPK(androidTools, Path.Join(instance.InstancePath, "Build", "Modded.apk"), extractApkPath);
await ApkService.SignAPK(androidTools, Path.Join(instance.InstancePath, "Build", "Modded.apk"));

if (!await AdbService.IsDeviceConnected(androidTools))
{
    Console.WriteLine("Quest headset not connected, not installing game");
    return;
}

await AdbService.InstallAPK(androidTools, Path.Join(instance.InstancePath, "Build", "Modded.apk"));
await AdbService.StartGame(androidTools);

// TODO: Uninstall game if needed
// TODO: Fix permissions