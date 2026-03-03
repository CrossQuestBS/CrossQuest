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
            new Tuple<string,string>("configuration", "Release" ),
            new Tuple<string,string>("platform", "Android" ),
            new Tuple<string,string>("architecture", "ARM64" ),
            new Tuple<string,string>("dotnetprofile", "unityaot-linux" ),
            new Tuple<string,string>("convert-to-cpp", "" ),
            new Tuple<string,string>("directory", assemblies ),
            new Tuple<string,string>("generatedcppdir", GeneratedPath(outputPath) ),
            new Tuple<string,string>("compile-cpp", "" ),
            new Tuple<string,string>("outputpath", OutputFilePath(outputPath) ),
            new Tuple<string,string>("cachedirectory", CachedPath(directory) ),
            new Tuple<string,string>("tool-chain-path", ndkPath ),
            new Tuple<string,string>("verbose", "" ),
            new Tuple<string,string>("emit-null-checks", "" ),
            new Tuple<string,string>("enable-array-bounds-check", "" ),
            new Tuple<string,string>("emit-null-checks", "" ),
            new Tuple<string,string>("print-command-line", "" ),
            new Tuple<string,string>("baselib-directory", BaseLibPath(androidPlayer) ),
        };
    }
}