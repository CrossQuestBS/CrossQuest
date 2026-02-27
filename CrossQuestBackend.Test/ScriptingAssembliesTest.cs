using CrossQuestBackend.Compilation;

namespace CrossQuestBackend.Test;

public class Tests
{
    private ScriptingAssemblies _scriptingAssemblies;
    private readonly string _expectedValue = "{\"names\":[\"Unity.Assembly.dll\",\"Unity.Assembly2.dll\",\"User.Assembly.dll\",\"User.Assembly2.dll\"],\"types\":[2,2,16,16]}";
    
    [SetUp]
    public void Setup()
    {
        List<string> unityAssemblies = ["Unity.Assembly.dll", "Unity.Assembly2.dll"];
        List<string> userAssemblies = ["User.Assembly.dll", "User.Assembly2.dll"];
        _scriptingAssemblies = new ScriptingAssemblies(unityAssemblies, userAssemblies);
    }

    [Test]
    public void ShouldConvertToJson()
    {
        Assert.That(_scriptingAssemblies.ToJson(), Is.EqualTo(_expectedValue));
    }
}