using System.Collections.Generic;
using Newtonsoft.Json;

namespace CrossQuestBackend.Unity.Resource;

internal record ScriptingAssemblies(
    [JsonProperty("names")] List<string> Names,
    [JsonProperty("types")] List<int> Types
);