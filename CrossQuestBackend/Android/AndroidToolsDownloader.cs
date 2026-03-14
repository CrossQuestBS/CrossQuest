using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CrossQuestBackend.Android;

public static class AndroidToolsDownloader
{
    private static readonly HttpClient Client = new();

    private static string GetPlatformString(OSPlatform platform) =>
        platform == OSPlatform.OSX ? "darwin" : platform == OSPlatform.Linux ? "linux" : "windows";

    public static async Task DownloadNDK()
    {
        var androidFolder = GetAndroidFolder();
        
        if (Directory.Exists(Path.Join(androidFolder, "android-ndk-r27d")))
            return;
        
        var requestUrl =
            $"https://dl.google.com/android/repository/android-ndk-r27d-{GetPlatformString(PlatformService.CurrentPlatform)}.zip";

        var stream = await Client.GetStreamAsync(requestUrl);
        
        await ZipFile.ExtractToDirectoryAsync(stream, androidFolder);
    }

    private static string GetAndroidFolder()
    {
        var applicationFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        var androidFolder = Path.Join(applicationFolder, "CrossQuest", "Android");

        Directory.CreateDirectory(androidFolder);
        return androidFolder;
    }

    public static async Task DownloadApktool()
    {
        var androidFolder = GetAndroidFolder();


        var filePath = Path.Join(androidFolder, "apktool.jar");

        if (Path.Exists(filePath))
            return;

        var bytes = await Client.GetByteArrayAsync(
            "https://bitbucket.org/iBotPeaches/apktool/downloads/apktool_3.0.1.jar");
        await File.WriteAllBytesAsync(filePath, bytes);
    }

    public static async Task DownloadBuildTools()
    {
        var androidFolder = GetAndroidFolder();

        if (Directory.Exists(Path.Join(androidFolder, "build-tools")))
            return;

        // Just a check to prevent overriding or error from ZipFile.ExtractToDirectoryAsync
        if (Directory.Exists(Path.Join(androidFolder, "android-14")))
            Directory.Move(Path.Join(androidFolder, "android-14"), Path.Join(androidFolder, "android-14_backup"));

        var platformString = GetPlatformString(PlatformService.CurrentPlatform);

        if (platformString == "darwin")
            platformString = "macosx";
        
        
        var requestUrl =
            $"https://dl.google.com/android/repository/build-tools_r34-{platformString}.zip";
        var manifestStream = await Client.GetStreamAsync(requestUrl);


        await ZipFile.ExtractToDirectoryAsync(manifestStream, androidFolder);

        Directory.Move(Path.Join(androidFolder, "android-14"), Path.Join(androidFolder, "build-tools"));
    }

    public static async Task DownloadPlatformTools()
    {
        var androidFolder = GetAndroidFolder();
        
        if (Directory.Exists(Path.Join(androidFolder, "platform-tools")))
            return;

        var platformString = GetPlatformString(PlatformService.CurrentPlatform);

        var requestUrl = $"https://dl.google.com/android/repository/platform-tools-latest-{platformString}.zip";
        var manifestStream = await Client.GetStreamAsync(requestUrl);


        await ZipFile.ExtractToDirectoryAsync(manifestStream, androidFolder);
    }
}