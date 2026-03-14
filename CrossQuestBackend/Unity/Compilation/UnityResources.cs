using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CrossQuestBackend.Unity.Models;

namespace CrossQuestBackend.Unity.Compilation;

public static class UnityResources
{
    public static string BootConfig()
    {
        return $@"gfx-enable-gfx-jobs=1
gfx-enable-native-gfx-jobs=1
gfx-threading-mode=4
wait-for-native-debugger=0
hdr-display-enabled=0
xrsdk-pre-init-library=UnityOpenXR
xr-meta-enabled=1
xr-vulkan-extension-fragment-density-map-enabled=1
xr-latelatching-enabled=0
xr-latelatchingdebug-enabled=0
xr-low-latency-audio-enabled=1
xr-require-backbuffer-textures=0
xr-keyboard-overlay-enabled=1
xr-pipeline-cache-enabled=1
xr-skip-B10G11R11-special-casing=1
xr-hide-memoryless-render-texture=1
xr-skip-audio-buffer-size-check=1
xr-usable-core-mask-enabled=1
androidStartInFullscreen=1
androidRenderOutsideSafeArea=0
build-guid=201592bc64a74fd2aa4a2632c86769d7";
    }
    
    public static ScriptingAssemblies ScriptingAssemblies(List<string> unityAssemblies, List<string> userAssemblies)
    {
        const int UnityAssemblyType = 2;
        const int UserAssemblyType = 16;
        List<string> names = [];
        List<int> types = [];

        foreach (var unityAssembly in unityAssemblies)
        {
            names.Add(Path.GetFileName(unityAssembly));
            types.Add(UnityAssemblyType);
        }

        foreach (var userAssembly in userAssemblies)
        {
            names.Add(Path.GetFileName(userAssembly));
            types.Add(UserAssemblyType);
        }

        return new ScriptingAssemblies(names, types);
    }

    public static async Task RuntimeInitializeOnLoads(string linkerOutputPath, string unityDataPath, string outputPath)
    {
        var dotnetRunPath = Path.Join(unityDataPath, "netcorerun/netcorerun");
        var arguments = new List<string>()
        {
            Path.Join(unityDataPath, "BuildPlayerDataGenerator", "BuildPlayerDataGenerator.exe"),
            "-s=" + linkerOutputPath,
            "-rn=\"RuntimeInitializeOnLoads.json\"",
            "-o=" + outputPath
        };
        
        foreach (var file in Directory.GetFiles(linkerOutputPath).Where(it => it.EndsWith(".dll")))
        {
            arguments.Add("-a=" + file);
        }
        
        await ProcessCaller.ProcessAsync(dotnetRunPath, String.Join(" ", arguments));
    }
}