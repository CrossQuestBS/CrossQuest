using CrossQuestBackend.Unity.Compilation;
using CrossQuestBackend.Unity.Models;

namespace CrossQuestBackend.Test;

public class ScriptingAssembliesTests
{
    private ScriptingAssemblies _scriptingAssemblies;

    [SetUp]
    public void Setup()
    {
        List<string> unityAssemblies = ["Unity.Assembly.dll", "Unity.Assembly2.dll"];
        List<string> userAssemblies = ["User.Assembly.dll", "User.Assembly2.dll"];
        _scriptingAssemblies = UnityResourceService.GenerateScriptingAssemblies(unityAssemblies, userAssemblies);
    }

    [Test]
    public void ShouldProperlyInsertTypes()
    {
        Assert.That(_scriptingAssemblies.Types, Is.EqualTo([2, 2, 16, 16]));
    }

    [Test]
    public void ShouldProperlyInsertNames()
    {
        Assert.That(_scriptingAssemblies.Names,
            Is.EqualTo(["Unity.Assembly.dll", "Unity.Assembly2.dll", "User.Assembly.dll", "User.Assembly2.dll"]));
    }
}