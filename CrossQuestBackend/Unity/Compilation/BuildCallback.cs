using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using IPA.BuildProcess.Interfaces;

namespace CrossQuestBackend.Unity.Compilation;


public static class BuildCallback
{
    public static List<IPostLinkerBuild> PostLinkerBuilds = new ();
    public static List<IPreLinkerBuild> PreLinkerBuilds = new ();

    public static void RunPreLinkerBuilds(List<string> allFiles)
    {
        foreach (var preLinkerBuild in PreLinkerBuilds)
        {
            preLinkerBuild.Execute(allFiles);
        }
    }
    
    public static void RunPostLinkerBuilds(List<string> allFiles)
    {
        foreach (var postLinkerBuild in PostLinkerBuilds)
        {
            postLinkerBuild.Execute(allFiles);
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
            var parent = Directory.GetParent(assemblyFile)!.FullName;
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
                        PostLinkerBuilds.Add(postLinkerBuild);
                        break;
                    case IPreLinkerBuild preLinkerBuild:
                        PreLinkerBuilds.Add(preLinkerBuild);
                        break;
                }
            }
        }
                
        PostLinkerBuilds.Sort((a, b) => a.executeOrder.CompareTo(b.executeOrder));
        PreLinkerBuilds.Sort((a, b) => a.executeOrder.CompareTo(b.executeOrder));
    }   
}