using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CrossQuestBackend.Unity.Models;

namespace CrossQuestBackend.Unity.Compilation;

public static class UnityResources
{

    private static string ResourceToString(string fileName)
    {
        var assembly = typeof(UnityResources).GetTypeInfo().Assembly;
        Stream? resource = assembly.GetManifestResourceStream($"CrossQuestBackend.Resources.{fileName}");
        
        var reader = new StreamReader(resource);

        return reader.ReadToEnd();
    }
    
    public static string BootConfig()
    {
        return ResourceToString("boot.config");
    }
    
    public static string Manifest()
    {
        return ResourceToString("AndroidManifest.xml");
    }
    
    public static string LinkFile()
    {
        return ResourceToString("link.xml");
    }
    
    public static ScriptingAssemblies ScriptingAssemblies(List<string> unityAssemblies, List<string> userAssemblies)
    {
        const int UnityAssemblyType = 2;
        const int UserAssemblyType = 16;
        List<string> names = [];
        List<int> types = [];

        foreach (var unityAssembly in unityAssemblies)
        {
            names.Add(Path.GetFileName(unityAssembly));
            types.Add(UnityAssemblyType);
        }

        foreach (var userAssembly in userAssemblies)
        {
            names.Add(Path.GetFileName(userAssembly));
            types.Add(UserAssemblyType);
        }

        return new ScriptingAssemblies(names, types);
    }

    public static async Task RuntimeInitializeOnLoads(string linkerOutputPath, string unityDataPath, string outputPath)
    {
        var dotnetRunPath = Path.Join(unityDataPath, "netcorerun/netcorerun");
        var arguments = new List<string>()
        {
            Path.Join(unityDataPath, "BuildPlayerDataGenerator", "BuildPlayerDataGenerator.exe"),
            "-s=" + linkerOutputPath,
            "-rn=\"RuntimeInitializeOnLoads.json\"",
            "-o=" + outputPath
        };
        
        foreach (var file in Directory.GetFiles(linkerOutputPath).Where(it => it.EndsWith(".dll")))
        {
            arguments.Add("-a=" + file);
        }
        
        await ProcessCaller.ProcessAsync(dotnetRunPath, String.Join(" ", arguments));
    }
}