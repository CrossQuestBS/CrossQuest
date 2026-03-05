using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossQuestBackend.Compilation;

public class AsyncBuildProcess
{
    public List<string> BuildArguments = new();
    public string BuildExecutablePath = "";
    private string _executeArguments
    {
        get => String.Join(" ", BuildArguments);
    }

    public async Task<bool> Execute()
    {
        return await ProcessCaller.ProcessAsync(BuildExecutablePath, _executeArguments);
    }
}