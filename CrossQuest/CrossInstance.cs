using System;
using System.IO;
using System.Threading.Tasks;
using CrossQuest.Android.Models;
using CrossQuest.Game;
using CrossQuest.Unity;
using Newtonsoft.Json;

namespace CrossQuest;

public class CrossInstance(UnityInstance unityInstance, GameInstance gameInstance, AndroidTools androidTools)
{
    private static string ActiveInstancePathFile =>
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrossQuest", "instancePath.txt");
    
    public static string GetCrossInstancePath(string gameId, string gameVersion) => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrossQuest", "Games", gameId, gameVersion, "CrossInstance.json");

    public UnityInstance UnityInstance { get; set; } = unityInstance;

    public GameInstance GameInstance { get; set; } = gameInstance;
    public AndroidTools AndroidTools { get; set; } = androidTools;

    public static CrossInstance? GetActiveInstance()
    {
        Console.WriteLine("Getting active CrossInstance");
        
        if (!File.Exists(ActiveInstancePathFile))
            throw new Exception($"Failed to find active instance path file at {ActiveInstancePathFile}");

        var pathToFile = File.ReadAllText(ActiveInstancePathFile);

        if (!File.Exists(pathToFile))
            throw new Exception($"Failed to find {pathToFile} are you sure this instance exists?");
            
      
        using (StreamReader file = File.OpenText(pathToFile))
        {

            JsonSerializer serializer = new JsonSerializer();
            return (CrossInstance)serializer.Deserialize(file, typeof(CrossInstance));
        }
    }

    public async Task SaveInstance(string gameId, string gameVersion)
    {
        var instancePath = GetCrossInstancePath(gameId, gameVersion);
        await File.WriteAllTextAsync(instancePath, JsonConvert.SerializeObject(this));
    }

    public static async Task SetAsActiveInstance(string gameId, string gameVersion)
    {
        var instancePath = GetCrossInstancePath(gameId, gameVersion);
        await File.WriteAllTextAsync(ActiveInstancePathFile, instancePath);
    }
}