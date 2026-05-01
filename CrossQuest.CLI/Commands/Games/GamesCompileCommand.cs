using System.ComponentModel;
using System.Diagnostics;
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
        
        [CommandOption("-s|--skip-deploy-setup")]
        [Description("Skips deploy setup checks")]
        [DefaultValue(false)]
        public required bool SkipSetup { get; init; } = false;
        
        [CommandOption("--debug")]
        [Description("Builds with C++ debug option, is faster build but slower runtime")]
        [DefaultValue(false)]
        public required bool EnableDebug { get; init; } = false;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings,
        CancellationToken cancellationToken)
    {
        long longTotalStopWatch = 0;
        var crossInstance = CrossInstance.GetActiveInstance();

        if (crossInstance is null)
        {
            Console.WriteLine("Found no active instance, please run `games install ...`");
            return 1;
        }

        var instance = crossInstance.GameInstance;
        var unityInstance = crossInstance.UnityInstance;
        var androidTools = crossInstance.AndroidTools;
        Stopwatch stopWatch = new Stopwatch();

        stopWatch.Start();
        Console.WriteLine($"Running Pre il2cpp");

        if (!await instance.RunPreIL2CPP(unityInstance))
        {
            Console.WriteLine("something went wrong during pre il2cpp step");
            return 1;
        }
        
        stopWatch.Stop();

        longTotalStopWatch += stopWatch.ElapsedMilliseconds;
        Console.WriteLine($"Pre il2cpp took {stopWatch.ElapsedMilliseconds / 1000}s / {longTotalStopWatch / 1000}s");
        Thread.Sleep(400);

        stopWatch.Reset();
        stopWatch.Start();
        Console.WriteLine($"Running IL2CPP step");
        
        if (!await instance.RunIL2CPP(unityInstance, androidTools.NDK, settings.EnableDebug))
        {
            Console.WriteLine("IL2CPP compilation failed!");
            return 1;
        }

        stopWatch.Stop();
        longTotalStopWatch += stopWatch.ElapsedMilliseconds;
        Console.WriteLine($"IL2CPP took {stopWatch.ElapsedMilliseconds / 1000}s / {longTotalStopWatch / 1000}s");
        Thread.Sleep(400);

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
                    
                    stopWatch.Reset();
                    stopWatch.Start();
                    var extractApkPath = Path.Join(tempPath, "beat-saber");
                    Console.WriteLine($"Extracting APK to {extractApkPath}");

                    if (!await ApkService.ExtractApk(androidTools, gameApk, extractApkPath))
                    {
                        Console.WriteLine("Failed to extract APK!");
                        return 1;
                    }
                    stopWatch.Stop();
                    longTotalStopWatch += stopWatch.ElapsedMilliseconds;
                    Console.WriteLine($"Extracting APK took {stopWatch.ElapsedMilliseconds / 1000}s / {longTotalStopWatch / 1000}s");
                    Thread.Sleep(400);

                    Console.WriteLine($"Copying files");
                    stopWatch.Reset();
                    stopWatch.Start();
                    ApkService.CopyJniLibs(instance, extractApkPath);

                    await ApkService.CopyMetadata(cancellationToken, instance, extractApkPath, manifest, bootConfig);

                    stopWatch.Stop();
                    longTotalStopWatch += stopWatch.ElapsedMilliseconds;
                    Console.WriteLine($"Copying files took {stopWatch.ElapsedMilliseconds / 1000}s / {longTotalStopWatch / 1000}s");
                    Thread.Sleep(400);

                    stopWatch.Reset();
                    stopWatch.Start();
                    Console.WriteLine($"Creating APK");
                    await ApkService.CreateAPK(androidTools, moddedApkPath,
                        extractApkPath);
                    
                    stopWatch.Stop();
                    longTotalStopWatch += stopWatch.ElapsedMilliseconds;
                    Console.WriteLine($"Creating APK took {stopWatch.ElapsedMilliseconds / 1000}s / {longTotalStopWatch / 1000}s");
                    Thread.Sleep(400);

                    stopWatch.Reset();
                    stopWatch.Start();
                    Console.WriteLine($"Signing APK");

                    await ApkService.SignAPK(androidTools, moddedApkPath);
                    
                    stopWatch.Stop();
                    longTotalStopWatch += stopWatch.ElapsedMilliseconds;
                    Console.WriteLine($"Signing APK took {stopWatch.ElapsedMilliseconds / 1000}s / {longTotalStopWatch / 1000}s");
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
        
        Console.WriteLine("Installing apk!");
        await AdbService.InstallAPK(androidTools, moddedApkPath);

        
        Console.WriteLine($"Checking if it has path {crossQuestFolder}!");

        if (!settings.SkipSetup)
        {
            if (!await AdbService.HasPathOnDevice(androidTools, crossQuestFolder))
            {
                Console.WriteLine($"Does not have the path");
                await AdbService.CreateFolder(androidTools, "/sdcard/CrossQuest/com.beatgames.beatsaber");
                await AdbService.CreateFolder(androidTools,"/sdcard/CrossQuest/com.beatgames.beatsaber/UserData");
                await AdbService.SetPermission(androidTools, "/sdcard/CrossQuest");
            }
        
            await instance.SetupObb(androidTools);
        
            Console.WriteLine("Setting permissions to game!");
            await AdbService.SetManageExternalStoragePermission(androidTools);
        }
  
        
        Console.WriteLine($"Starting game!");
        
        await AdbService.StartGame(androidTools);
        return 0;
    }
}