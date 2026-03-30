using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CrossQuest.Oculus.Models;
using Newtonsoft.Json;

namespace CrossQuest.Oculus;

// TODO:
// - Add OBB Downloader (Requires GraphQL call)
public static class OculusDownloader
{
    private static readonly HttpClient Client = new();
    private static string ManifestUrl(string manifestId, string accessToken) =>
        $"https://securecdn.oculus.com/binaries/download/?id={manifestId}&access_token={accessToken}&get_manifest=1";

    private static string SegmentUrl(string binaryId, string segmentSha256, string accessToken) =>
        $"https://securecdn.oculus.com/binaries/segment/?access_token={accessToken}&binary_id={binaryId}&segment_sha256={segmentSha256}";

    private static string QuestURL(string binaryId, string accessToken) =>
        $"https://securecdn.oculus.com/binaries/download/?id={binaryId}&access_token={accessToken}";

    public static bool ObbExists(ObbBinary obbBinary, string path) => 
        Directory.GetFiles(path).Any(it => it.Contains(obbBinary.FileName));

    public static async Task DownloadObb(ObbBinary obbBinary, string path, string accessToken)
    {
        Console.WriteLine($"[Download] Downloading obb: {obbBinary.BinaryId} - {obbBinary.FileName}");
        var responseMessage = await Client.GetAsync(
            QuestURL(obbBinary.BinaryId, accessToken)
        );

        var contentDisposition = responseMessage.Content.Headers.ContentDisposition;
        if (contentDisposition is null)
            throw new FileNotFoundException($"obb with binaryId {obbBinary.BinaryId} not found");

      

        var filePath = Path.Join(path, obbBinary.FileName);

        await using FileStream outputFileStream = File.Create(filePath);
        await responseMessage.Content.CopyToAsync(outputFileStream);
        Console.WriteLine($"[Download] Done downloading obb file: {obbBinary.BinaryId} - {obbBinary.FileName}");
    }

    public static async Task<Manifest?> Manifest(string manifestId, string accessToken)
    {
        var manifestStream = await Client.GetStreamAsync(ManifestUrl(manifestId, accessToken));

        await using ZipArchive arc = new ZipArchive(manifestStream);

        if (arc.Entries.Count <= 0) return null;

        var manifest = arc.Entries[0];
        var stream = await manifest.OpenAsync();

        using StreamReader sr = new StreamReader(stream);
        var reader = new JsonTextReader(sr);

        return new JsonSerializer().Deserialize<Manifest>(reader);
    }

    public static bool RiftGameExists(RiftDownloadConfig downloadConfig,
        string downloadPath) =>
        Path.Exists(Path.Join(downloadPath, downloadConfig.FilesToDownload[0].Replace("\\", "/")));

    public static bool QuestGameExists(string path) =>
        Directory.GetFiles(path).Any(it => it.EndsWith(".apk"));

    public static async Task<bool> RiftGame(RiftDownloadConfig downloadConfig, string accessToken,
        string downloadPath)
    {
        Console.WriteLine($"[Download] Downloading rift game: {downloadConfig.AppId} - {downloadConfig.Version}");
        var manifest = await Manifest(downloadConfig.BinaryId, accessToken);

        if (manifest is null)
            return false;

        for (int i = 0; i < downloadConfig.FilesToDownload.Count; i++)
        {
            var file = downloadConfig.FilesToDownload[i];
            var filePath = file.Replace("\\", "/");
            Console.WriteLine($"[Download({i}/{downloadConfig.FilesToDownload.Count})] Downloading file {file}");
            if (!manifest.Files.TryGetValue(file, out var manifestFile)) continue;

            var path = Path.Join(downloadPath, filePath);

            var parentPath = Directory.GetParent(path);
            if (parentPath is null)
                continue;

            Directory.CreateDirectory(parentPath.FullName);


            List<byte> fileBytes = new();

            await DownloadSegments(downloadConfig.BinaryId, accessToken, manifestFile, fileBytes);
            await SaveSegmentFile(fileBytes.ToArray(), path);
        }
        Console.WriteLine($"[Download] Done downloading rift game: {downloadConfig.AppId} - {downloadConfig.Version}");
        return true;
    }

    public static async Task QuestGame(QuestDownloadConfig config, string accessToken, string path)
    {
        Console.WriteLine($"[Download] Downloading quest game: {config.AppId} - {config.Version}");
        var responseMessage = await Client.GetAsync(
            QuestURL(config.BinaryId, accessToken)
        );

        var contentDisposition = responseMessage.Content.Headers.ContentDisposition;
        if (contentDisposition is null)
            throw new FileNotFoundException($"APK with binaryId {config.BinaryId} not found");

        var downloadFileName = contentDisposition.FileName;
        var versionedFile = GetVersionedFilename(config, downloadFileName);

        var filePath = Path.Join(path, versionedFile);

        await using FileStream outputFileStream = File.Create(filePath);
        await responseMessage.Content.CopyToAsync(outputFileStream);
        Console.WriteLine($"[Download] Done downloading quest game: {config.AppId} - {config.Version}");
    }

    private static async Task SaveSegmentFile(byte[] bytes, string path)
    {
        using var stream = new MemoryStream(bytes);
        await using var decompressor = new ZLibStream(stream, CompressionMode.Decompress);
        await using FileStream outputFileStream = File.Create(path);
        await decompressor.CopyToAsync(outputFileStream);
    }

    private static async Task DownloadSegments(string binaryId, string accessToken,
        ManifestFile manifestFile, List<byte> fileBytes)
    {
        foreach (var segment in manifestFile.Segments)
        {
            var requestUrl = SegmentUrl(binaryId, (string)segment[1], accessToken);
            var bytes = await Client.GetByteArrayAsync(requestUrl);
            fileBytes.AddRange(bytes);
        }
    }
    
    private static string GetVersionedFilename(QuestDownloadConfig config, string? downloadFileName)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(downloadFileName);
        var fileExtension = Path.GetExtension(downloadFileName);

        var finalFileName = fileNameWithoutExtension + $"_{config.Version}{fileExtension}";
        return finalFileName;
    }
}