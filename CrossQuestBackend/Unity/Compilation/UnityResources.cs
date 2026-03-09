using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CrossQuestBackend.Unity.Models;
using Newtonsoft.Json;

namespace CrossQuestBackend.Unity.Compilation;

public static class UnityResources
{
    public static ScriptingAssemblies ScriptingAssemblies(List<string> unityAssemblies, List<string> userAssemblies)
    {
        const int UnityAssemblyType = 2;
        const int UserAssemblyType = 16;
        
        List<string> names = [];
        List<int> types = [];

        foreach (var assembly in unityAssemblies.Select(Path.GetFileName))
        {
            if (assembly is null)
                continue;
            names.Add(assembly);
            types.Add(UnityAssemblyType);
        }

        foreach (var assembly in userAssemblies.Select(Path.GetFileName))
        {
            if (assembly is null)
                continue;
            
            names.Add(assembly);
            types.Add(UserAssemblyType);
        }

        return new ScriptingAssemblies(names, types);
    }
}