using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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