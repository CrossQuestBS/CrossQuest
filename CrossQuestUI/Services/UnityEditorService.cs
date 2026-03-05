using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace CrossQuestUI.Services
{
    public static class UnityEditorService
    {
        private static readonly HttpClient Client = new HttpClient();
        
        
        
        public static async Task<bool> InstallBaseProject(string basePath, string unityProjectPath, string downloadPath)
        {
            try
            {
                var contents = await Client.GetByteArrayAsync(downloadPath);
                var zipFile = basePath + ".zip";
                
                await File.WriteAllBytesAsync(zipFile, contents);
                
                
                await ZipFile.ExtractToDirectoryAsync(zipFile, basePath);
                
                DirectoryExtensions.MoveZippedToParent(basePath);

                foreach (var subFolder in (List<string>)["Assets", "ProjectSettings", "Packages"])
                {
                    var destFolder = Path.Join(unityProjectPath, subFolder);
                    var srcFolder = Path.Join(basePath, subFolder);
                    Directory.Delete(destFolder, true); 
                    DirectoryExtensions.CopyFolder(srcFolder, destFolder);
                }
                
                Directory.Delete(basePath, true);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Message);
                return false;
            } 
        }

        public static async Task<bool> CreateProject(string unityPath, string projectPath)
        {
            var arguments = "-batchmode ";
            arguments += $"-createProject \"{projectPath}\" ";
            arguments += "-quit -logfile - ";
            
            return await ProcessCallerService.ProcessAsync(unityPath, arguments);
        }
        
        public static async Task<bool> CompileProject(string unityPath, string projectPath, string buildPath)
        {
            var arguments = "-batchmode ";
            arguments += $"-project-path \"{projectPath}\" ";
            arguments += "-quit -logfile - ";
            arguments += $"-activeBuildProfile \"Assets/Settings/Build Profiles/Compile.asset\" ";
            arguments += $"-build \"{buildPath}\"";

            var compiled = await ProcessCallerService.ProcessAsync(unityPath, arguments);
            return File.Exists(buildPath) && compiled;
        }
    }
}