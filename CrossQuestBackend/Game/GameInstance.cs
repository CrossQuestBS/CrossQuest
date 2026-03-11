using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CrossQuestBackend.Game.Models;
using CrossQuestBackend.Oculus;

namespace CrossQuestBackend.Game;

public class GameInstance
{
    public string InstancePath { get; set; }
    
    public GameVersionInfo GameVersionInfo { get; set; }
    
    public GameInstance(string gameId, GameVersionInfo version, string unityVersion)
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

    public async Task DownloadFiles()
    {
        var unityDependenciesPath = Path.Join(InstancePath, "UnityDependencies");
        
        if (!Directory.Exists(Path.Join(unityDependenciesPath, "dependencies", "Managed")))
            await GameDependenciesDownloader.Dependencies(GameVersionInfo.ReleaseTag,
                unityDependenciesPath);
        
        if (!Directory.Exists(Path.Join(unityDependenciesPath, "JniLibs", "arm64-v8a")))
            await GameDependenciesDownloader.Jni(GameVersionInfo.ReleaseTag,
                unityDependenciesPath);
    }
    
    public async Task DownloadGameFiles(string access_token)
    {
        var oculusPath = Path.Join(InstancePath, "Oculus");
        
        var riftFilePath = Path.Join(oculusPath, GameVersionInfo.RiftConfig.FilesToDownload[0].Replace("\\", "/")); 
        if (!Path.Exists(riftFilePath))
            await OculusDownloader.RiftGame(GameVersionInfo.RiftConfig, access_token, oculusPath);

        var files = Directory.GetFiles(oculusPath);
        if (!files.Any(it => it.EndsWith(".apk")))
            await OculusDownloader.QuestGame(GameVersionInfo.QuestConfig, access_token, oculusPath);
    }
}