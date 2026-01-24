using System;
using System.IO;
using System.Threading.Tasks;

namespace CrossQuestUI.Services
{
    public static class QuestService
    {
        private const string AdbPathTemplate = "{0}/SDK/platform-tools/adb";
        private const string ApkSignerTemplate = "{0}/SDK/build-tools/34.0.0/apksigner";

        public static async Task ClearCache(string androidPlayerPath)
        {
            var adbPath = string.Format(AdbPathTemplate, androidPlayerPath);

            await ProcessCallerService.ProcessAsync(adbPath, "shell pm clear com.beatgames.beatsaber");
        }
        
        public static async Task<bool> SignApk(string apkPath, string androidPlayerPath)
        {
            var temporaryPath = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString());

            Directory.CreateDirectory(temporaryPath);

            var certFile = "debug_cert.crt";
            var keyFile = "debug_key.pk8";

            var keyPath = Path.Join(temporaryPath, keyFile);
            var certPath = Path.Join(temporaryPath, certFile);

            ResourceManager.ExtractAssetFile(keyFile, keyPath);
            ResourceManager.ExtractAssetFile(certFile, certPath);

            var apkSignerPath = string.Format(ApkSignerTemplate, androidPlayerPath);
            var result = await ProcessCallerService.ProcessAsync(
                apkSignerPath, 
                $"sign -v --key \"{keyPath}\" --cert \"{certPath}\" \"{apkPath}\"");

            Directory.Delete(temporaryPath, true);

            return result;
        }

        public static async Task<bool> ExtractApk(string apkPath, string outputPath)
        {
            return await ProcessCallerService.ProcessAsync("apktool", $"d \"{apkPath}\" -o \"{outputPath}\" -f", true);
        }

        public static async Task<bool> UpdateGamePermissions(string packageId, string androidPlayerPath)
        {
            var adbPath = string.Format(AdbPathTemplate, androidPlayerPath);
            return await ProcessCallerService.ProcessAsync(adbPath,
                $"shell appops set --uid {packageId} MANAGE_EXTERNAL_STORAGE allow");
        }

        public static async Task<bool> BuildApk(string folder, string apkPath)
        {
            return await ProcessCallerService.ProcessAsync("apktool", $"b \"{folder}\" -o \"{apkPath}\" -f", true);

        }

        public static bool CopyApkFiles(string buildPath, string apkPath)
        {
            var libPath = "lib/arm64-v8a";
            var assetDataPath = "assets/bin/Data";
            
            string[] filesToCopy =
            [
                $"{assetDataPath}/boot.config", 
                $"{assetDataPath}/ScriptingAssemblies.json",
                $"{assetDataPath}/RuntimeInitializeOnLoads.json",
                $"{assetDataPath}/Managed/Metadata/global-metadata.dat"
            ];

            ResourceManager.ExtractAssetFile("AndroidManifest.xml", Path.Join(apkPath, "AndroidManifest.xml"));
            
            foreach (var file in Directory.GetFiles(Path.Join(buildPath, $"{assetDataPath}/Managed/Resources")))
            {
                File.Copy(file, Path.Join(apkPath, $"{assetDataPath}/Managed/Resources", Path.GetFileName(file)), true);
            }
            
            foreach (var file in Directory.GetFiles(Path.Join(buildPath, $"{libPath}")))
            {
                File.Copy(file, Path.Join(apkPath, $"{libPath}", Path.GetFileName(file)), true);
            }

            foreach (var filePath in filesToCopy)
            {
                var extractedFilePath = Path.Join(buildPath, filePath);
                var unpackedFilePath = Path.Join(apkPath, filePath);
                File.Copy(extractedFilePath, unpackedFilePath, true);
            }

            return true;
        }
        
    }
}