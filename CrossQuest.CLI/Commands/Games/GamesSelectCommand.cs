using System.ComponentModel;
using CrossQuest.Android.Models;
using CrossQuest.Game;
using CrossQuest.Unity;
using Spectre.Console.Cli;

namespace CrossQuest.CLI.Commands.Games;

public class GamesSelectCommand : AsyncCommand<GamesSelectCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<id>")]
        [Description("The game id to mod")]
        public string GameId { get; init; } = string.Empty;

        [CommandArgument(0, "<version>")]
        [Description("The game version to mod")]
        public string GameVersion { get; init; } = string.Empty;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings,
        CancellationToken cancellationToken)
    {
        try
        {
            var games = await ResourceDownloader.Games();

            var game = games.FirstOrDefault(it => it.Id == settings.GameId);

            if (game is null)
            {
                Console.WriteLine($"Game with id: {settings.GameId} not found :(");
                return 1;
            }

            var version = game.ModdableVersionList.FirstOrDefault(it => it.Version == settings.GameVersion);
            var InstancePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrossQuest", "Games", game.Id, $"{version.Version}");

            var crossQuestFile = Path.Join(InstancePath, ".crossquest");
            
            if (!File.Exists(crossQuestFile))
            {
                Console.WriteLine($"Could not find the instance  {settings.GameId}/{settings.GameVersion}");
                return 1;
            }

            await CrossInstance.SetAsActiveInstance(settings.GameId, settings.GameVersion);

            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }
    }
}