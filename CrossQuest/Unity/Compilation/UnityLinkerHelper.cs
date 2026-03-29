using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CrossQuestBackend.Game;

namespace CrossQuestBackend.Unity.Compilation;

public static class UnityLinkerHelper
{
    private static List<string> GetDllFiles(string directory) =>
        Directory.GetFiles(directory).Where(path => path.EndsWith(".dll")).ToList();

    public static void CopyFilesToStaging(UnityInstance unityInstanceParam, GameInstance gameInstance)
    {
        var tempFolder = Path.Join(unityInstanceParam.InstancePath, "Temp");

        var stagingArea = Path.Join(tempFolder, "StagingArea");

        if (Directory.Exists(stagingArea))
            Directory.Delete(stagingArea, true);

        Directory.CreateDirectory(stagingArea);

        List<string> assemblyPaths =
        [
            Path.Join(gameInstance.InstancePath, "UnityDependencies/dependencies/PlayerScriptAssemblies"),
            Path.Join(gameInstance.InstancePath, "UnityDependencies/dependencies/Managed"),
            Path.Join(gameInstance.InstancePath, "Libs"),
            Path.Join(gameInstance.InstancePath, "Mods"),
            Path.Join(gameInstance.InstancePath, "Mods", "Build"),
            Path.Join(gameInstance.InstancePath, "Oculus/Beat Saber_Data/Managed"),
            Path.Join(unityInstanceParam.InstancePath, "UnityData/unityaot-linux"),
            Path.Join(unityInstanceParam.InstancePath, "UnityData/unityaot-linux/Facades")
        ];

        foreach (var assemblyPath in assemblyPaths)
        {
            foreach (var filePath in Directory.GetFiles(assemblyPath))
            {
                var fileName = Path.GetFileName(filePath);
                File.Copy(filePath, Path.Join(stagingArea, fileName), true);
            }
        }
    }

    public static void GenerateLinkFile(GameInstance instance)
    {
        List<string> filesToSave = new();

        var libFiles = Directory.GetFiles(Path.Join(instance.InstancePath, "Libs"))
            .Where(it => !it.Contains("Build") && it.EndsWith(".dll")).Select(it => Path.GetFileName(it));
        var modFiles = Directory.GetFiles(Path.Join(instance.InstancePath, "Mods")).Where(it => it.EndsWith(".dll"))
            .Select(it => Path.GetFileName(it));
        var gameFiles = Directory.GetFiles(Path.Join(instance.InstancePath, "Oculus/Beat Saber_Data/Managed"))
            .Where(it => it.EndsWith(".dll")).Select(it => Path.GetFileName(it));

        var playerAssemblies = Directory.GetFiles(Path.Join(instance.InstancePath, "Oculus/Beat Saber_Data/Managed"))
            .Where(it => it.EndsWith(".dll")).Select(it => Path.GetFileName(it));

        var unityFiles = Directory.GetFiles(Path.Join(instance.InstancePath, "UnityDependencies/dependencies/Managed"))
            .Where(it => it.EndsWith(".dll")).Select(it => Path.GetFileName(it));

        filesToSave.AddRange(libFiles);
        filesToSave.AddRange(modFiles);
        filesToSave.AddRange(gameFiles);
        filesToSave.AddRange(unityFiles);
        filesToSave.AddRange(playerAssemblies);
        filesToSave.AddRange(
            new List<string>()
            {
                "UnityEngine.TextCoreFontEngineModule",
                "Unity.Addressables",
                "Unity.ResourceManager",
                "Unity.InputSystem",
                "Unity.TextMeshPro",
                "Unity.Timeline",
                "UnityEngine.CoreModule",
                "UnityEngine.AnimationModule",
                "UnityEngine.AudioModule",
                "UnityEngine.ClothModule",
                "UnityEngine.DirectorModule",
                "UnityEngine.ParticleSystemModule",
                "UnityEngine.PhysicsModule",
                "UnityEngine.SpatialTracking",
                "UnityEngine.TextRenderingModule",
                "UnityEngine.UI",
                "UnityEngine.UIModule",
                "UnityEngine.VideoModule",
                "Unity.Burst.Unsafe",
                "Unity.Burst",
                "UnityEngine.TLSModule",
                "UnityEngine.UmbraModule",
                "UnityEngine.MarshallingModule",
                "UnityEngine.MultiplayerModule",
                "UnityEngine.CoreModule",
            });
        // TODO: Add libs + mods needed

        var xmlElements = string.Join("\n",
            filesToSave.Select(it => $"<assembly fullname=\"{it.Replace(".dll", "")}\" preserve=\"all\"/>"));

        var xmlFile = @$"<linker>
        {xmlElements}
    </linker>
    ";

        Directory.CreateDirectory(Path.Join(instance.InstancePath, "Build"));
        File.WriteAllText(Path.Join(instance.InstancePath, "Build", "GameLink.xml"), xmlFile);

        File.WriteAllText(Path.Join(instance.InstancePath, "Resources", "link.xml"), UnityResources.LinkFile());
    }


    // TODO: Remove this code!
    public static Tuple<List<string>, List<string>> GetLinkerAssemblyPaths(string androidPlayer, string unityData,
        string gameDependencies)
    {
        var allowedAssemblies = new List<string>();

        var managedAssembliesPath = Path.Join(androidPlayer, "Managed");
        var playerScriptAssemblies = Path.Join(gameDependencies, "dependencies/PlayerScriptAssemblies");
        var unityAotLinux = Path.Join(unityData, "unityaot-linux");
        var unityAotLinuxFacade = Path.Join(unityData, "unityaot-linux/Facades");

        allowedAssemblies.AddRange(GetDllFiles(managedAssembliesPath));
        allowedAssemblies.AddRange(GetDllFiles(playerScriptAssemblies));
        allowedAssemblies.AddRange(GetDllFiles(unityAotLinux));
        allowedAssemblies.AddRange(GetDllFiles(unityAotLinuxFacade));

        List<string> includeDirectory = new()
        {
            managedAssembliesPath,
            playerScriptAssemblies,
            //additionalAssembliesPrebuilt,
            unityAotLinux,
            unityAotLinuxFacade
        };

        return new Tuple<List<string>, List<string>>(allowedAssemblies, includeDirectory);
    }
}