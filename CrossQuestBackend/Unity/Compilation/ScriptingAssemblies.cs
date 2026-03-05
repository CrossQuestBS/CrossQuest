using System.Collections.Generic;
using Newtonsoft.Json;

namespace CrossQuestBackend.Unity.Compilation;

public class ScriptingAssemblies
{
    private const int UnityAssemblyType = 2;
    private const int UserAssemblyType = 16;
        
    public ScriptingAssemblies(List<string> unityAssemblies, List<string> userAssemblies)
    {
        names = [];
        types = [];

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
    }

    public List<string> names;
    public List<int> types;

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }
}