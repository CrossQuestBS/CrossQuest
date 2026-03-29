using System.Collections.Generic;
using Newtonsoft.Json;

namespace CrossQuest.Oculus.Models;

public record Manifest(
    [JsonProperty("appId")] string AppId,
    [JsonProperty("canonicalName")] string CanonicalName,
    [JsonProperty("isCore")] bool IsCore,
    [JsonProperty("packageType")] string PackageType,
    [JsonProperty("version")] string Version,
    [JsonProperty("files")] Dictionary<string, ManifestFile> Files
);