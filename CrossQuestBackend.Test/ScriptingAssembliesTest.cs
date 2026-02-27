using CrossQuestBackend.Compilation;

namespace CrossQuestBackend.Test;

public class ScriptingAssembliesTests
{
    private ScriptingAssemblies _scriptingAssemblies;
    private const string ExpectedValue = "{\"names\":[\"Unity.Assembly.dll\",\"Unity.Assembly2.dll\",\"User.Assembly.dll\",\"User.Assembly2.dll\"],\"types\":[2,2,16,16]}";

    [SetUp]
    public void Setup()
    {
        List<string> unityAssemblies = ["Unity.Assembly.dll", "Unity.Assembly2.dll"];
        List<string> userAssemblies = ["User.Assembly.dll", "User.Assembly2.dll"];
        _scriptingAssemblies = new ScriptingAssemblies(unityAssemblies, userAssemblies);
    }

    [Test]
    public void ShouldProperlyInsertTypes()
    {
        Assert.That(_scriptingAssemblies.types, Is.EqualTo([2, 2, 16, 16]));
    }
    
    [Test]
    public void ShouldProperlyInsertNames()
    {
        Assert.That(_scriptingAssemblies.names, Is.EqualTo(["Unity.Assembly.dll", "Unity.Assembly2.dll", "User.Assembly.dll", "User.Assembly2.dll"]));
    }

    [Test]
    public void ShouldConvertToJson()
    {
        Assert.That(_scriptingAssemblies.ToJson(), Is.EqualTo(ExpectedValue));
    }
}