using System.Collections.Generic;
using System.IO;

namespace CrossQuestBackend.Compilation.Builder;

public class UnityLinkerBuilder : AsyncBuildProcess
{
    public UnityLinkerBuilder(
        string il2cppPath,
        List<string> allowedAssemblies,
        List<string> includeLinks,
        List<string> unityRootAssemblies,
        string outDirectory,
        string includeDirectory,
        string androidPlayer,
        string editorDataFile
    )
    {
        BuildExecutablePath = Path.Join(il2cppPath, "build/deploy/UnityLinker");

        BuildArguments = new()
        {
            { "out", outDirectory },
            { "include-directory", includeDirectory },
            { "dotnetprofile", "unityaot-linux" },
            { "dotnetruntime", "Il2Cpp" },
            { "platform", "Android" },
            { "engine-modules-asset-file", Path.Join(androidPlayer, "modules.asset") },
            { "use-editor-options", "" },
            { "editor-data-file", editorDataFile }
        };

        foreach (var allowedAssembly in allowedAssemblies)
        {
            BuildArguments.Add("allowed-assembly", allowedAssembly);
        }

        foreach (var includeLink in includeLinks)
        {
            BuildArguments.Add("include-link-xml", includeLink);
        }

        foreach (var unityRootAssembly in unityRootAssemblies)
        {
            BuildArguments.Add("include-unity-root-assembly", unityRootAssembly);
        }
    }
}