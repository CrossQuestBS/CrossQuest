// See https://aka.ms/new-console-template for more information

using System.Runtime.Loader;
using CrossQuestBackend;
using CrossQuestBackend.Game;
using CrossQuestBackend.Unity;
using CrossQuestBackend.Unity.Compilation;
using CrossQuestBackend.Unity.Models;
using IPA.BuildProcess.Interfaces;

Console.WriteLine("Hello, World!");


// TODO: Properly fix this

var games = await ResourceDownloader.Games();


var beatSaber = games.First(it => it.Id == "com.beatgames.beatsaber");
var version = beatSaber.ModdableVersionList[0];

// Create new instances
var unityInstance = new UnityInstance(version.UnityVersion);
var instance = new GameInstance(beatSaber.Id, version, version.UnityVersion.Version);


await unityInstance.DownloadFiles();
await instance.DownloadFiles();
await instance.DownloadGameFiles("..._token_here");

List<string> assemblyPaths =
[
    Path.Join(instance.InstancePath, "Libs"),
    Path.Join(instance.InstancePath, "Mods"),
    Path.Join(instance.InstancePath, "Oculus/Beat Saber_Data/Managed"),
    Path.Join(instance.InstancePath, "UnityDependencies/dependencies/PlayerScriptAssemblies"),
    Path.Join(instance.InstancePath, "UnityDependencies/dependencies/Managed"),
    Path.Join(unityInstance.InstancePath, "UnityData/unityaot-linux"),
    Path.Join(unityInstance.InstancePath, "UnityData/unityaot-linux/Facades")
];

List<string> modAndLibAssemblies =
[
    Path.Join(instance.InstancePath, "Libs"),
    Path.Join(instance.InstancePath, "Libs/Build"),
    Path.Join(instance.InstancePath, "Mods"),
    Path.Join(instance.InstancePath, "Oculus/Beat Saber_Data/Managed"),
    Path.Join(instance.InstancePath, "UnityDependencies/dependencies/Managed"),
];

List<IPostLinkerBuild> postLinkerBuilds = new List<IPostLinkerBuild>();
List<IPreLinkerBuild> preLinkerBuilds = new List<IPreLinkerBuild>();


var allFiles = new List<string>();

LoadAssemblies(modAndLibAssemblies, allFiles);
LoadCallbacks(modAndLibAssemblies, assemblyPaths, postLinkerBuilds, preLinkerBuilds);

postLinkerBuilds.Sort((a, b) => a.executeOrder.CompareTo(b.executeOrder));
preLinkerBuilds.Sort((a, b) => a.executeOrder.CompareTo(b.executeOrder));


foreach (var preLinkerBuild in preLinkerBuilds)
{
    preLinkerBuild.Execute(allFiles);
}

CopyFilesToStaging(unityInstance, instance);

foreach (var postLinkerBuild in postLinkerBuilds)
{
    postLinkerBuild.Execute(Directory.GetFiles(Path.Join(unityInstance.InstancePath, "Temp", "StagingArea")).ToList());
}

GenerateLinkFile(instance);

foreach (var filePath in Directory.GetFiles(Path.Join(unityInstance.InstancePath, "Temp", "StagingArea")))
{
    if (filePath.Contains("IPA") && filePath.Contains("Build"))
        File.Delete(filePath);

    if (filePath.Contains("CrossAccord") && filePath.Contains("Build"))
        File.Delete(filePath);
}

List<string> unityAssemblies = new List<string>();

List<string> userAssemblies = new List<string>();

await UnityLinker.StartCompile(unityInstance, instance);

var libFiles = Directory.GetFiles(Path.Join(instance.InstancePath, "Libs")).Where(it => it.EndsWith(".dll"));
var modFiles = Directory.GetFiles(Path.Join(instance.InstancePath, "Mods")).Where(it => it.EndsWith(".dll"));
var beatsaberFiles = Directory.GetFiles(Path.Join(instance.InstancePath, "Oculus/Beat Saber_Data/Managed")).Where(it => it.EndsWith(".dll"));

userAssemblies.AddRange(libFiles);
userAssemblies.AddRange(modFiles);
userAssemblies.AddRange(beatsaberFiles);

var stagingAreaFileNames = Directory
    .GetFiles(Path.Join(unityInstance.InstancePath, "Temp", "StagingArea"))
    .Where(it => it.EndsWith(".dll")).Select(it => Path.GetFileName(it));
var unityAssembliesFileNames = Directory.GetFiles(Path.Join(instance.InstancePath, "UnityDependencies/dependencies/Managed"))
    .Where(it => it.EndsWith(".dll")).Select(it => Path.GetFileName(it)).Where(it => stagingAreaFileNames.Contains(it));

unityAssemblies.AddRange(unityAssembliesFileNames);

var scriptingAssemblies = UnityResources.ScriptingAssemblies(unityAssemblies, userAssemblies);

var assemblies = scriptingAssemblies.AsJson();


// TODO: Create ScriptingAssemblies.json
// TODO: Create RuntimeInitializeOnLoads.json

await il2cppCompile.Compile(unityInstance, instance, "/Users/maribell/QPM-RS/ndk/29.0.14206865+preview-0");

Console.WriteLine(assemblies);

// TODO: Need a way to get boot.config
// TODO: use apktool to unextract downloaded apk
// TODO: Patch apk with compiled files
// TODO: use apktool to extract apk
// TODO: use apk sign to new apk
// TODO: Uninstall game if needed
// TODO: Clear game cache
// TODO: Install game
// TODO: Fix permissions

void GenerateLinkFile(GameInstance instance)
{
    List<string> filesToSave = new();

    var libFiles = Directory.GetFiles(Path.Join(instance.InstancePath, "Libs"))
        .Where(it => !it.Contains("Build") && it.EndsWith(".dll")).Select(it => Path.GetFileName(it));
    var modFiles = Directory.GetFiles(Path.Join(instance.InstancePath, "Mods")).Where(it => it.EndsWith(".dll"))
        .Select(it => Path.GetFileName(it));
    var gameFiles = Directory.GetFiles(Path.Join(instance.InstancePath, "Oculus/Beat Saber_Data/Managed"))
        .Where(it => it.EndsWith(".dll")).Select(it => Path.GetFileName(it));
    
    var playerAssemblies = Directory.GetFiles(Path.Join(instance.InstancePath, "Oculus/Beat Saber_Data/Managed"))
        .Where(it => it.EndsWith(".dll")).Select(it => Path.GetFileName(it));

    var unityFiles = Directory.GetFiles(Path.Join(instance.InstancePath, "UnityDependencies/dependencies/Managed"))
        .Where(it => it.EndsWith(".dll")).Select(it => Path.GetFileName(it));

    filesToSave.AddRange(libFiles);
    filesToSave.AddRange(modFiles);
    filesToSave.AddRange(gameFiles);
    filesToSave.AddRange(unityFiles);
    filesToSave.AddRange(playerAssemblies);
    filesToSave.AddRange(
        new List<string>()
        {
            "UnityEngine.TextCoreFontEngineModule",
            "Unity.Addressables",
            "Unity.ResourceManager",
            "Unity.InputSystem",
            "Unity.TextMeshPro",
            "Unity.Timeline",
            "UnityEngine.CoreModule",
            "UnityEngine.AnimationModule",
            "UnityEngine.AudioModule",
            "UnityEngine.ClothModule",
            "UnityEngine.DirectorModule",
            "UnityEngine.ParticleSystemModule",
            "UnityEngine.PhysicsModule",
            "UnityEngine.SpatialTracking",
            "UnityEngine.TextRenderingModule",
            "UnityEngine.UI",
            "UnityEngine.UIModule",
            "UnityEngine.VideoModule",
            "Unity.Burst.Unsafe",
            "Unity.Burst",
            "UnityEngine.TLSModule",
            "UnityEngine.UmbraModule",
            "UnityEngine.MarshallingModule",
            "UnityEngine.MultiplayerModule",
            "UnityEngine.CoreModule",
        });
    // TODO: Add libs + mods needed

    var xmlElements = string.Join("\n",
        filesToSave.Select(it => $"<assembly fullname=\"{it.Replace(".dll", "")}\" preserve=\"all\"/>"));

    var xmlFile = @$"<linker>
    {xmlElements}
</linker>
";

    File.WriteAllText(Path.Join(instance.InstancePath, "Build", "GameLink.xml"), xmlFile);
}

void CopyFilesToStaging(UnityInstance unityInstanceParam, GameInstance gameInstance)
{
    var tempFolder = Path.Join(unityInstanceParam.InstancePath, "Temp");

    var stagingArea = Path.Join(tempFolder, "StagingArea");

    if (Directory.Exists(stagingArea))
        Directory.Delete(stagingArea, true);

    Directory.CreateDirectory(stagingArea);

    List<string> assemblyPaths =
    [        
        Path.Join(gameInstance.InstancePath, "UnityDependencies/dependencies/PlayerScriptAssemblies"),
        Path.Join(gameInstance.InstancePath, "UnityDependencies/dependencies/Managed"),
        Path.Join(gameInstance.InstancePath, "Libs"),
        Path.Join(gameInstance.InstancePath, "Mods"),
        Path.Join(gameInstance.InstancePath, "Oculus/Beat Saber_Data/Managed"),
        Path.Join(unityInstanceParam.InstancePath, "UnityData/unityaot-linux"),
        Path.Join(unityInstanceParam.InstancePath, "UnityData/unityaot-linux/Facades")
    ];

    foreach (var assemblyPath in assemblyPaths)
    {
        foreach (var filePath in Directory.GetFiles(assemblyPath))
        {
            var fileName = Path.GetFileName(filePath);
            File.Copy(filePath, Path.Join(stagingArea, fileName), true);
        }
    }
}

void LoadCallbacks(List<string> list, List<string> assemblyPaths1, List<IPostLinkerBuild> postLinkerBuilds1,
    List<IPreLinkerBuild> preLinkerBuilds1)
{
    foreach (var assemblyPath in list)
    {
        foreach (var assemblyFile in Directory.GetFiles(assemblyPath).Where(it => it.EndsWith(".dll")))
        {
            var parent = Directory.GetParent(assemblyFile).FullName;

            AssemblyHelper.InitializeResolver(parent, assemblyPaths1.ToArray());

            using var assembly = AssemblyHelper.ReadAssemblyInMemory(assemblyFile);

            var callbacks = assembly.MainModule.Types
                .Where(it => it.HasInterfaces && it.Interfaces.Any(it =>
                    it.InterfaceType.FullName == "IPA.BuildProcess.Interfaces.IBuildCallback"))
                .Where(it => !it.IsInterface).ToArray();

            if (callbacks.Length == 0)
                continue;

            var assemblies2 = AssemblyLoadContext.Default.Assemblies;
            var reflectionAssembly = assemblies2.First(it => it.FullName == assembly.FullName);


            foreach (var callbackType in callbacks)
            {
                var type = reflectionAssembly.GetType(callbackType.FullName);

                IBuildCallback buildCallback = (IBuildCallback)Activator.CreateInstance(type);

                switch (buildCallback)
                {
                    case IPostLinkerBuild postLinkerBuild:
                        postLinkerBuilds1.Add(postLinkerBuild);
                        break;
                    case IPreLinkerBuild preLinkerBuild:
                        preLinkerBuilds1.Add(preLinkerBuild);
                        break;
                }
            }
        }
    }
}

void LoadAssemblies(List<string> modAndLibAssemblies1, List<string> allFiles1)
{
    foreach (var assemblyPath in modAndLibAssemblies1)
    {
        allFiles1.AddRange(Directory.GetFiles(assemblyPath));
    }


    foreach (var assemblyPath in allFiles1.Where(it => it.EndsWith(".dll")))
    {
        try
        {
            AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}