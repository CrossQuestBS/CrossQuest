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

        public async Task<bool> SetupProject()
        {
            if (HasProject)
                return true;

            if (Mods.Length == 0)
                return false;

            bool createdProject = await UnityEditorService.CreateProject(UnityPath, UnityProjectPath);

            if (!createdProject)
                throw new Exception("Failed to create project");

            var installBaseProject = await UnityEditorService.InstallBaseProject(Version,
                Path.Join(ModdingPath, "BaseUnityProject"), UnityProjectPath);

            if (!installBaseProject)
                throw new Exception("Failed to install base unity project");

            Directory.CreateDirectory(ModsFolderPath);

            var downloadMods = await ModInstallerService.DownloadMods(Mods, ModsFolderPath);

            if (!downloadMods)
                throw new Exception("Failed to download mods");

            var assemblies = await GameAssemblyService.GetAssembliesMapping(Version);

            GameAssemblyService.CopyAssemblies(GameAssembliesPath, Path.Join(UnityProjectPath, "Assets", "Plugins"),
                assemblies);
            
            await File.WriteAllTextAsync(Path.Join(ModdingPath, ".CrossQuest", ".initialized"), "Exists!");
            return true;
        }

        public async Task<bool> CompileProject()
        {
            return await UnityEditorService.CompileProject(UnityPath, UnityProjectPath, ProjectBuildPath);
        }

        
        
        public async Task<bool> BuildModdedApk()
        {
            var tempPath = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString());
            var buildPath = Path.Join(tempPath, "build");
            var baseGamePath = Path.Join(tempPath, "base-game");
            
            await QuestService.ExtractApk(ProjectBuildPath, buildPath);
            await QuestService.ExtractApk(BaseApkPath, baseGamePath);

            QuestService.CopyApkFiles(buildPath, baseGamePath);
            await QuestService.BuildApk(baseGamePath, Path.Join(ModdingPath, $"ModdedGame_{Guid.NewGuid().ToString()}.apk"));
            return true;
        }
    }
}