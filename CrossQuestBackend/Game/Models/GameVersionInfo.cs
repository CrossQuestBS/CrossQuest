using CrossQuestBackend.Oculus.Models;
using Newtonsoft.Json;

namespace CrossQuestBackend.Game.Models;

[method: JsonConstructor]
public record GameVersionInfo(
    [JsonProperty("version")] string Version,
    [JsonProperty("unityVersion")] UnityVersionInfo UnityVersion,
    [JsonProperty("releaseTag")] string ReleaseTag,
    [JsonProperty("riftConfig")] RiftDownloadConfig RiftConfig,
    [JsonProperty("questConfig")] QuestDownloadConfig QuestConfig
);