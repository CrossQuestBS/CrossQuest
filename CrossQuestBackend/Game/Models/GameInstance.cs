using System;
using System.IO;

namespace CrossQuestBackend.Game.Models;

public class GameInstance
{
    public string InstancePath { get; set; }
    
    public GameInstance(string gameId, string version, string unityVersion)
    {
        InstancePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrossQuest", "Games", gameId, $"{version}");
        Directory.CreateDirectory(InstancePath);
    }
}