using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CrossQuestBackend.Compilation.Builder;

public class IL2CPPBuilder : AsyncBuildProcess
{
    public IL2CPPBuilder(string directory, string ndkPath, string androidPlayer, string il2cppPath)
    {
        var cppDirectory = Path.Join(directory, "Build/Generated");
        var outputPath = Path.Join(directory, "Build/Native/arm64-v8a/libil2cpp.so");
        var cachedDirectory = Path.Join(directory, "Build/Cache");
        var baseLibPath = Path.Join(androidPlayer, "Variations/il2cpp/Release/StaticLibs/arm64-v8a");

        // /Applications/Unity/Hub/Editor/6000.0.40f1/Unity.app/Contents/il2cpp
        BuildExecutablePath = Path.Join(il2cppPath, "build/deploy/il2cpp");

        // TODO: Figure out what is best arguments to use here
        BuildArguments = new()
        {
            { "configuration", "Release" },
            { "platform", "Android" },
            { "architecture", "ARM64" },
            { "dotnetprofile", "unityaot-linux" },
            { "convert-to-cpp", "" },
            { "directory", directory },
            { "generatedcppdir", cppDirectory },
            { "compile-cpp", "" },
            { "outputpath", outputPath },
            { "cachedirectory", cachedDirectory },
            { "tool-chain-path", ndkPath },
            { "verbose", "" },
            { "emit-null-checks", "" },
            { "enable-array-bounds-check", "" },
            { "emit-null-checks", "" },
            { "print-command-line", "" },
            { "baselib-directory", baseLibPath },
        };
    }
}