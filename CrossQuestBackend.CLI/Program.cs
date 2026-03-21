using CrossQuestBackend.CLI.Commands;
using CrossQuestBackend.CLI.Commands.Games;
using CrossQuestBackend.Unity.Compilation;
using Spectre.Console.Cli;

var cancellationTokenSource = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cancellationTokenSource.Cancel();
    Console.WriteLine("Cancelling!");
};

var app = new CommandApp();
app.Configure(config =>
{
    config.AddBranch("games", add =>
    {
        add.AddCommand<GamesListCommand>("list");
        add.AddCommand<GamesInstallCommand>("install");
        add.AddCommand<GamesCompileCommand>("compile");
    });
});

return await app.RunAsync(args, cancellationTokenSource.Token);