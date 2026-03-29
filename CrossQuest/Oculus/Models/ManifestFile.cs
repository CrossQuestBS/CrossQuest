using Newtonsoft.Json;

namespace CrossQuestBackend.Oculus.Models;

public record ManifestFile(
    [JsonProperty("sha256")] string Sha256,
    [JsonProperty("size")] int Size,
    [JsonProperty("segmentSize")] int SegmentSize,
    [JsonProperty("segments")] object[][] Segments
);