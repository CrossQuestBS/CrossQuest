using System.Collections.Generic;
using Newtonsoft.Json;

namespace CrossQuest.Unity.Models;

public static class ScriptingAssembliesExtensions {
    public static string AsJson(this ScriptingAssemblies assemblies)
    {
        return JsonConvert.SerializeObject(assemblies);
    }
}

public record ScriptingAssemblies(
    [JsonProperty("names")] List<string> Names,
    [JsonProperty("types")] List<int> Types
);