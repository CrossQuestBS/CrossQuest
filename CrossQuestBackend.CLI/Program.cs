// See https://aka.ms/new-console-template for more information
using CrossQuestBackend.Game.Models;
using CrossQuestBackend.Oculus.Models;
using CrossQuestBackend.Unity;
using CrossQuestBackend.Unity.Compilation;
using CrossQuestBackend.Unity.Models;
using Newtonsoft.Json;

Console.WriteLine("Hello, World!");

List<string> unityAssemblies = new List<string>()
{
    "/Hello/UnityAssembly.dll"
};

List<string> userAssemblies = new List<string>()
{
    "/Hello/UserAssembly.dll"
};

var scriptingAssemblies = UnityResources.ScriptingAssemblies(unityAssemblies, userAssemblies);

var assemblies = scriptingAssemblies.AsJson();

Console.WriteLine(assemblies);

var unityInstance = new UnityInstance("6000.0.40f1");
var instance = new GameInstance("com.beatgames.beatsaber", "1.42.2", "6000.0.40f1");

var unityVersion = new UnityVersionInfo("6000.0.40f1", "6000.0.40f1-1.1");
var latestGameVersion = new GameVersionInfo("1.42.2", unityVersion, "1.42.2-1.0");
var versions = new List<GameVersionInfo>
{
    latestGameVersion
};
var gameInfo = new GameInfo("BeatSaber", versions);

Console.WriteLine(gameInfo);
Console.WriteLine(JsonConvert.SerializeObject(gameInfo));
