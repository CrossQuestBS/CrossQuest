using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using IPA.BuildProcess.Interfaces;

namespace CrossQuest.Unity.Compilation;


public static class BuildCallback
{
    public static List<IPostStagingBuild> IPostStagingBuilds = new ();
    public static List<IPreStagingBuild> IPreStagingBuilds = new ();

    public static void RunPreLinkerBuilds(Dictionary<string,List<string>> files)
    {
        foreach (var preLinkerBuild in IPreStagingBuilds)
        {
            preLinkerBuild.Execute(files);
        }
    }
    
    public static void RunPostLinkerBuilds(List<string> allFiles)
    {
        var assemblies = new Dictionary<string, Assembly>();

        foreach (var assembly in AssemblyLoadContext.Default.Assemblies)
        {
            var fileName = Path.GetFileName(assembly.Location);
            var properFilePath = allFiles.FirstOrDefault(it => it.EndsWith(fileName));
            
            if (properFilePath is null)
                continue;
            
            assemblies.TryAdd(properFilePath, assembly);
        }
        
        foreach (var postLinkerBuild in IPostStagingBuilds)
        {
            postLinkerBuild.Execute(allFiles, assemblies);
        }
    }
    
    public static void LoadAssemblies(List<string> modAndLibAssemblies1, List<string> allFiles1)
    {
        foreach (var assemblyPath in modAndLibAssemblies1)
        {
            allFiles1.AddRange(Directory.GetFiles(assemblyPath));
        }


        foreach (var assemblyPath in allFiles1.Where(it => it.EndsWith(".dll")))
        {
            if (assemblyPath.EndsWith("CrossAccord.Generated.dll"))
            {
                File.Delete(assemblyPath);
                continue;
            }
            
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
    
    public static void LoadCallbacks(List<string> callbackAssemblyPaths, List<string> assemblyPaths1)
    {
        var allAssemblies = callbackAssemblyPaths.SelectMany(it => Directory.GetFiles(it).Where(it => it.EndsWith(".dll")));

        foreach (var assemblyFile in allAssemblies)
        {
            if (assemblyFile.EndsWith("CrossAccord.Generated.dll"))
                continue;
            
            var parent = Directory.GetParent(assemblyFile)!.FullName;
            AssemblyHelper.InitializeResolver(parent, assemblyPaths1.ToArray());
            var assembly = AssemblyHelper.ReadAssemblyInMemory(assemblyFile, false);

            var callbacks = assembly.MainModule.Types
                .Where(it => it.HasInterfaces && it.Interfaces.Any(it =>
                    it.InterfaceType.FullName == "IPA.BuildProcess.Interfaces.IBuildCallback"))
                .Where(it => !it.IsInterface).ToArray();

            if (callbacks.Length == 0)
                continue;

            var assemblies2 = AssemblyLoadContext.Default.Assemblies;
            var reflectionAssembly = assemblies2.First(it => it.FullName == assembly.FullName);

            assembly.Dispose();

            foreach (var callbackType in callbacks)
            {
                var type = reflectionAssembly.GetType(callbackType.FullName);

                IBuildCallback buildCallback = (IBuildCallback)Activator.CreateInstance(type);

                switch (buildCallback)
                {
                    case IPostStagingBuild postLinkerBuild:
                        IPostStagingBuilds.Add(postLinkerBuild);
                        break;
                    case IPreStagingBuild preLinkerBuild:
                        IPreStagingBuilds.Add(preLinkerBuild);
                        break;
                }
            }
        }
                
        IPostStagingBuilds.Sort((a, b) => a.executeOrder.CompareTo(b.executeOrder));
        IPreStagingBuilds.Sort((a, b) => a.executeOrder.CompareTo(b.executeOrder));
    }   
}