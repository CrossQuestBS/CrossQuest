using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CrossQuestBackend.Game;

namespace CrossQuestBackend.Unity.Compilation;

public static class UnityLinker
{
    public static async Task StartCompile(UnityInstance unityInstance, GameInstance gameInstance)
    {
        var executable = Path.Join(unityInstance.InstancePath, "UnityData/il2cpp/build/deploy/UnityLinker");
        var outputPath = Path.Join(gameInstance.InstancePath, "Build/ManagedStripped");
    
        var playerScriptAssembliesFileNames = Directory.GetFiles(Path.Join(gameInstance.InstancePath, "UnityDependencies/dependencies/PlayerScriptAssemblies")).Where(it => it.EndsWith(".dll")).Select(it => Path.GetFileName(it));

        var stagingArea = Path.Join(unityInstance.InstancePath, "Temp", "StagingArea");
        
        List<string> includeLinks =
        [
            Path.Join(gameInstance.InstancePath, "Build", "GameLink.xml"),
            Path.Join(gameInstance.InstancePath, "Resources", "link.xml"),
            Path.Join(gameInstance.InstancePath, "Resources", "link_old.xml"),
            Path.Join(unityInstance.InstancePath, "AndroidPlayer/AndroidNativeLink.xml"),
        ];

        var arguments = new List<string>()
        {
            $"--out=\"{outputPath}\"",
            "--dotnetprofile=unityaot-linux",
            "--dotnetruntime=Il2Cpp",
            "--platform=Android",
            $"--engine-modules-asset-file=\"{Path.Join(unityInstance.InstancePath, "/AndroidPlayer/modules.asset")}\"",
            "--allowed-assemblies-only",
            "--use-editor-options",
            $"--include-directory=\"{stagingArea}\""
        };

        foreach (var assembly in Directory.GetFiles(stagingArea))
        {
            arguments.Add($"--allowed-assembly=\"{assembly}\"");
        }
     
        foreach (var includeLink in includeLinks)
        {
            arguments.Add("--include-link-xml=\"" + includeLink+"\"");
        }

        foreach (var unityRootAssembly in Directory.GetFiles(stagingArea).Where(it => playerScriptAssembliesFileNames.Contains(Path.GetFileName(it))))
        {
            arguments.Add("--include-unity-root-assembly=\""+unityRootAssembly+"\"");
        }
        
        Console.WriteLine(String.Join("\n", arguments));
        await ProcessCaller.ProcessAsync(executable, String.Join(" ", arguments));
    }
}