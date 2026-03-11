// See https://aka.ms/new-console-template for more information

using CrossQuestBackend;
using CrossQuestBackend.Game;
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

var games = await ResourceDownloader.Games();


var beatSaber = games.First(it => it.Id == "com.beatgames.beatsaber");
var version = beatSaber.ModdableVersionList[0];

// Create new instances
var unityInstance = new UnityInstance(version.UnityVersion);
var instance = new GameInstance(beatSaber.Id, version, version.UnityVersion.Version);

await unityInstance.DownloadFiles();
await instance.DownloadFiles();

