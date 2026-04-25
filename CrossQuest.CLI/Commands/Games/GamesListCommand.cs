using Spectre.Console.Cli;

namespace CrossQuest.CLI.Commands.Games;

public class GamesListCommand : AsyncCommand<GamesListCommand.Settings>
{
    public class Settings : CommandSettings
    {
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings,
        CancellationToken cancellationToken)
    {
        try
        {
            var games = await ResourceDownloader.Games();

            var gameNames = games.Select(it => it.Id);

            Console.WriteLine("Games");
            foreach (var game in games)
            {
                Console.WriteLine($"  {game.Id}");
                foreach (var version in game.ModdableVersionList)
                {
                    var InstancePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrossQuest", "Games", game.Id, $"{version.Version}");

                    Console.WriteLine($"   - {version.Version}");
                    Console.WriteLine($"   {InstancePath}");
                    Console.WriteLine();
                }
            }

            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }
    }
}