using Elsa;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.CreatingTaskOperations;
using TelegramToTrello.Notifications;
using TelegramToTrello.Repositories;
using TelegramToTrello.SyncDbOperations;
using TelegramToTrello.TaskManager;
using TelegramToTrello.TaskManager.CreatingTaskOperations;
using TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;
using TelegramToTrello.TaskManager.CreatingTaskOperations.RequestFromuser;
using TelegramToTrello.TaskManager.CurrentTaskOperations;
using TelegramToTrello.ToFromTrello;
using TelegramToTrello.UserRegistration;

namespace TelegramToTrello;

public class Program
{
    public static async Task Main(string[] args)
    {
        Configuration.InitializeVariables();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
        
        var host = new HostBuilder()
            .ConfigureServices(ConfigureServices)
            .UseConsoleLifetime()
            .UseSerilog()
            .Build();

        using (host)
        {
            await host.StartAsync();

            Log.Logger.Information("app starting");
            
            var dbContext = host.Services.GetRequiredService<BotDbContext>();
            await dbContext.Database.EnsureCreatedAsync();

            var botClient = host.Services.GetRequiredService<BotClient>();
            Task bot = botClient.BotOperations();
        
            var webServer = host.Services.GetRequiredService<WebServer>();
            Task server = webServer.Run(args);
        
            await Task.WhenAll(server, bot);

            await host.WaitForShutdownAsync();
        }
    }
    
    private static void ConfigureServices(IServiceCollection collection)
    {
        collection.AddSingleton<ITelegramBotClient>(sp =>
        {
            var token = Configuration.BotToken;
            return new TelegramBotClient(token);
        });

        collection.AddDbContext<BotDbContext>(sp =>
        {
            sp.UseNpgsql(Configuration.ConnectionString);
        });

        collection.AddHttpClient();
        collection.AddLogging();

        collection.AddTransient<BotClient>();
        collection.AddScoped<Message>();
        collection.AddTransient<WebServer>();
        collection.AddTransient<TrelloOperations>();
        
        collection.AddTransient<ActionsFactory>();
        collection.AddTransient<CallbackFactory>();
        
        collection.AddTransient<UserRegistrationHandler>();
        
        collection.AddTransient<BotNotificationCentre>();
        collection.AddTransient<SyncService>();
        
        collection.AddTransient<SyncBoardDbOperations>();
        collection.AddTransient<SyncTablesDbOperations>();
        collection.AddTransient<SyncUsersDbOperations>();
        
        collection.AddTransient<StartTaskCreation>();
        collection.AddTransient<TaskPlaceholderOperator>();

        collection.AddTransient<DbOperations>();
        collection.AddTransient<TaskDbOperations>();
        collection.AddTransient<CreatingTaskDbOperations>();
        collection.AddTransient<UserDbOperations>();
        collection.AddTransient<NotificationsDbOperations>();

        collection.AddTransient<CurrentTasksDisplay>();
        collection.AddTransient<TaskInfoDisplay>();
        collection.AddTransient<CreateKeyboardWithBoards>();
        collection.AddTransient<MarkTaskAsCompleted>();
        collection.AddTransient<DropTask>();
        collection.AddTransient<DisplayCurrentTaskInfo>();

        collection.AddTransient<AddNameToTask>();
        collection.AddTransient<AddTagToTask>();
        collection.AddTransient<AddDescriptionToTask>();
        collection.AddTransient<AddBoardToTask>();
        collection.AddTransient<AddTableToTask>();
        collection.AddTransient<AddDateToTask>();
        collection.AddTransient<AddParticipantToTask>();
        collection.AddTransient<AddAttachmentToTask>();
        collection.AddTransient<PushTask>();

        collection.AddTransient<TaskDateRequest>();
        collection.AddTransient<TaskNameRequest>();
        collection.AddTransient<TaskDescriptionRequest>();
        collection.AddTransient<AttachmentRequest>();

        collection.AddTransient<CreateKeyboardWithBoards>();
        collection.AddTransient<CreateKeyboardWithTables>();
        collection.AddTransient<CreateKeyboardWithUsers>();
        collection.AddTransient<CreateKeyboardWithTags>();

        collection.AddTransient<IRepository<TTTTask>, TTTTaskRepository>();
        collection.AddTransient<IUsersRepository, UsersRepository>();
        collection.AddTransient<IRepository<Board>, BoardRepository>();
        collection.AddTransient<ITableRepository, TableRepository>();
        collection.AddTransient<ITrelloUsersRepository, TrelloUsersRepository>();
        collection.AddTransient<INotificationsRepository, NotificationsRepository>();
        collection.AddTransient<IBoardRepository, BoardRepository>();

        collection.AddElsa(builder =>
        {
            builder.AddQuartzTemporalActivities().
                AddWorkflow<SyncService>().
                AddWorkflow<BotNotificationCentre>();
        });
    }
}