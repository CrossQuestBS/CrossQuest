using System.Collections.Generic;
using System.IO;
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

        await using FileStream outputFileStream = File.Create(Path.Join(directory, fileName));
        
        await responseMessage.Content.CopyToAsync(outputFileStream);
    }

    public static async Task UnityData(string tag, OSPlatform platform, string directory) => 
        await DownloadUnityRelease(tag, directory, $"unitydata-{GetPlatformString(platform)}.tar.gz");
    
    public static async Task AndroidPlayer(string tag, string directory) =>
        await DownloadUnityRelease(tag, directory, "androidplayer.tar.gz");
}