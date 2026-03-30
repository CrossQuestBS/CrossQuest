using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CrossQuest.Android;
using CrossQuest.Android.Models;
using CrossQuest.Game.Models;
using CrossQuest.Oculus;
using CrossQuest.Unity;
using CrossQuest.Unity.Compilation;
using CrossQuest.Unity.Models;
using Newtonsoft.Json;

namespace CrossQuest.Game;

public class GameInstance
{
    [JsonProperty("InstancePath")]
    public string InstancePath { get; set; }
    
    [JsonProperty("GameVersionInfo")]
    public GameVersionInfo GameVersionInfo { get; set; }

    [JsonConstructor]
    public GameInstance(string instancePath, GameVersionInfo gameVersionInfo)
    {
        InstancePath = instancePath;
        GameVersionInfo = gameVersionInfo;
    }
    
    public GameInstance(GameVersionInfo version, string gameId)
    {
        GameVersionInfo = version;
        InstancePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrossQuest", "Games", gameId, $"{GameVersionInfo.Version}");
        Directory.CreateDirectory(InstancePath);
        Directory.CreateDirectory(Path.Join(InstancePath, "UnityDependencies"));
        Directory.CreateDirectory(Path.Join(InstancePath, "Resources"));
        Directory.CreateDirectory(Path.Join(InstancePath, "Mods"));
        Directory.CreateDirectory(Path.Join(InstancePath, "Libs"));
        Directory.CreateDirectory(Path.Join(InstancePath, "Oculus"));
    }

    public async Task<bool> RunIL2CPP(UnityInstance unityInstance, string ndkPath)
    {
        return await il2cppCompile.Compile(unityInstance, this, ndkPath);
    }
    
    public async Task<bool> RunPreIL2CPP(UnityInstance unityInstance)
    {
        List<string> assemblyPaths =
        [
            Path.Join(InstancePath, "Libs"),
            Path.Join(InstancePath, "Mods"),
            Path.Join(InstancePath, "Oculus", "Beat Saber_Data", "Managed"),
            Path.Join(InstancePath, "UnityDependencies", "dependencies", "PlayerScriptAssemblies"),
            Path.Join(InstancePath, "UnityDependencies", "dependencies", "Managed"),
            Path.Join(unityInstance.InstancePath, "UnityData", "unityaot-linux"),
            Path.Join(unityInstance.InstancePath, "UnityData", "unityaot-linux", "Facades")
        ];

        List<string> modAndLibAssemblies =
        [
            Path.Join(InstancePath, "Libs"),
            Path.Join(InstancePath, "Libs", "Build"),
            Path.Join(InstancePath, "Mods"),
            Path.Join(InstancePath, "Mods", "Build"),
            Path.Join(InstancePath, "Oculus", "Beat Saber_Data", "Managed"),
            Path.Join(InstancePath, "UnityDependencies", "dependencies", "Managed"),
        ];


        var allFiles = new List<string>();

        BuildCallback.LoadAssemblies(modAndLibAssemblies, allFiles);
        BuildCallback.LoadCallbacks(modAndLibAssemblies, assemblyPaths);
        BuildCallback.RunPreLinkerBuilds(allFiles);
        UnityLinkerHelper.CopyFilesToStaging(unityInstance, this);

        var stagingFiles = Directory.GetFiles(Path.Join(unityInstance.InstancePath, "Temp", "StagingArea")).ToList();

        BuildCallback.RunPostLinkerBuilds(stagingFiles);

        UnityLinkerHelper.GenerateLinkFile(this);

        if (!await UnityLinker.StartCompile(unityInstance, this))
            return false;
        
        
        List<string> unityAssemblies = new List<string>();

        List<string> userAssemblies = new List<string>();
        var libFiles = Directory.GetFiles(Path.Join(InstancePath, "Libs")).Where(it => it.EndsWith(".dll"));
        var modFiles = Directory.GetFiles(Path.Join(InstancePath, "Mods")).Where(it => it.EndsWith(".dll"));
        var beatsaberFiles = Directory.GetFiles(Path.Join(InstancePath, "Oculus", "Beat Saber_Data", "Managed")).Where(it => it.EndsWith(".dll"));

        userAssemblies.AddRange(libFiles);
        userAssemblies.AddRange(modFiles);
        userAssemblies.AddRange(beatsaberFiles);

        var stagingAreaFileNames = stagingFiles.Where(it => it.EndsWith(".dll")).Select(it => Path.GetFileName(it));
        var unityAssembliesFileNames = Directory.GetFiles(Path.Join(InstancePath, "UnityDependencies", "dependencies", "Managed"))
            .Where(it => it.EndsWith(".dll")).Select(it => Path.GetFileName(it)).Where(it => stagingAreaFileNames.Contains(it));

        unityAssemblies.AddRange(unityAssembliesFileNames);

        var scriptingAssemblies = UnityResources.ScriptingAssemblies(unityAssemblies, userAssemblies);
        
        await File.WriteAllTextAsync(Path.Join(InstancePath, "Resources", "ScriptingAssemblies.json"), scriptingAssemblies.AsJson().Replace("Names", "names").Replace("Types", "types"));
        return true;
    }

    public async Task SetupInstance(string OculusToken, UnityInstance unityInstance)
    {
        await unityInstance.DownloadFiles();
        await DownloadFiles();
        await DownloadGameFiles(OculusToken);
        
    }

    public async Task SetupObb(AndroidTools tools)
    {
        var devicePath = $"/sdcard/Android/obb/com.beatgames.beatsaber/{GameVersionInfo.ObbBinary.FileName}";
        if (await AdbService.HasPathOnDevice(tools,
                devicePath))
            return;
        
        var oculusPath = Path.Join(InstancePath, "Oculus");
        await AdbService.PushFile(tools, Path.Join(oculusPath, GameVersionInfo.ObbBinary.FileName), devicePath);
    }
    
    private async Task DownloadFiles()
    {
        var unityDependenciesPath = Path.Join(InstancePath, "UnityDependencies");
        
        if (!Directory.Exists(Path.Join(unityDependenciesPath, "dependencies", "Managed")))
            await GameDependenciesDownloader.Dependencies(GameVersionInfo.ReleaseTag,
                unityDependenciesPath);
        
        if (!Directory.Exists(Path.Join(unityDependenciesPath, "JniLibs", "arm64-v8a")))
            await GameDependenciesDownloader.Jni(GameVersionInfo.ReleaseTag,
                unityDependenciesPath);
    }
    
    private async Task DownloadGameFiles(string access_token)
    {
        var oculusPath = Path.Join(InstancePath, "Oculus");
        
        if (!OculusDownloader.RiftGameExists(GameVersionInfo.RiftConfig, oculusPath))
            await OculusDownloader.RiftGame(GameVersionInfo.RiftConfig, access_token, oculusPath);
        
        if (!OculusDownloader.QuestGameExists(oculusPath))
            await OculusDownloader.QuestGame(GameVersionInfo.QuestConfig, access_token, oculusPath);
        
        if (!OculusDownloader.ObbExists(GameVersionInfo.ObbBinary, oculusPath))
            await OculusDownloader.DownloadObb(GameVersionInfo.ObbBinary, oculusPath, access_token);
    }
}