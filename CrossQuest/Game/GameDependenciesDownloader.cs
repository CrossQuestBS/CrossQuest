using System;
using System.Formats.Tar;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace CrossQuest.Game;

public static class GameDependenciesDownloader
{
    private static readonly HttpClient Client = new();

    private const string GithubReleaseUrl =
        "https://github.com/CrossQuestBS/BeatSaberDependencies/releases/download";
    
    private static async Task DownloadGithubRelease(string tag, string directory, string fileName)
    {
        var responseMessage = await Client.GetAsync($"{GithubReleaseUrl}/{tag}/{fileName}");
        
        await using GZipStream gZipStream = new GZipStream(await responseMessage.Content.ReadAsStreamAsync(), CompressionMode.Decompress);
        
        await TarFile.ExtractToDirectoryAsync(gZipStream, directory, true);
    }

    public static async Task Jni(string tag, string directory)
    {
        Console.WriteLine($"[Download] Starting to download jni libraries for tag: {tag}");
        await DownloadGithubRelease(tag, directory, $"jni-beatsaber.tar.gz");
        Console.WriteLine($"[Download] Done downloading jni libraries for tag: {tag}");
    }

    public static async Task Dependencies(string tag, string directory)
    {
        Console.WriteLine($"[Download] Downloading dependencies for tag: {tag}");
        await DownloadGithubRelease(tag, directory, "dependencies-beatsaber.tar.gz");
        Console.WriteLine($"[Download] Done downloading dependencies for tag: {tag}");
    }
}