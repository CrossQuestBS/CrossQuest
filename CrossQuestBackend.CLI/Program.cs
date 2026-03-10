// See https://aka.ms/new-console-template for more information

using CrossQuestBackend.Game;
using CrossQuestBackend.Unity.Compilation;
using CrossQuestBackend.Unity.Models;

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