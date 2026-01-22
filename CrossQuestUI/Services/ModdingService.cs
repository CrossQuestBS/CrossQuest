using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using CrossQuestUI.Models;

namespace CrossQuestUI.Services
{
    public class ModdingService : IModdingService
    {
        private readonly IDownloader _downloader;

        private readonly IUnityEditor _editor;
        
        private readonly IAndroidService _androidService;

        private readonly IFilesService _filesService;

        private readonly IStreamLogger _logger;

        private string _moddingPath = "";

        private string _unityProject = "";

        private string _resourceFolder = "";

        private string _buildPath = "";
        

        private void CreateLinkerFile(string linkPath, Dictionary<string, string[]> assemblies)
        {
            List<string> listOfEntries = [];


            foreach (var keyPair in assemblies)
            {
                listOfEntries.AddRange(keyPair.Value);
            }

            string[] unityFiles =
            [
                "UnityEngine.CoreModule",
                "UnityEngine.UI",
                "UnityEngine.UIModule",
                "UnityEngine.AnimationModule",
                "UnityEngine.ParticleSystemModule",
                "UnityEngine.PhysicsModule",
                "UnityEngine.VFXModule",
                "UnityEngine.XRModule",
                "UnityEngine.Timeline",
                "UnityEngine.ProBuilder",
                "UnityEngine.ProBuilder.KdTree",
                "UnityEngine.ProBuilder.Csg",
                "UnityEngine.ProBuilder.Poly2Tri",
                "UnityEngine.Mathematics"
            ];

            listOfEntries.AddRange(unityFiles);

            var linkContent = CreateLink(listOfEntries.ToArray());
            File.WriteAllText(linkPath, linkContent);
        }

        private string CreateLink(string[] files)
        {
            string output = "<linker>\n";
            foreach (var file in files)
            {
                output += String.Format("   <assembly fullname=\"" + file.Split("/").Last().Split(".dll").First() +
                                        "\" preserve=\"all\"/>\n");
            }

            output += "</linker>";
            return output;
        }

        private bool CopyGameFiles(Dictionary<string, string[]> assemblies)
        {
            var plugins = Path.Join(_unityProject, "Assets", "Plugins");
            var pluginFolders = Directory.GetDirectories(plugins);

            foreach (var keyPair in assemblies)
            {
                _logger.WriteMessage($"Copying assembly family {keyPair.Key}");
                var pluginPath = pluginFolders.First(t => t.Split("/").Last() == keyPair.Key);

                if (pluginPath == "")
                {
                    Console.WriteLine("Failed to find path for key: " + keyPair.Key);
                    continue;
                }

                foreach (var assemblyFile in keyPair.Value)
                {
                    var file = Path.Join(App.Current?.ModdingConfig.GamePath, "Beat Saber_Data", "Managed",
                        assemblyFile);
                    var unityAssemblyPath = Path.Join(pluginPath, Path.GetFileName(file));
                    
                    if (File.Exists(unityAssemblyPath))
                        continue;
                    
                    File.Copy(file, unityAssemblyPath, true);
                }
            }

            return true;
        }

        public ModdingService(IDownloader downloader, IUnityEditor editor, IAndroidService androidService,
            IStreamLogger logger, IFilesService filesService)
        {
            _downloader = downloader;
            _editor = editor;
            _logger = logger;
            _androidService = androidService;
            _filesService = filesService;
        }

        public async Task<bool> PrepareStep()
        {
            _logger.WriteMessage("Preparing");
            var downloadDir = Path.Join(_moddingPath, "Downloads");
            
            _resourceFolder = Path.Join(_moddingPath, "Resources", Guid.NewGuid().ToString());
            
            var modsDownloadDir = Path.Join(downloadDir, "Mods");

            Directory.CreateDirectory(_resourceFolder);

            var unityBase = Path.Join(downloadDir, "UnityBase.zip");

            _logger.WriteMessage("Unzipping UnityBase.zip");

            await ZipFile.ExtractToDirectoryAsync(unityBase, Path.Join(_resourceFolder, "UnityBaseProject"));

            MoveUpFolder(Path.Join(_resourceFolder, "UnityBaseProject"));

            Directory.CreateDirectory(modsDownloadDir);

            var baseGameApk = Path.Join(_resourceFolder, "BaseGame.apk");


            _logger.WriteMessage($"Copying {App.Current?.ModdingConfig.ApkPath} to {baseGameApk}");

            File.Copy(App.Current.ModdingConfig.ApkPath, baseGameApk);

            _logger.WriteMessage("Installing Mods");
            foreach (var modInfo in App.Current.ModdingConfig.ModsInstalled)
            {
                var zipFile = Path.Join(modsDownloadDir, $"{modInfo.Id}.zip");
                var modFolder = Path.Join(_resourceFolder, "Mods", $"{modInfo.Id}");

                _logger.WriteMessage($"Installing Mod: {modInfo.Name}");
                await _downloader.DownloadFile(modInfo.DownloadUrl, zipFile);
                await ZipFile.ExtractToDirectoryAsync(zipFile, modFolder);
                MoveUpFolder(modFolder);
            }

            _logger.WriteMessage("Done Installing Mods");
            return true;
        }

        private void MoveUpFolder(string folder)
        {
            var subDirectories = Directory.GetDirectories(folder);
            
            if (subDirectories.Length == 1)
            {
                _filesService.CopyFolder(subDirectories[0], folder);
            }
            
            Directory.Delete(subDirectories[0], true);
        }

        public async Task<bool> SetupUnityStep()
        {
            _logger.WriteMessage("-- Setting up Unity Project --- ");
            _unityProject = Path.Join(_moddingPath, "UnityProject");

            await _editor.CreateProject(_unityProject);

            _logger.WriteMessage("Copying Unity Base files to Unity Project ");

            string[] folders = ["Assets", "ProjectSettings", "Packages"];

            foreach (var folder in folders)
            {
                var path = Path.Join(_unityProject, folder);
                var unityBase = Path.Join(_resourceFolder, "UnityBaseProject", folder);
                _logger.WriteMessage($"Deleting {Path.GetFileName(path)}");
                Directory.Delete(path, true);
                _logger.WriteMessage($"Copying {Path.GetFileName(unityBase)}");
                _filesService.CopyFolder(unityBase, path);
            }

            _logger.WriteMessage("Copying Game Files to Unity Project ");
            CopyGameFiles(null);
            var plugins = Path.Join(_unityProject, "Assets", "Plugins");
           
            _logger.WriteMessage("Creating Linker file");
            CreateLinkerFile(Path.Join(plugins, "link.xml"), null);
            return true;
        }

        public bool SetupModsStep()
        {
            _logger.WriteMessage("Copying Mods to UnityProject!");

            var mods = Path.Join(_unityProject, "Assets", "Plugins", "Mods");

            Directory.CreateDirectory(mods);

            var modsDirectories = Directory.GetDirectories(Path.Join(_resourceFolder, "Mods"));

            foreach (var directory in modsDirectories)
            {
                
                var directoryName = Path.GetFileName(directory);
                
                _logger.WriteMessage($"Copying mod {directoryName} to UnityProject");

                _filesService.CopyFolder(directory, Path.Join(mods, directoryName));
            }

            return true;
        }

        public async Task<bool> CompileProjectStep()
        {
            _logger.WriteMessage("Starting to compile, this will take a while...");
            _buildPath = Path.Join(_moddingPath, "build", "build.apk");
            Directory.CreateDirectory(Path.Join(_moddingPath, "build"));
            return await _editor.CompileUnityProject(_unityProject, Path.Join(_moddingPath, "build", "build.apk"),
                "Assets/Settings/Build Profiles/Compile.asset");
        }

        public async Task<bool> PatchApkStep()
        {
            _logger.WriteMessage($"Extracting APK from buildPath: {_buildPath}");
            await _androidService.ExtractApk(_buildPath, _buildPath.Replace(".apk", ""));
            
            _logger.WriteMessage($"Extracting APK from baseGame: {Path.Join(_resourceFolder, "BaseGame.apk")}");
            var apkFolder = Path.Join(_moddingPath, "BaseGame");
            await _androidService.ExtractApk(Path.Join(_resourceFolder, "BaseGame.apk"), apkFolder);

            _logger.WriteMessage($"Patching apk: {_buildPath}");
            _androidService.PatchGame(_buildPath.Replace(".apk", ""), apkFolder);
            _logger.WriteMessage($"Building apk: {"PatchedGame.apk"}");
            await _androidService.BuildApk(Path.Join(_moddingPath, "PatchedGame.apk"), apkFolder);
            _logger.WriteMessage($"Signing apk: {"PatchedGame.apk"}");
            await _androidService.SignApk(Path.Join(_moddingPath, "PatchedGame.apk"));
            return true;
        }

        public async Task<bool> InstallApkStep()
        {
            var packageId = "com.beatgames.beatsaber";
            await _androidService.BackupFiles(packageId);
            await _androidService.UninstallGame(packageId);
            await _androidService.InstallGame(Path.Join(_moddingPath, "PatchedGame.apk"));
            await _androidService.RestoreBackup(packageId);
            await _androidService.GiveAppExternalStoragePermission(packageId);
            return true;
        }

        public async Task<bool> RunProcess(bool initial, ModdingInstance instance)
        {
            if (initial)
            {
                _moddingPath = instance.Path;
                await PrepareStep();
            }
            
            await CompileProjectStep();
            return true;
        }
    }
}