Configuration.InitializeVariables();

using BotDbContext dbContext = new BotDbContext();
{
    await dbContext.Database.EnsureCreatedAsync();
}

var botClient = new BotClient();
Task bot = botClient.BotOperations();

var server = new WebServer();
Task webServer = server.Run(args);

await Task.WhenAll(webServer, bot);
        
Console.ReadLine();
Environment.Exit(1);