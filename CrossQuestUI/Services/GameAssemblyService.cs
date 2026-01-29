using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BepInEx.AssemblyPublicizer;

namespace CrossQuestUI.Services
{
    public static class GameAssemblyService
    {
        private static readonly HttpClient Client = new ();

        public static async Task<Dictionary<string, string[]>> GetAssembliesMapping(string version)
        {
            var contents = await Client.GetStringAsync($"https://github.com/CrossQuestBS/UnityBaseProject/raw/refs/heads/{version}/.CrossQuest/assemblies.json");

            var result = JsonSerializer.Deserialize<Dictionary<string, string[]>>(contents);

            return result ?? throw new Exception("Failed to deserialize assemblies mapping!");
        }

        private static void RevertPublicizedAssembly(string originalFilePath, string publicizedPath, string pluginsPath)
        {
            File.Delete(publicizedPath);
            File.Delete(pluginsPath);
            File.Copy(originalFilePath, pluginsPath);
        }

        private static void AddPublicizedAssembly(string originalFilePath, string publicizedPath, string pluginsPath)
        {
            File.Delete(pluginsPath);
            AssemblyPublicizer.Publicize(originalFilePath, pluginsPath);
            File.Copy(originalFilePath, publicizedPath, true);
        }

        public static void CopyPublicizedAssemblies(string assembliesPath, string unityProjectPath,
            string[] publicizedAssemblies)
        {
            var publicizedPath = Path.Join(unityProjectPath, "Publicized");

            Directory.CreateDirectory(publicizedPath);

            var publicizedExisting = Directory.GetFiles(publicizedPath).Select(Path.GetFileName).Where(it => it.EndsWith(".dll")).ToArray();

            var revertedAssemblies = publicizedExisting.Where(it => !publicizedAssemblies.Contains(it)).ToString();
            
            
            var pluginPath = Path.Join(unityProjectPath, "Assets", "Plugins");

            foreach (var directory in Directory.GetDirectories(pluginPath))
            {
                foreach (var dllFile in Directory.GetFiles(directory))
                {
                    var fileName = Path.GetFileName(dllFile);
                    var assembliesFile = Path.Join(assembliesPath, fileName);
                    var publicizedFilePath = Path.Join(publicizedPath, fileName);

                    if (revertedAssemblies != null && revertedAssemblies.Contains(fileName))
                    {
                        RevertPublicizedAssembly(assembliesFile, publicizedFilePath, dllFile);
                        continue;
                    }

                    if (!publicizedAssemblies.Contains(fileName)) continue;
                    AddPublicizedAssembly(assembliesFile, publicizedFilePath, dllFile);
                }
            }
        }

        public static void CopyAssemblies(string assembliesPath, string pluginsPath, Dictionary<string, string[]> assemblies, bool overrideFiles = false)
        {
            var pluginFolders = Directory.GetDirectories(pluginsPath);

            foreach (var keyPair in assemblies)
            {
                var pluginPath = pluginFolders.First(t => Path.GetFileName(t) == keyPair.Key);

                if (pluginPath == "")
                    continue;
                
                foreach (var assemblyFile in keyPair.Value)
                {
                    var file = Path.Join(assembliesPath, assemblyFile);
                    var unityAssemblyPath = Path.Join(pluginPath, Path.GetFileName(file));
                    
                    if (File.Exists(unityAssemblyPath) && !overrideFiles)
                        continue;
                    
                    File.Copy(file, unityAssemblyPath, true);
                }
            }
            
        }
    }
}