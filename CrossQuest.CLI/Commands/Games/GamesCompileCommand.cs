using System.ComponentModel;
using CrossQuest.Android;
using CrossQuest.Unity.Compilation;
using Spectre.Console.Cli;

namespace CrossQuest.CLI.Commands.Games;

public class GamesCompileCommand : AsyncCommand<GamesCompileCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-b|--build-apk")]
        [Description("Build apk after compilation")]
        [DefaultValue(false)]
        public required bool BuildAPK { get; init; }

        [CommandOption("-d|--deploy-to-device")]
        [Description("Deploy to device if connected using adb")]
        [DefaultValue(false)]
        public required bool DeployToDevice { get; init; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings,
        CancellationToken cancellationToken)
    {
        var crossInstance = CrossInstance.GetActiveInstance();

        if (crossInstance is null)
        {
            Console.WriteLine("Found no active instance, please run `games install ...`");
            return 1;
        }

        var instance = crossInstance.GameInstance;
        var unityInstance = crossInstance.UnityInstance;
        var androidTools = crossInstance.AndroidTools;

        if (!await instance.RunPreIL2CPP(unityInstance))
        {
            Console.WriteLine("something went wrong during pre il2cpp step");
            return 1;
        }

        if (!await instance.RunIL2CPP(unityInstance, androidTools.NDK))
        {
            Console.WriteLine("SOMETHING WENT WRONG during compilation!");
            return 1;
        }

        var moddedApkPath = Path.Join(instance.InstancePath, "Build", "Modded.apk");
        try
        {
            if (settings.BuildAPK)
            {
                var bootConfig = UnityResources.BootConfig();
                var manifest = UnityResources.Manifest();

                var tempPath = Path.GetTempPath() + Guid.NewGuid();

                Directory.CreateDirectory(tempPath);

                try
                {
                    var gameApk = Directory.GetFiles(Path.Join(instance.InstancePath, "Oculus"))
                        .First(it => it.Contains("beat-saber") && it.EndsWith("apk"));

                    var extractApkPath = Path.Join(tempPath, "beat-saber");
                    Console.WriteLine($"Extracting APK to {extractApkPath}");

                    if (!await ApkService.ExtractApk(androidTools, gameApk, extractApkPath))
                    {
                        Console.WriteLine("Failed to extract APK!");
                        return 1;
                    }

                    ApkService.CopyJniLibs(instance, extractApkPath);

                    await ApkService.CopyMetadata(cancellationToken, instance, extractApkPath, manifest, bootConfig);

                    await ApkService.CreateAPK(androidTools, moddedApkPath,
                        extractApkPath);
                    await ApkService.SignAPK(androidTools, moddedApkPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                finally
                {
                    Directory.Delete(tempPath, true);
                }
            }
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }
        
        if (!settings.DeployToDevice)
            return 0;

        if (!File.Exists(moddedApkPath))
        {
            Console.WriteLine("Could not find built apk!");
            return 1;
        }

        if (!await AdbService.IsDeviceConnected(androidTools))
        {
            Console.WriteLine("Quest headset not connected, not deploying game");
            return 1;
        }

        var crossQuestFolder = "/sdcard/CrossQuest/com.beatgames.beatsaber";
        
        Console.WriteLine("Logging to apk!");
        await AdbService.InstallAPK(androidTools, moddedApkPath);

        Console.WriteLine("Setting permissions to game!");
        await AdbService.SetManageExternalStoragePermission(androidTools);
        
        Console.WriteLine($"Checking if it has path {crossQuestFolder}!");
        if (!await AdbService.HasPathOnDevice(androidTools, crossQuestFolder))
        {
            Console.WriteLine($"It does not have the path");
            await AdbService.CreateFolder(androidTools, "/sdcard/CrossQuest/com.beatgames.beatsaber");
            await AdbService.CreateFolder(androidTools, Path.Join(crossQuestFolder, "UserData"));
            await AdbService.SetPermission(androidTools, "/sdcard/CrossQuest");
        }

       
        
        await instance.SetupObb(androidTools);
        
        Console.WriteLine($"Starting game!");
        
        await AdbService.StartGame(androidTools);
        return 0;
    }
}