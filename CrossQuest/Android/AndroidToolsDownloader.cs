using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CrossQuest.Android;

public static class AndroidToolsDownloader
{
    private static readonly HttpClient Client = new();

    private static string GetPlatformString(OSPlatform platform)
    {
        if(platform == OSPlatform.OSX)
            return "darwin";
        if(platform == OSPlatform.Linux)
            return  "linux";
        if(platform == OSPlatform.Windows)
            return "windows";
        throw new InvalidOperationException("Your operating system is not supported.");
    }

    private static readonly string AndroidNDKName = "android-ndk-r29";

    private static async Task<string> DownloadNDKToFile(string ndkDirectory, string postfix)
    {
        var name = $"{AndroidNDKName}-{postfix}";

        var responseByteArray = await Client.GetByteArrayAsync($"https://dl.google.com/android/repository/{name}");

        var tempFolder = Path.GetTempPath();

        var dir = Path.Join(tempFolder, Guid.NewGuid().ToString());

        Directory.CreateDirectory(dir);

        var filePath = Path.Join(dir, name);
        await File.WriteAllBytesAsync(filePath, responseByteArray);

        return filePath;
    }

    private static async Task<string> DownloadNDKForOSX(string ndkDirectory)
    {
        var dmgFilePath = await DownloadNDKToFile(ndkDirectory, "darwin.dmg");

        await ProcessCaller.ProcessAsync("hdiutil", $"attach \"{dmgFilePath}\" -nobrowse -noautoopen", true);

        var ndkVolumePath = Directory.GetDirectories("/Volumes").First(it => it.Contains("Android"));

        var ndkAppDir = Directory.GetDirectories(ndkVolumePath)
            .First(it => it.Contains("AndroidNDK") && it.EndsWith(".app"));

        await ProcessCaller.ProcessAsync("cp", $"-r \"{ndkAppDir}/Contents/NDK\" \"{ndkDirectory}\" ", true);
        await ProcessCaller.ProcessAsync("hdiutil", $"detach \"{ndkVolumePath}\"", true);

        Directory.Delete(Path.GetDirectoryName(dmgFilePath), true);
        return ndkDirectory;
    }

    private static async Task<string> DownloadNDKForLinux(string ndkDirectory)
    {
        var zipFilePath = await DownloadNDKToFile(ndkDirectory, "linux.zip");
        // Need to use external program `unzip`, because .NET does not extract symlinks correctly
        await ProcessCaller.ProcessAsync("unzip", $"\"{zipFilePath}\" -d \"{GetAndroidFolder()}\"");
        Directory.Delete(Path.GetDirectoryName(zipFilePath), true);
        return $"{GetAndroidFolder()}/{AndroidNDKName}";
    }

    public static async Task<string> DownloadNDK()
    {
        var androidFolder = GetAndroidFolder();

        var ndkDirectory = Path.Join(androidFolder, AndroidNDKName);

        if (Directory.Exists(ndkDirectory))
            return ndkDirectory;
        Console.WriteLine($"[Download] Downloading {AndroidNDKName}");
        if (PlatformService.CurrentPlatform == OSPlatform.OSX)
            ndkDirectory = await DownloadNDKForOSX(ndkDirectory);
        else if (PlatformService.CurrentPlatform == OSPlatform.Linux)
            ndkDirectory = await DownloadNDKForLinux(ndkDirectory);
        else
        {
            var requestUrl =
                $"https://dl.google.com/android/repository/{AndroidNDKName}-{GetPlatformString(PlatformService.CurrentPlatform)}.zip";
            var stream = await Client.GetStreamAsync(requestUrl);
            await ZipFile.ExtractToDirectoryAsync(stream, androidFolder);
        }
        Console.WriteLine($"[Download] Done downloading {AndroidNDKName}");
        return ndkDirectory;
    }

    private static string GetAndroidFolder()
    {
        var applicationFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        var androidFolder = Path.Join(applicationFolder, "CrossQuest", "Android");

        Directory.CreateDirectory(androidFolder);
        return androidFolder;
    }

    public static async Task<string> DownloadApktool()
    {
        var androidFolder = GetAndroidFolder();

        var filePath = Path.Join(androidFolder, "apktool.jar");

        if (Path.Exists(filePath))
            return filePath;
        Console.WriteLine($"[Download] Downloading apktool.jar");
        var bytes = await Client.GetByteArrayAsync(
            "https://bitbucket.org/iBotPeaches/apktool/downloads/apktool_3.0.1.jar");
        await File.WriteAllBytesAsync(filePath, bytes);
        Console.WriteLine($"[Download] Done Downloading apktool.jar");
        return filePath;
    }

    public static async Task<string> DownloadApkSigner()
    {
        var androidFolder = GetAndroidFolder();
        var buildToolsPath = Path.Join(androidFolder, "build-tools");

        var fileSuffix = PlatformService.CurrentPlatform == OSPlatform.Windows ? ".bat" : "";

        var apksignerPath = Path.Join(buildToolsPath, $"apksigner{fileSuffix}");

        if (File.Exists(apksignerPath))
            return apksignerPath;


        // Just a check to prevent overriding or error from ZipFile.ExtractToDirectoryAsync
        if (Directory.Exists(Path.Join(androidFolder, "android-14")))
            Directory.Move(Path.Join(androidFolder, "android-14"), Path.Join(androidFolder, "android-14_backup"));

        var platformString = GetPlatformString(PlatformService.CurrentPlatform);

        if (platformString == "darwin")
            platformString = "macosx";

        Console.WriteLine($"[Download] Downloading ApkSigner for platform: {platformString}");
        var requestUrl =
            $"https://dl.google.com/android/repository/build-tools_r34-{platformString}.zip";
        var manifestStream = await Client.GetStreamAsync(requestUrl);

        await ZipFile.ExtractToDirectoryAsync(manifestStream, androidFolder);

        Directory.Move(Path.Join(androidFolder, "android-14"), buildToolsPath);
        Console.WriteLine($"[Download] Done downloading ApkSigner for platform: {platformString}");
        return apksignerPath;
    }

    public static async Task<string> DownloadADB()
    {
        var androidFolder = GetAndroidFolder();

        var fileSuffix = PlatformService.CurrentPlatform == OSPlatform.Windows ? ".exe" : "";

        var adbPath = Path.Join(androidFolder, "platform-tools", $"adb{fileSuffix}");

        if (File.Exists(adbPath))
            return adbPath;

        var platformString = GetPlatformString(PlatformService.CurrentPlatform);

        Console.WriteLine($"[Download] Downloading adb for platform: {platformString}");
        var requestUrl = $"https://dl.google.com/android/repository/platform-tools-latest-{platformString}.zip";
        var manifestStream = await Client.GetStreamAsync(requestUrl);


        await ZipFile.ExtractToDirectoryAsync(manifestStream, androidFolder);
        Console.WriteLine($"[Download] Done downloading adb for platform: {platformString}");
        return adbPath;
    }
}
