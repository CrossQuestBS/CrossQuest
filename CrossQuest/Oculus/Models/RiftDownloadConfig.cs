using System.Collections.Generic;
using Newtonsoft.Json;

namespace CrossQuestBackend.Oculus.Models;

[method: JsonConstructor]
public record RiftDownloadConfig(
    [JsonProperty("appId")] string AppId,
    [JsonProperty("version")] string Version,
    [JsonProperty("binaryId")] string BinaryId,
    [JsonProperty("filesToDownload")] List<string> FilesToDownload
);