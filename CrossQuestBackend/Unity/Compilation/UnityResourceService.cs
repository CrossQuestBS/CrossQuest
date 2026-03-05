using System.Collections.Generic;
using CrossQuestBackend.Unity.Models;

namespace CrossQuestBackend.Unity.Compilation;

public static class UnityResourceService
{
    private const int UnityAssemblyType = 2;
    private const int UserAssemblyType = 16;

    public static ScriptingAssemblies GenerateScriptingAssemblies(List<string> unityAssemblies,
        List<string> userAssemblies)
    {
        List<string> names = [];
        List<int> types = [];

        foreach (var assembly in unityAssemblies)
        {
            names.Add(assembly);
            types.Add(UnityAssemblyType);
        }

        foreach (var assembly in userAssemblies)
        {
            names.Add(assembly);
            types.Add(UserAssemblyType);
        }

        return new ScriptingAssemblies(names, types);
    }
}