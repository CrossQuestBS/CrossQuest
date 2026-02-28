using System.Collections.Generic;
using Newtonsoft.Json;

namespace CrossQuestBackend.Compilation;

public class ScriptingAssemblies
{
    private const int UnityAssemblyType = 2;
    private const int UserAssemblyType = 16;

    // TODO: Figure out the path for all platforms
    // unityAssemblies =
    // MacOS: /Applications/Unity/Hub/Editor/6000.0.40f1/PlaybackEngines/AndroidPlayer/Variations/il2cpp/Managed

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