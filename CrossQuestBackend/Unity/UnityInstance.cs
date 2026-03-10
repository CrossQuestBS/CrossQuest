using System;
using System.IO;

namespace CrossQuestBackend.Unity;

public class UnityInstance
{
    public string Version { get; set; }
    public string InstancePath { get; set; }
    
    public UnityInstance(string version)
    {
        Version = version; 
        InstancePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrossQuest", "Unity", Version);
        Directory.CreateDirectory(InstancePath);
    }

}