using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CrossQuestBackend.Game;

namespace CrossQuestBackend.Unity.Compilation;

public static class il2cppCompile
{
    public static async Task<bool> Compile(UnityInstance unityInstance, GameInstance gameInstance, string ndkPath)
    {

        var outDirectory = Path.Join(gameInstance.InstancePath, "Build/Native/arm64-v8a/libil2cpp.so");
        var directory = Path.Join(gameInstance.InstancePath, "Build/ManagedStripped");
        var cache = Path.Join(gameInstance.InstancePath, "Build/Cache");
        var generated = Path.Join(gameInstance.InstancePath, "Build/Generated");
        
        var executable = Path.Join(unityInstance.InstancePath, "UnityData/il2cpp/build/deploy/il2cpp");

        var arguments = new List<string>()
        {
            "--platform=Android",
            "--configuration=Release",
            "--architecture=ARM64",
            "--dotnetprofile=unityaot-linux",
            "--convert-to-cpp",
            $"--directory=\"{directory}\"",
            $"--generatedcppdir=\"{generated}\"",
            "--compile-cpp",
            $"--outputpath=\"{outDirectory}\"",
            $"--cachedirectory=\"{cache}\"",
            $"--tool-chain-path=\"{ndkPath}\"",
            "--verbose",
            "--emit-null-checks",
            "--enable-array-bounds-check",
            "--emit-null-checks",
            "--print-command-line",
            $"--baselib-directory=\"{Path.Join(unityInstance.InstancePath, "AndroidPlayer/StaticLibs")}\"",
        };
        
        return await ProcessCaller.ProcessAsync(executable, String.Join(" ", arguments), false);
    }
}