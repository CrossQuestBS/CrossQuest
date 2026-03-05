using System;
using System.IO;
using Avalonia.Platform;

namespace CrossQuestUI.Services
{
    public static class ResourceManager
    {
        public static void ExtractAssetFile(string fileName, string path)
        {
            Uri textFileUri = new ($"avares://CrossQuestUI/Assets/{fileName}");
            var stream = AssetLoader.Open(textFileUri);
            
            if (File.Exists(path))
                File.Delete(path);
            
            using var fileStream = File.Create(path);
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(fileStream);
        }

    }
}