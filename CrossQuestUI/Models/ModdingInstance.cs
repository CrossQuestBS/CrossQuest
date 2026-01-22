using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CrossQuestUI.Services;

namespace CrossQuestUI.Models
{
    public class ModdingInstance(string ModdingPath, string UnityPath, string Version, string GamePath, ModInfo[] mods)
    {
        
        private bool HasProject => File.Exists(Path.Join(ModdingPath, ".CrossQuest", ".initialized"));

        private string UnityProjectPath => Path.Join(ModdingPath, $"UnityProject_{Version}");

        private string ModsFolderPath => Path.Join(UnityProjectPath, "Assets", "Plugins", "Mods");

        private string GameAssembliesPath => Path.Join(GamePath, "Beat Saber_Data", "Managed");
        
        private string ProjectBuildPath => Path.Join(ModdingPath, "build.apk");
        
        public async Task<bool> SetupProject()
        {
            if (HasProject)
                return true;

            if (mods.Length == 0)
                return false;

            bool createdProject = await UnityEditorService.CreateProject(UnityPath, UnityProjectPath);

            if (!createdProject)
                throw new Exception("Failed to create project");

            var installBaseProject = await UnityEditorService.InstallBaseProject(Version, Path.Join(ModdingPath, "BaseUnityProject"), UnityProjectPath);
            
            if (!installBaseProject)
                throw new Exception("Failed to install base unity project");

            Directory.CreateDirectory(ModsFolderPath);

            var downloadMods = await ModInstallerService.DownloadMods(mods, ModsFolderPath);
            
            if (!downloadMods)
                throw new Exception("Failed to download mods");

            var assemblies = await GameAssemblyService.GetAssembliesMapping(Version);
            
            GameAssemblyService.CopyAssemblies(GameAssembliesPath, Path.Join(UnityProjectPath, "Assets", "Plugins"), assemblies);
            
            return true;
        }

        public async Task<bool> CompileProject()
        {
            return await UnityEditorService.CompileProject(UnityPath, UnityProjectPath, ProjectBuildPath);
        }

        public async Task<bool> PostCompile()
        {
            
        }
    }
}