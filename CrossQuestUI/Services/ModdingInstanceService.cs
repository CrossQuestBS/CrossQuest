using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CrossQuestUI.Models;

namespace CrossQuestUI.Services
{
    public static class ModdingInstanceService
    {
        private static string InstancePath => Path.Join(App.Current?.ApplicationPath, "Instances");
        
        public static async Task<ModdingInstance[]> GetInstanceList()
        {
            List<ModdingInstance> instances = [];
            Directory.CreateDirectory(InstancePath);

            foreach (var directory in Directory.GetDirectories(InstancePath))
            {
                var instanceFile = Path.Join(directory, "instance.json");
                if (!File.Exists(instanceFile))
                    continue;
                
                var jsonString = await File.ReadAllTextAsync(instanceFile);
                var moddingInstance = JsonSerializer.Deserialize<ModdingInstance>(jsonString);
                instances.Add(moddingInstance);
            }

            return instances.ToArray();
        }

        public static async Task<ModdingInstance> CreateInstance(string unityPath, string version, string apkPath, string gamePath, ModInfo[] mods)
        {
            var guid = Guid.NewGuid();
            var path = Path.Join(InstancePath, guid.ToString());

            Directory.CreateDirectory(path);

            var instance = new ModdingInstance(path, unityPath, version, gamePath, mods);

            var jsonString = JsonSerializer.Serialize(instance);
            
            File.Copy(apkPath, Path.Join(path, "BaseGame.apk"));
            
            await File.WriteAllTextAsync(Path.Join(path, "instance.json"), jsonString);
            return instance;
        }
    }
}