using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CrossQuestBackend.Unity.Compilation;

public static class UnityLinkerHelper
{
    private static List<string> GetDllFiles(string directory) => Directory.GetFiles(directory).Where(path => path.EndsWith(".dll")).ToList();
    
    public static Tuple<List<string>, List<string>> GetLinkerAssemblyPaths(string androidPlayer, string unityData, string gameDependencies)
    {
        var allowedAssemblies = new List<string>();
    
        var managedAssembliesPath = Path.Join(androidPlayer, "Managed");
        var playerScriptAssemblies = Path.Join(gameDependencies, "dependencies/PlayerScriptAssemblies");
        //var additionalAssembliesPrebuilt = Path.Join(config.ProjectPath, ConstantPaths.AdditionalAssembliesPath);
        var unityAotLinux = Path.Join(unityData, "unityaot-linux");
        var unityAotLinuxFacade = Path.Join(unityData, "unityaot-linux/Facades");
        
        allowedAssemblies.AddRange(GetDllFiles(managedAssembliesPath));
        //allowedAssemblies.AddRange(GetDllFiles(additionalAssembliesPrebuilt));
        allowedAssemblies.AddRange(GetDllFiles(playerScriptAssemblies));
        allowedAssemblies.AddRange(GetDllFiles(unityAotLinux));
        allowedAssemblies.AddRange(GetDllFiles(unityAotLinuxFacade));

        List<string> includeDirectory = new()
        {
            managedAssembliesPath,
            playerScriptAssemblies,
            //additionalAssembliesPrebuilt,
            unityAotLinux,
            unityAotLinuxFacade
        };

        return new Tuple<List<string>, List<string>>(allowedAssemblies, includeDirectory);
    }
}