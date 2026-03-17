using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CrossQuestBackend.Game;

namespace CrossQuestBackend.Unity.Compilation;

public static class UnityLinker
{
    private static void RemoveBuildFiles(UnityInstance unityInstance)
    {
        foreach (var filePath in Directory.GetFiles(Path.Join(unityInstance.InstancePath, "Temp", "StagingArea")))
        {
            if (filePath.Contains("IPA") && filePath.Contains("Build"))
                File.Delete(filePath);

            if (filePath.Contains("CrossAccord") && filePath.Contains("Build"))
                File.Delete(filePath);
        }

    }
    
    public static async Task<bool> StartCompile(UnityInstance unityInstance, GameInstance gameInstance)
    {
        RemoveBuildFiles(unityInstance);
        
        var executable = Path.Join(unityInstance.InstancePath, "UnityData", "il2cpp", "build", "deploy", "UnityLinker");
        var outputPath = Path.Join(gameInstance.InstancePath, "Build", "ManagedStripped");
    
        var playerScriptAssembliesFileNames = Directory.GetFiles(Path.Join(gameInstance.InstancePath, "UnityDependencies", "dependencies", "PlayerScriptAssemblies")).Where(it => it.EndsWith(".dll")).Select(it => Path.GetFileName(it));

        var stagingArea = Path.Join(unityInstance.InstancePath, "Temp", "StagingArea");
        
        List<string> includeLinks =
        [
            Path.Join(gameInstance.InstancePath, "Build", "GameLink.xml"),
            Path.Join(gameInstance.InstancePath, "Resources", "link.xml"),
            Path.Join(gameInstance.InstancePath, "Resources", "link_old.xml"),
            Path.Join(unityInstance.InstancePath, "AndroidPlayer", "AndroidNativeLink.xml"),
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
        
     
        arguments.Add($"--allowed-assembly={toSpecialList(Directory.GetFiles(stagingArea).Select(it => Path.GetFileName(it)).Where(it => it.EndsWith(".dll")).ToList())}");
       
        var includeLinkXmlArguments = toSpecialList(includeLinks);
        arguments.Add("--include-link-xml=" + includeLinkXmlArguments);

        arguments.Add("--include-unity-root-assembly=" + toSpecialList(playerScriptAssembliesFileNames.ToList()));

        Console.WriteLine(String.Join(" ", arguments));
        
        
        return await ProcessCaller.ProcessAsync(executable, String.Join(" ", arguments), false, stagingArea);
    }

    private static string toSpecialList(List<string> includeLinks)
    {
        return String.Join(",",includeLinks.Select(it => $"\"{it}\""));
    }
}