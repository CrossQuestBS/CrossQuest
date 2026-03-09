using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CrossQuestBackend.Unity.Models;

namespace CrossQuestBackend.Unity.Compilation;

public static class UnityResources
{
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
}