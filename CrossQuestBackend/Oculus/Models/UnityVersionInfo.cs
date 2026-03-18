using Newtonsoft.Json;

namespace CrossQuestBackend.Oculus.Models;

[method: JsonConstructor]
public record UnityVersionInfo(
    [JsonProperty("version")] string Version,
    [JsonProperty("releaseTag")] string ReleaseTag
);