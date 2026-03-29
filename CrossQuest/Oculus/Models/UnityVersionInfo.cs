using Newtonsoft.Json;

namespace CrossQuest.Oculus.Models;

[method: JsonConstructor]
public record UnityVersionInfo(
    [JsonProperty("version")] string Version,
    [JsonProperty("releaseTag")] string ReleaseTag
);