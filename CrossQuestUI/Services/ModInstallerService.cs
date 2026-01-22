using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using CrossQuestUI.Models;

namespace CrossQuestUI.Services
{
    public static class ModInstallerService
    {
        private static readonly HttpClient Client = new ();
        
        public static async Task<bool> DownloadMods(ModInfo[] mods, string path)
        {
            try
            {
                foreach (var modInfo in mods)
                {
                    var contents = await Client.GetByteArrayAsync(modInfo.DownloadUrl);
                    var modFolder = Path.Join(path, modInfo.Id);
                    var zipFile = modFolder + ".zip";
                    await File.WriteAllBytesAsync(zipFile, contents);

                    await ZipFile.ExtractToDirectoryAsync(zipFile, modFolder);
                
                    DirectoryExtensions.MoveZippedToParent(modFolder);
                    
                    if (File.Exists(zipFile))
                        File.Delete(zipFile);
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
        
    }
}