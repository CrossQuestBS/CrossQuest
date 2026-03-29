using Newtonsoft.Json;

namespace CrossQuestBackend.Oculus.Models;

[method: JsonConstructor]
public record QuestDownloadConfig(
    [JsonProperty("appId")] string AppId,
    [JsonProperty("version")] string Version,
    [JsonProperty("binaryId")] string BinaryId
);