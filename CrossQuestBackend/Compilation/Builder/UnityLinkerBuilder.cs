using System;
using System.Collections.Generic;
using System.IO;

namespace CrossQuestBackend.Compilation.Builder;

public class UnityLinkerBuilder : AsyncBuildProcess
{
    private string ExecutablePath(string il2cppPath) => Path.Join(il2cppPath, "build/deploy/UnityLinker");
    private string ModuleAssetsPath(string androidPlayer) => Path.Join(androidPlayer, "modules.asset");
    public UnityLinkerBuilder(
        string il2cppPath,
        List<string> allowedAssemblies,
        List<string> includeLinks,
        List<string> unityRootAssemblies,
        List<string> includeDirectory,
        string outDirectory,
        string androidPlayer)
    {
        BuildExecutablePath = ExecutablePath(il2cppPath);

        BuildArguments = new()
        {
            $"--out={outDirectory}",
            "--dotnetprofile=unityaot-linux",
            "--dotnetruntime=Il2Cpp",
            "--platform=Android",
            $"--engine-modules-asset-file={ModuleAssetsPath(androidPlayer)}",  
            "--allowed-assemblies-only",
            "--use-editor-options"
        };

        foreach (var directory in includeDirectory)
        {
            BuildArguments.Add($"--include-directory={directory}");
        }

        foreach (var allowedAssembly in allowedAssemblies)
        {
            BuildArguments.Add($"--allowed-assembly={allowedAssembly}");
        }

        foreach (var includeLink in includeLinks)
        {
            BuildArguments.Add($"--include-link-xml={includeLink}");
        }

        foreach (var unityRootAssembly in unityRootAssemblies)
        {
            BuildArguments.Add($"--include-unity-root-assembly={unityRootAssembly}");
        }
    }
}