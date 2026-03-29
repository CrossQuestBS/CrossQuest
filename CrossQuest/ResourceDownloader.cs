using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CrossQuest.Game.Models;
using Newtonsoft.Json;

namespace CrossQuest;

public static class ResourceDownloader
{
    private static readonly HttpClient Client = new();

    public static async Task<GameInfo[]> Games()
    {
        var stream = await Client.GetStreamAsync("https://raw.githubusercontent.com/CrossQuestBS/Resources/refs/heads/main/games.json");

        using StreamReader sr = new StreamReader(stream);
        var reader = new JsonTextReader(sr);

        return new JsonSerializer().Deserialize<GameInfo[]>(reader);
    }
}