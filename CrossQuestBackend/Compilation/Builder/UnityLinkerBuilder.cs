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
            new Tuple<string, string>("out", outDirectory),
            new Tuple<string, string>("dotnetprofile", "unityaot-linux" ),
            new Tuple<string, string>("dotnetruntime", "Il2Cpp" ),
            new Tuple<string, string>("platform", "Android" ),
            new Tuple<string, string>("engine-modules-asset-file", ModuleAssetsPath(androidPlayer) ),
            new Tuple<string, string>("allowed-assemblies-only", ""),
            new Tuple<string, string>("use-editor-options", "")
        };

        foreach (var directory in includeDirectory)
        {
            BuildArguments.Add(new Tuple<string, string>("include-directory", directory));
        }

        foreach (var allowedAssembly in allowedAssemblies)
        {
            BuildArguments.Add(new Tuple<string, string>("allowed-assembly", allowedAssembly));
        }

        foreach (var includeLink in includeLinks)
        {
            BuildArguments.Add(new Tuple<string, string>("include-link-xml", includeLink));
        }

        foreach (var unityRootAssembly in unityRootAssemblies)
        {
            BuildArguments.Add(new Tuple<string, string>("include-unity-root-assembly", unityRootAssembly));
        }
    }
}