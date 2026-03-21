using CrossQuestBackend.Unity.Compilation;
using CrossQuestBackend.Unity.Models;

namespace CrossQuestBackend.Test.Unity;

public class UnityResourcesTests
{
        private ScriptingAssemblies _scriptingAssemblies;

        [SetUp]
        public void Setup()
        {
            List<string> unityAssemblies = ["Path/To/Place/Unity.Assembly.dll", "Path/To/Place/Unity.Assembly2.dll"];
            List<string> userAssemblies = ["Path/To/Place/User.Assembly.dll", "Path/To/Place/User.Assembly2.dll"];
            _scriptingAssemblies = UnityResources.ScriptingAssemblies(unityAssemblies, userAssemblies);
        }
        
        public class ScriptingAssembliesTests : UnityResourcesTests
        {
            [Test]
            public void ShouldProperlyInsertTypes()
            {
                Assert.That(_scriptingAssemblies.Types, 
                    Is.EqualTo([2, 2, 16, 16]));
            }

            [Test]
            public void ShouldProperlyInsertNames()
            {
                Assert.That(_scriptingAssemblies.Names,
                    Is.EqualTo(["Unity.Assembly.dll", "Unity.Assembly2.dll", "User.Assembly.dll", "User.Assembly2.dll"]));
            }
        
            [Test]
            public void ShouldReturnValidJSON()
            {
                Assert.That(_scriptingAssemblies.AsJson(),
                    Is.EqualTo("{\"Names\":[\"Unity.Assembly.dll\",\"Unity.Assembly2.dll\",\"User.Assembly.dll\",\"User.Assembly2.dll\"],\"Types\":[2,2,16,16]}"));
            }
        }
}