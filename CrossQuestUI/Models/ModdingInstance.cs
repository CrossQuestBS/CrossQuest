using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CrossQuestUI.Services;

namespace CrossQuestUI.Models
{
    public class ModdingInstance
    {
        public ModdingInstance(string moddingPath, string unityPath, string version, string gamePath, ModInfo[] mods)
        {
            Version = version;
            ModdingPath = moddingPath;
            UnityPath = unityPath;
            GamePath = gamePath;
            Mods = mods;
        }

        public string UnityPath { get; set; }

        public string ModdingPath { get; set; }
        public string Version { get; set; }
        public string GamePath { get; set; }
        public ModInfo[] Mods { get; set; }
        
        private bool HasProject => File.Exists(Path.Join(ModdingPath, ".CrossQuest", ".initialized"));

        private string UnityProjectPath => Path.Join(ModdingPath, $"UnityProject_{Version}");

        private string ModsFolderPath => Path.Join(UnityProjectPath, "Assets", "Plugins", "Mods");

        private string GameAssembliesPath => Path.Join(GamePath, "Beat Saber_Data", "Managed");

        private string ProjectBuildPath => Path.Join(ModdingPath, "build.apk");

        private string BaseApkPath => Path.Join(ModdingPath, "BaseGame.apk");
        private string UnityEditorPath => Path.Join(UnityPath, "Contents/MacOS/Unity");

        private string AndroidPlayer =>
            Path.Join(Directory.GetParent(UnityPath).FullName, "PlaybackEngines/AndroidPlayer");

        public async Task<bool> SetupProject()
        {
            if (HasProject)
                return true;

            if (Mods.Length == 0)
                return false;

            bool createdProject = await UnityEditorService.CreateProject(UnityEditorPath, UnityProjectPath);

            if (!createdProject)
                throw new Exception("Failed to create project");

            var getModdableGame =
                App.Current.ModdableGames.FirstOrDefault(it => it.Name == "Beat Saber" && it.Version == Version);
            
            var installBaseProject = await UnityEditorService.InstallBaseProject(
                Path.Join(ModdingPath, "BaseUnityProject"), UnityProjectPath, getModdableGame.BaseProjectUrl);

            if (!installBaseProject)
                throw new Exception("Failed to install base unity project");

            Directory.CreateDirectory(ModsFolderPath);

            var downloadMods = await ModInstallerService.DownloadMods(Mods, ModsFolderPath);

            if (!downloadMods)
                throw new Exception("Failed to download mods");

            var assemblies = await GameAssemblyService.GetAssembliesMapping(Version);

            GameAssemblyService.CopyAssemblies(GameAssembliesPath, Path.Join(UnityProjectPath, "Assets", "Plugins"),
                assemblies);

            Directory.CreateDirectory(Path.Join(ModdingPath, ".CrossQuest"));
            
            await File.WriteAllTextAsync(Path.Join(ModdingPath, ".CrossQuest", ".initialized"), "Exists!");
            return true;
        }

        public async Task<bool> CompileProject()
        {
            return await UnityEditorService.CompileProject(UnityEditorPath, UnityProjectPath, ProjectBuildPath);
        }
        
        public async Task<bool> BuildModdedApk()
        {
            var tempPath = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                var buildPath = Path.Join(tempPath, "build");
                var baseGamePath = Path.Join(tempPath, "base-game");

                await QuestService.ExtractApk(ProjectBuildPath, buildPath);
                await QuestService.ExtractApk(BaseApkPath, baseGamePath);

                QuestService.CopyApkFiles(buildPath, baseGamePath);
                var moddedGamePath = Path.Join(ModdingPath, $"ModdedGame_{Guid.NewGuid().ToString()}.apk");
                await QuestService.BuildApk(baseGamePath, moddedGamePath);
                await QuestService.SignApk(moddedGamePath, AndroidPlayer);
                await QuestService.ClearCache(AndroidPlayer);
            }
            catch (Exception e)
            {
                Directory.Delete(tempPath, true);
                Console.WriteLine(e);
                return false;
            }
            Directory.Delete(tempPath, true);
            return true;
        }
    }
}