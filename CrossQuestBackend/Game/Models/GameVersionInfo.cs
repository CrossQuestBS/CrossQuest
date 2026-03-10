using CrossQuestBackend.Oculus.Models;
using Newtonsoft.Json;

namespace CrossQuestBackend.Game.Models;

public record GameVersionInfo(
    [JsonProperty("version")] string Version,
    [JsonProperty("unityVersion")] UnityVersionInfo UnityVersion,
    [JsonProperty("releaseTag")] string ReleaseTag
);