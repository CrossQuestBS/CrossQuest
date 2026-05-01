using System.ComponentModel;
using System.Diagnostics;
using CrossQuest.Android.Models;
using CrossQuest.Game;
using CrossQuest.Unity;
using Spectre.Console.Cli;

namespace CrossQuest.CLI.Commands.Games;

public class ExcludeDefenderCommand : AsyncCommand<ExcludeDefenderCommand.Settings>
{
    public class Settings : CommandSettings
    {
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings,
        CancellationToken cancellationToken)
    {
        int exitCode = 0;
        
        var crossQuestPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CrossQuest");
        Console.WriteLine($"Excluding {crossQuestPath} from Windows Defender");
        try
        {
            
            var processInfo = new ProcessStartInfo("powershell")
            {
                Verb = "runas",
                CreateNoWindow = true,
                ArgumentList = { $" -Command Add-MpPreference -ExclusionPath '{crossQuestPath}'" },
                UseShellExecute = false,
            };
            
            using var proc = new Process();
            proc.StartInfo = processInfo;
            proc.Start();

            await proc.WaitForExitAsync(cancellationToken);

            exitCode = proc.ExitCode;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }

        return exitCode;
    }
}