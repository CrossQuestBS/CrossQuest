using System.Collections.Generic;
using Newtonsoft.Json;

namespace CrossQuest.Game.Models;

public record GameInfo(
    [JsonProperty("id")] string Id,
    [JsonProperty("moddableVersionList")] List<GameVersionInfo> ModdableVersionList
);