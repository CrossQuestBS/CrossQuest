using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CrossQuestBackend.Unity;

public static class UnityDownloader
{
    private static readonly HttpClient Client = new();

    private const string GithubReleaseUrl =
        "https://github.com/CrossQuestBS/UnityCompileTools/releases/download";

    private static string GetPlatformString(OSPlatform platform) =>
        platform == OSPlatform.OSX ? "osx" : platform == OSPlatform.Linux ? "linux" : "windows";

    private static async Task DownloadUnityRelease(string tag, string directory, string fileName)
    {
        var responseMessage = await Client.GetAsync($"{GithubReleaseUrl}/{tag}/{fileName}");
        
        await using GZipStream gZipStream = new GZipStream(await responseMessage.Content.ReadAsStreamAsync(), CompressionMode.Decompress);
        
        await TarFile.ExtractToDirectoryAsync(gZipStream, directory, true);
    }

    public static async Task UnityData(string tag, OSPlatform platform, string directory)
    {
        var unitydataFile = $"unitydata-{GetPlatformString(platform)}.tar.gz";
        Console.WriteLine($"[Download] Downloading {unitydataFile}");
        await DownloadUnityRelease(tag, directory, unitydataFile);
        Console.WriteLine($"[Download] Done downloading {unitydataFile}");
    } 
    
    public static async Task AndroidPlayer(string tag, string directory){
        Console.WriteLine("[Download] Downloading androidplayer.tar.gz");
        await DownloadUnityRelease(tag, directory, "androidplayer.tar.gz");
        Console.WriteLine("[Download] Done downloading androidplayer.tar.gz");
    }
        
}