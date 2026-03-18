using System;
using System.IO;
using System.Threading.Tasks;
using CrossQuestBackend.Oculus;
using CrossQuestBackend.Oculus.Models;
using Newtonsoft.Json;

namespace CrossQuestBackend.Unity;

public class UnityInstance
{
    public UnityVersionInfo Version { get; set; }
    public string InstancePath { get; set; }

    [JsonConstructor]
    public UnityInstance(UnityVersionInfo version, string instancePath)
    {
        Version = version;
        InstancePath = instancePath;
    }
    
    public UnityInstance(UnityVersionInfo version)
    {
        Version = version; 
        InstancePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrossQuest", "Unity", Version.Version);
        Directory.CreateDirectory(InstancePath);
    }

    public async Task DownloadFiles()
    {
        if (!Directory.Exists(Path.Join(InstancePath, "AndroidPlayer")))
            await UnityDownloader.AndroidPlayer(Version.ReleaseTag, InstancePath);
        
        if (!Directory.Exists(Path.Join(InstancePath, "UnityData")))
            await UnityDownloader.UnityData(Version.ReleaseTag, PlatformService.CurrentPlatform, InstancePath);
    }
}