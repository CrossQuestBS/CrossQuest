using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CrossQuestBackend.Unity.Models;

public static class ScriptingAssembliesExtensions
{
    public static void Save(this ScriptingAssemblies self, string path)
    {
        File.WriteAllText(path, JsonConvert.SerializeObject(self));
    }

    public static async Task SaveAsync(this ScriptingAssemblies self, string path)
    {
        await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(self));
    }
}

public record ScriptingAssemblies(
    [JsonProperty("names")] List<string> Names,
    [JsonProperty("types")] List<int> Types
);