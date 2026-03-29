using CrossQuest.Oculus.Models;
using Newtonsoft.Json;

namespace CrossQuest.Game.Models;

[method: JsonConstructor]
public record GameVersionInfo(
    [JsonProperty("version")] string Version,
    [JsonProperty("unityVersion")] UnityVersionInfo UnityVersion,
    [JsonProperty("releaseTag")] string ReleaseTag,
    [JsonProperty("riftConfig")] RiftDownloadConfig RiftConfig,
    [JsonProperty("questConfig")] QuestDownloadConfig QuestConfig,
    [JsonProperty("obbBinary")] ObbBinary ObbBinary
);