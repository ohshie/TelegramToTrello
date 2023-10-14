using System.Net.Sockets;
using Elsa;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;
using TelegramToTrello.Notifications;
using TelegramToTrello.Repositories;
using TelegramToTrello.SyncDbOperations;
using TelegramToTrello.TaskManager;
using TelegramToTrello.TaskManager.CreatingTaskOperations;
using TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;
using TelegramToTrello.TaskManager.CreatingTaskOperations.RequestFromuser;
using TelegramToTrello.TaskManager.CurrentTaskOperations;
using TelegramToTrello.TemplateManager;
using TelegramToTrello.TemplateManager.CreatingTemplatesOperations;
using TelegramToTrello.TemplateManager.CreatingTemplatesOperations.AddToTemplate;
using TelegramToTrello.TemplateManager.CreatingTemplatesOperations.CreateKeyboards;
using TelegramToTrello.TemplateManager.CreatingTemplatesOperations.RequestFromUser;
using TelegramToTrello.ToFromTrello;
using TelegramToTrello.UserRegistration;

namespace TelegramToTrello;

public class Program
{
    public static async Task Main(string[] args)
    {
        Configuration.InitializeVariables();
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Warning()
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
        
        // bot classes
        collection.AddTransient<BotClient>();
        collection.AddScoped<Message>();
        collection.AddTransient<WebServer>();
        collection.AddTransient<TrelloOperations>();
        
        collection.AddTransient<BotSettingsMenu>();
        
        collection.AddTransient<ActionsFactory>();
        collection.AddTransient<CallbackFactory>();
        collection.AddTransient<PlaceholderOperator>();

        collection.AddTransient<BotMessenger>();
        
        collection.AddTransient<UserRegistrationHandler>();
        
        collection.AddTransient<BotNotificationCentre>();
        collection.AddTransient<SyncService>();
        
        // keyboard classes
        collection.AddTransient<MenuKeyboards>();
        collection.AddTransient<BoardsKeyboard>();
        collection.AddTransient<TablesKeyboard>();
        collection.AddTransient<TagsKeyboard>();
        collection.AddTransient<UsersKeyboard>();
        collection.AddTransient<DisplayTaskKeyboard>();
        collection.AddTransient<ConfirmTemplateKeyboard>();
        collection.AddTransient<TemplatesKeyboard>();
        collection.AddTransient<DateKeyboard>();
        collection.AddTransient<CurrentTasksKeyboard>();
        
        // template classes
        collection.AddTransient<TemplateHandler>();
        collection.AddTransient<StartTemplateCreation>();
        collection.AddTransient<DisplayTemplate>();
        collection.AddTransient<ConfirmTemplate>();
        
        collection.AddTransient<TemplateCreateKbWithBoards>();
        collection.AddTransient<TemplateCreateKBWithTables>();
        
        collection.AddTransient<AddBoardToTemplate>();
        collection.AddTransient<AddNameToTemplate>();
        collection.AddTransient<AddDescToTemplate>();
        collection.AddTransient<AddTableToTemplate>();

        collection.AddTransient<RequestDesc>();
        collection.AddTransient<RequestName>();
        
        // Db classes
        collection.AddTransient<SyncBoardDbOperations>();
        collection.AddTransient<SyncTablesDbOperations>();
        collection.AddTransient<SyncUsersDbOperations>();
        
        collection.AddTransient<DbOperations>();
        collection.AddTransient<TaskDbOperations>();
        collection.AddTransient<DialogueStorageDbOperations>();
        collection.AddTransient<CreatingTaskDbOperations>();
        collection.AddTransient<UserDbOperations>();
        collection.AddTransient<NotificationsDbOperations>();
        collection.AddTransient<TemplatesDbOperations>();
        collection.AddTransient<Verifier>();
        
        collection.AddTransient<IUsersRepository, UsersRepository>();
        collection.AddTransient<IRepository<Board>, BoardRepository>();
        collection.AddTransient<ITableRepository, TableRepository>();
        collection.AddTransient<ITrelloUsersRepository, TrelloUsersRepository>();
        collection.AddTransient<INotificationsRepository, NotificationsRepository>();
        collection.AddTransient<IBoardRepository, BoardRepository>();
        collection.AddTransient<ITemplateRepository, TemplateRepository>();
        collection.AddTransient<IDialogueStorageRepository, DialogueStorageRepository>();
        collection.AddTransient<ITTTTaskRepository, TTTTaskRepository>();
        
        // current task classes
        collection.AddTransient<CurrentTasksDisplay>();
        collection.AddTransient<TaskInfoDisplay>();
        collection.AddTransient<MarkTaskAsCompleted>();
        
        // creating task classes
        collection.AddTransient<StartTaskCreation>();
        collection.AddTransient<DisplayCurrentTaskInfo>();
        collection.AddTransient<DropTask>();
        collection.AddTransient<PushTask>();
        
        collection.AddTransient<AddBoardToTask>();
        
        collection.AddTransient<CreateKeyboardWithTemplate>();
        collection.AddTransient<AssembleTaskFromTemplate>();
        
        collection.AddTransient<AddNameToTask>();
        collection.AddTransient<AddTagToTask>();
        collection.AddTransient<AddDescriptionToTask>();
        
        collection.AddTransient<AddTableToTask>();
        collection.AddTransient<AddDateToTask>();
        collection.AddTransient<AddParticipantToTask>();
        collection.AddTransient<AddAttachmentToTask>();
        
        collection.AddTransient<TaskDateRequest>();
        collection.AddTransient<TaskNameRequest>();
        collection.AddTransient<TaskDescriptionRequest>();
        collection.AddTransient<AttachmentRequest>();
        
        collection.AddTransient<CreateKeyboardWithBoards>();
        collection.AddTransient<CreateKeyboardWithTables>();
        collection.AddTransient<CreateKeyboardWithUsers>();
        collection.AddTransient<CreateKeyboardWithTags>();

        collection.AddElsa(builder =>
        {
            builder.AddQuartzTemporalActivities().
                AddWorkflow<SyncService>().
                AddWorkflow<BotNotificationCentre>();
        });
    }
}