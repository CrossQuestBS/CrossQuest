using CrossQuestBackend.Compilation;

namespace CrossQuestBackend.Test;

public class AsyncBuildProcessTests
{
    private AsyncBuildProcess? _buildProcess;
    
    [SetUp]
    public void Setup()
    {
        _buildProcess = new AsyncBuildProcess();
    }
    
    [Test]
    public void ShouldPrintCorrectArguments()
    {
        if (_buildProcess is null)
        {
            Assert.Fail("Build Process is empty");
            return;
        }
            
        _buildProcess.BuildArguments.Add("test", "");
        _buildProcess.BuildArguments.Add("dir", "found");
        _buildProcess.BuildArguments.Add("", "");

        var expected = "--test --dir=found";
        var actual = _buildProcess.ToArgumentsString();
        Assert.That(actual, Is.EqualTo(expected));
    }
}