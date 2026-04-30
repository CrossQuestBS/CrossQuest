using System.ComponentModel;
using CrossQuest.Android;
using CrossQuest.Android.Models;
using CrossQuest.Game;
using CrossQuest.Unity;
using Spectre.Console.Cli;

namespace CrossQuest.CLI.Commands.Games;

public class GamesInstallCommand : AsyncCommand<GamesInstallCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<id>")]
        [Description("The game id to mod")]
        public string GameId { get; init; } = string.Empty;

        [CommandArgument(0, "<version>")]
        [Description("The game version to mod")]
        public string GameVersion { get; init; } = string.Empty;

        [CommandOption("-t|--token")]
        [Description("Oculus token to download resources")]
        [DefaultValue("")]
        public string Token { get; init; } = "";
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings,
        CancellationToken cancellationToken)
    {
        if (!settings.Token.StartsWith("OC"))
        {
            Console.WriteLine("Invalid Oculus token, it must start with OC");
            return 1;
        }
        
        var games = await ResourceDownloader.Games();

        var game = games.FirstOrDefault(it => it.Id == settings.GameId);

        if (game is null)
        {
            Console.WriteLine($"Game with id: {settings.GameId} not found :(");
            return 1;
        }

        var version = game.ModdableVersionList.FirstOrDefault(it => it.Version == settings.GameVersion);

        if (version is null)
        {
            Console.WriteLine($"Game {settings.GameId}: could not find version {settings.GameVersion}:(");
            return 1;
        }

        var directory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrossQuest",
            "Games", game.Id, $"{version.Version}");

        var finishedFile = Path.Join(directory, ".crossquest");

        if (File.Exists(finishedFile))
        {
            Console.WriteLine(
                $"File {finishedFile} already exists, skipping rest of install. Delete file to install again");
            return 0;
        }

        var unityInstance = new UnityInstance(version.UnityVersion);
        var instance = new GameInstance(version, game.Id);

     

        Console.WriteLine("Setting up instance");
        await instance.SetupInstance(settings.Token, unityInstance);

        var apkSignerPath = await AndroidToolsDownloader.DownloadApkSigner();
        var adb = await AndroidToolsDownloader.DownloadADB();
        var apktoolJar = await AndroidToolsDownloader.DownloadApktool();
        var ndkPath = await AndroidToolsDownloader.DownloadNDK();

        var androidTools = new AndroidTools(ndkPath, apkSignerPath, adb, apktoolJar);
        Console.WriteLine("Saving AndroidTools as json");
        await androidTools.Save(unityInstance);

        var CrossInstance = new CrossInstance(unityInstance, instance, androidTools);
        await CrossInstance.SaveInstance(settings.GameId, settings.GameVersion);
        await CrossInstance.SetAsActiveInstance(settings.GameId, settings.GameVersion);

        await File.WriteAllTextAsync(finishedFile, "Hello!", cancellationToken);
        return 0;
    }
}