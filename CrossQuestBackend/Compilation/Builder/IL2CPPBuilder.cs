using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CrossQuestBackend.Compilation.Builder;

public class IL2CPPBuilder : AsyncBuildProcess
{
    private string GeneratedPath(string outputPath) => Path.Join(outputPath, "Build/Generated");
    private string OutputFilePath(string outputPath) => Path.Join(outputPath, "Build/Native/arm64-v8a/libil2cpp.so");
    private string CachedPath(string directory) => Path.Join(directory, "Cache");
    private string BaseLibPath(string androidPlayer) => Path.Join(androidPlayer, "Variations/il2cpp/Release/StaticLibs/arm64-v8a");
    private string ExecutablePath(string il2cppPath) => Path.Join(il2cppPath, "build/deploy/il2cpp");
    
    public IL2CPPBuilder(string assemblies, string directory, string outputPath, string ndkPath, string androidPlayer, string il2cppPath)
    {
        BuildExecutablePath = ExecutablePath(il2cppPath);

        BuildArguments = new()
        {
            "--configuration=Release",
            "--platform=Android",
            "--architecture=ARM64",
            "--dotnetprofile=unityaot-linux",
            "--convert-to-cpp",
            $"--directory={assemblies}",
            $"--generatedcppdir={GeneratedPath(outputPath)}",
            "--compile-cpp",
            $"--outputpath={OutputFilePath(outputPath)}",
            $"--cachedirectory={CachedPath(directory)}",
            $"--tool-chain-path={ndkPath}",
            "--verbose",
            "--emit-null-checks",
            "--enable-array-bounds-check",
            "--emit-null-checks",
            "--print-command-line",
            $"--baselib-directory={BaseLibPath(androidPlayer)}"
        };
    }
}