using System.Collections.Generic;
using CrossQuestBackend.Unity.Resource;
using Newtonsoft.Json;

namespace CrossQuestBackend.Unity.Services;

public static class UnityResourceService
{
    public static string ScriptingAssemblies(List<string> unityAssemblies,
        List<string> userAssemblies)
    {
        const int UnityAssemblyType = 2;
        const int UserAssemblyType = 16;
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

        return JsonConvert.SerializeObject(new ScriptingAssemblies(names, types));
    }
}