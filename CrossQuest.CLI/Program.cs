using CrossQuest.CLI.Commands.Games;
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
    config.SetExceptionHandler((ex, resolver) =>
    {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.StackTrace);
    });
    config.AddBranch("games", add =>
    {
        add.AddCommand<GamesListCommand>("list");
        add.AddCommand<GamesInstallCommand>("install");
        add.AddCommand<GamesCompileCommand>("compile");
        add.AddCommand<GamesSelectCommand>("select");
    });
    
    config.AddBranch("fix", add =>
    {
        add.AddCommand<ExcludeDefenderCommand>("defender");
    });
});

return await app.RunAsync(args, cancellationTokenSource.Token);