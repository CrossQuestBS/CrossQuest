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
    private static string InstancePath =>
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrossQuest", "Instance.json");
    public UnityInstance UnityInstance { get; set; } = unityInstance;

    public GameInstance GameInstance { get; set; } = gameInstance;
    public AndroidTools AndroidTools { get; set; } = androidTools;

    public static CrossInstance? GetActiveInstance()
    {
        Console.WriteLine("Getting active CrossInstance");
      
        using (StreamReader file = File.OpenText(InstancePath))
        {
            JsonSerializer serializer = new JsonSerializer();
            return (CrossInstance)serializer.Deserialize(file, typeof(CrossInstance));
        }
    }

    public async Task SetAsActiveInstance()
    {
        Console.WriteLine("Setting CrossInstance as active");
        await File.WriteAllTextAsync(InstancePath, JsonConvert.SerializeObject(this));
    }
}