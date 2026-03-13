// See https://aka.ms/new-console-template for more information

using System.Runtime.Loader;
using CrossQuestBackend;
using CrossQuestBackend.Game;
using CrossQuestBackend.Unity;
using CrossQuestBackend.Unity.Compilation;
using CrossQuestBackend.Unity.Models;
using IPA.BuildProcess.Interfaces;

#region Setup Beat Saber + Unity
var games = await ResourceDownloader.Games();
var beatSaber = games.First(it => it.Id == "com.beatgames.beatsaber");
var version = beatSaber.ModdableVersionList[0];

var unityInstance = new UnityInstance(version.UnityVersion);
var instance = new GameInstance(beatSaber.Id, version, version.UnityVersion.Version);

await unityInstance.DownloadFiles();
await instance.DownloadFiles();
await instance.DownloadGameFiles("..._token_here");
#endregion




#region PreIL2CPPCompilation
List<string> assemblyPaths =
[
    Path.Join(instance.InstancePath, "Libs"),
    Path.Join(instance.InstancePath, "Mods"),
    Path.Join(instance.InstancePath, "Oculus/Beat Saber_Data/Managed"),
    Path.Join(instance.InstancePath, "UnityDependencies/dependencies/PlayerScriptAssemblies"),
    Path.Join(instance.InstancePath, "UnityDependencies/dependencies/Managed"),
    Path.Join(unityInstance.InstancePath, "UnityData/unityaot-linux"),
    Path.Join(unityInstance.InstancePath, "UnityData/unityaot-linux/Facades")
];

List<string> modAndLibAssemblies =
[
    Path.Join(instance.InstancePath, "Libs"),
    Path.Join(instance.InstancePath, "Libs/Build"),
    Path.Join(instance.InstancePath, "Mods"),
    Path.Join(instance.InstancePath, "Oculus/Beat Saber_Data/Managed"),
    Path.Join(instance.InstancePath, "UnityDependencies/dependencies/Managed"),
];

var allFiles = new List<string>();

BuildCallback.LoadAssemblies(modAndLibAssemblies, allFiles);
BuildCallback.LoadCallbacks(modAndLibAssemblies, assemblyPaths);

BuildCallback.RunPreLinkerBuilds(allFiles);
UnityLinkerHelper.CopyFilesToStaging(unityInstance, instance);

var stagingFiles = Directory.GetFiles(Path.Join(unityInstance.InstancePath, "Temp", "StagingArea")).ToList();

BuildCallback.RunPostLinkerBuilds(stagingFiles);

UnityLinkerHelper.GenerateLinkFile(instance);

await UnityLinker.StartCompile(unityInstance, instance);

#endregion

#region ScriptingAssemblies
List<string> unityAssemblies = new List<string>();

List<string> userAssemblies = new List<string>();
var libFiles = Directory.GetFiles(Path.Join(instance.InstancePath, "Libs")).Where(it => it.EndsWith(".dll"));
var modFiles = Directory.GetFiles(Path.Join(instance.InstancePath, "Mods")).Where(it => it.EndsWith(".dll"));
var beatsaberFiles = Directory.GetFiles(Path.Join(instance.InstancePath, "Oculus/Beat Saber_Data/Managed")).Where(it => it.EndsWith(".dll"));

userAssemblies.AddRange(libFiles);
userAssemblies.AddRange(modFiles);
userAssemblies.AddRange(beatsaberFiles);

var stagingAreaFileNames = Directory
    .GetFiles(Path.Join(unityInstance.InstancePath, "Temp", "StagingArea"))
    .Where(it => it.EndsWith(".dll")).Select(it => Path.GetFileName(it));
var unityAssembliesFileNames = Directory.GetFiles(Path.Join(instance.InstancePath, "UnityDependencies/dependencies/Managed"))
    .Where(it => it.EndsWith(".dll")).Select(it => Path.GetFileName(it)).Where(it => stagingAreaFileNames.Contains(it));

unityAssemblies.AddRange(unityAssembliesFileNames);

var scriptingAssemblies = UnityResources.ScriptingAssemblies(unityAssemblies, userAssemblies);

var scriptingAssembliesJson = scriptingAssemblies.AsJson();

#endregion

// TODO: Create RuntimeInitializeOnLoads.json

await il2cppCompile.Compile(unityInstance, instance, "/Users/maribell/QPM-RS/ndk/29.0.14206865+preview-0");

Console.WriteLine(scriptingAssembliesJson);

#region Patching++
// TODO: Need a way to get boot.config
// TODO: use apktool to unextract downloaded apk
// TODO: Patch apk with compiled files
// TODO: use apktool to extract apk
// TODO: use apk sign to new apk
// TODO: Uninstall game if needed
// TODO: Clear game cache
// TODO: Install game
// TODO: Fix permissions
#endregion
