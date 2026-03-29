using Newtonsoft.Json;

namespace CrossQuest.Oculus.Models;

public record ObbBinary(
    [JsonProperty("binaryId")] string BinaryId,
    [JsonProperty("fileName")] string FileName
);
