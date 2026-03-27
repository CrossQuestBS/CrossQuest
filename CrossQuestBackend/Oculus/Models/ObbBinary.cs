using Newtonsoft.Json;

namespace CrossQuestBackend.Oculus.Models;

public record ObbBinary(
    [JsonProperty("binaryId")] string BinaryId,
    [JsonProperty("fileName")] string FileName
);
