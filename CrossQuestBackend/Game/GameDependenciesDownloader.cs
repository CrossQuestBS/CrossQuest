using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CrossQuestBackend.Game;

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

    public static async Task Jni(string tag, string directory) => 
        await DownloadGithubRelease(tag, directory, $"jni-beatsaber.tar.gz");
    
    public static async Task Dependencies(string tag, string directory) =>
        await DownloadGithubRelease(tag, directory, "dependencies-beatsaber.tar.gz");
}