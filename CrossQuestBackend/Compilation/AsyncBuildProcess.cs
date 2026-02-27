using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CrossQuestBackend.Compilation;

public class AsyncBuildProcess
{
    public Dictionary<string, string> BuildArguments = new ();
    public string BuildExecutablePath = "";
    
    public string ToArgumentsString()
    {
        var sb = new StringBuilder();
        foreach (var (key, value) in BuildArguments)
        {
            if (key.Length == 0)
                continue;
            
            sb.Append($"--{key.Trim()}");
            
            if (value.Length > 0)
                sb.Append($"={value.Trim()}");
            
            sb.Append(' ');
        }

        return sb.ToString().Trim();
    }
    
    public async Task Execute()
    {
        await ProcessCaller.ProcessAsync(BuildExecutablePath, ToArgumentsString());
    }
}