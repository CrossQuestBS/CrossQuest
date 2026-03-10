using Newtonsoft.Json;

namespace CrossQuestBackend.Oculus.Models;

public record UnityVersionInfo (
    [JsonProperty("version")] string Version,
    [JsonProperty("releaseTag")] string ReleaseTag
);