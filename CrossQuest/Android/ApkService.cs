using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CrossQuest.Android.Models;
using CrossQuest.Game;

namespace CrossQuest.Android;

public static class ApkService
{
    public static void CopyJniLibs(GameInstance instance, string extractApkPath)
    {
        var jniLibs = Path.Join(instance.InstancePath, "UnityDependencies", "JniLibs", "arm64-v8a");

        List<string> jniLibsToCopy = ["lib_burst_generated.so", "libunity.so"];

        var libPath = Path.Join(extractApkPath, "lib/arm64-v8a");
        
        var il2cppPathSo = Path.Join(instance.InstancePath, "Build/Native/arm64-v8a/libil2cpp.so");

        foreach (var jniLib in jniLibsToCopy)
        {
            var fileToCopy = Path.Join(jniLibs, jniLib);
            var toPath = Path.Join(libPath, jniLib);
            File.Copy(fileToCopy, toPath, true);
        }

        File.Copy(il2cppPathSo, Path.Join(libPath, "libil2cpp.so"), true);
    }
    
    public static async Task CopyMetadata(CancellationToken cancellationToken, GameInstance instance,
        string extractApkPath, string manifest, string bootConfig)
    {
        var globalMetadata = Path.Join(instance.InstancePath, "Build", "Native", "arm64-v8a", "Data",
            "Metadata",
            "global-metadata.dat");

        var resourcesFolder = Path.Join(instance.InstancePath, "Build", "Native", "arm64-v8a", "Data",
            "Resources");

        File.Copy(globalMetadata,
            Path.Join(extractApkPath, "assets", "bin", "Data", "Managed", "Metadata", "global-metadata.dat"),
            true);

        foreach (var resourceFile in Directory.GetFiles(resourcesFolder))
        {
            var fileName = Path.GetFileName(resourceFile);
            var resourceDir = Path.Join(extractApkPath, "assets", "bin", "Data", "Managed", "Resources");

            File.Copy(resourceFile, Path.Join(resourceDir, fileName), true);
        }
        
        // Required to add a new unity_app_guid to reset il2cpp cache
        await File.WriteAllTextAsync(Path.Join(extractApkPath, "assets", "bin", "Data", "unity_app_guid"),
            Guid.NewGuid().ToString(), cancellationToken);
        // Required for correct permissions
        await File.WriteAllTextAsync(Path.Join(extractApkPath, "AndroidManifest.xml"), manifest,
            cancellationToken);
        // Required for getting correct boot.config
        await File.WriteAllTextAsync(Path.Join(extractApkPath, "assets", "bin", "Data", "boot.config"),
            bootConfig,
            cancellationToken);
        File.Copy(Path.Join(instance.InstancePath, "Resources", "ScriptingAssemblies.json"),
            Path.Join(extractApkPath, "assets/bin/Data/ScriptingAssemblies.json"), true);
    }
    
    public static async Task<bool> ExtractApk(AndroidTools tools, string apkPath, string extractPath)
    {
        return await ProcessCaller.ProcessAsync("java", $"-jar \"{tools.ApktoolJar}\" d --only-manifest \"{apkPath}\" -o \"{extractPath}\" -f", true);
    }
    
    public static async Task<bool> CreateAPK(AndroidTools tools, string apkPath, string extractPath)
    {
        return await ProcessCaller.ProcessAsync("java", $"-jar \"{tools.ApktoolJar}\" b \"{extractPath}\" -o \"{apkPath}\" -f", true);
    }

    private static byte[] ResourceToBytes(string fileName)
    {
        var assembly = typeof(ApkService).GetTypeInfo().Assembly;
        Stream? resource = assembly.GetManifestResourceStream($"CrossQuest.Resources.{fileName}");
        
        using var memoryStream = new MemoryStream();
        resource?.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
    
    public static async Task<bool> SignAPK(AndroidTools tools, string apkPath)
    {
        var temporaryPath = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString());

        Directory.CreateDirectory(temporaryPath);

        var certFile = "debug_cert.crt";
        var keyFile = "debug_key.pk8";

        var keyPath = Path.Join(temporaryPath, keyFile);
        var certPath = Path.Join(temporaryPath, certFile);
        
        await File.WriteAllBytesAsync(keyPath, ResourceToBytes(keyFile));
        await File.WriteAllBytesAsync(certPath, ResourceToBytes(certFile));

        var apkSignerPath = tools.Apksigner;
        var result = await ProcessCaller.ProcessAsync(
            apkSignerPath, 
            $"sign -v --key \"{keyPath}\" --min-sdk-version 32 --cert \"{certPath}\" \"{apkPath}\"");

        Directory.Delete(temporaryPath, true);

        return result;
    }
    
}