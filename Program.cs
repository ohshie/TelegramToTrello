using Autofac;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
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
    public static IContainer? Container { get; private set; }
    public static async Task Main(string[] args)
    {
        Configuration.InitializeVariables();

        var builder = BuildContainer();
        Container = builder.Build();

        using (var scope = Container.BeginLifetimeScope())
        {
            var dbContext = scope.Resolve<BotDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }
        
        var botClinet = Container.Resolve<BotClient>();
        Task bot = botClinet.BotOperations();

        var webServer = Container.Resolve<WebServer>();
        Task server = webServer.Run(args);

        await Task.WhenAll(server, bot);

        Console.ReadLine();
        Environment.Exit(1);
    }

    private static ContainerBuilder BuildContainer()
    {
        var builder = new ContainerBuilder();
        
        builder.Register(bot =>
        {
            var token = Configuration.BotToken;
            return new TelegramBotClient(token);
        }).As<ITelegramBotClient>();
        
        builder.Register(x =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<BotDbContext>()
                .UseNpgsql((Configuration.ConnectionString));
            return optionsBuilder.Options;
        }).AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<BotDbContext>().AsSelf().InstancePerLifetimeScope();

        builder.RegisterType<BotClient>().AsSelf();
        builder.RegisterType<WebServer>().AsSelf();
        builder.RegisterType<TrelloOperations>().AsSelf();
        
        builder.RegisterType<ActionsFactory>().AsSelf();
        builder.RegisterType<CallbackFactory>().AsSelf();
        
        builder.RegisterType<UserRegistrationHandler>().AsSelf();
        
        builder.RegisterType<BotNotificationCentre>().AsSelf();
        builder.RegisterType<SyncService>().AsSelf();
        
        builder.RegisterType<SyncBoardDbOperations>().AsSelf();
        builder.RegisterType<SyncTablesDbOperations>().AsSelf();
        builder.RegisterType<SyncUsersDbOperations>().AsSelf();
        
        builder.RegisterType<StartTaskCreation>().AsSelf();
        builder.RegisterType<TaskPlaceholderOperator>().AsSelf();

        builder.RegisterType<DbOperations>().AsSelf();
        builder.RegisterType<TaskDbOperations>().AsSelf();
        builder.RegisterType<CreatingTaskDbOperations>().AsSelf();
        builder.RegisterType<UserDbOperations>().AsSelf();
        builder.RegisterType<NotificationsDbOperations>().AsSelf();

        builder.RegisterType<CurrentTasksDisplay>().AsSelf();
        builder.RegisterType<TaskInfoDisplay>().AsSelf();
        builder.RegisterType<CreateKeyboardWithBoards>().AsSelf();
        builder.RegisterType<MarkTaskAsCompleted>().AsSelf();
        builder.RegisterType<DropTask>();
        builder.RegisterType<DisplayCurrentTaskInfo>().AsSelf();

        builder.RegisterType<AddNameToTask>().AsSelf();
        builder.RegisterType<AddTagToTask>().AsSelf();
        builder.RegisterType<AddDescriptionToTask>().AsSelf();
        builder.RegisterType<AddBoardToTask>().AsSelf();
        builder.RegisterType<AddTableToTask>().AsSelf();
        builder.RegisterType<AddDateToTask>().AsSelf();
        builder.RegisterType<AddParticipantToTask>().AsSelf();
        builder.RegisterType<PushTask>().AsSelf();

        builder.RegisterType<TaskDateRequest>().AsSelf();
        builder.RegisterType<TaskNameRequest>().AsSelf();
        builder.RegisterType<TaskDescriptionRequest>().AsSelf();

        builder.RegisterType<CreateKeyboardWithBoards>().AsSelf();
        builder.RegisterType<CreateKeyboardWithTables>().AsSelf();
        builder.RegisterType<CreateKeyboardWithUsers>().AsSelf();
        builder.RegisterType<CreateKeyboardWithTags>().AsSelf();

        builder.RegisterType<TTTTaskRepository>().As<IRepository<TTTTask>>();
        builder.RegisterType<UsersRepository>().As<IUsersRepository>();
        builder.RegisterType<BoardRepository>().As<IRepository<Board>>();
        builder.RegisterType<TableRepository>().As<ITableRepository>();
        builder.RegisterType<TrelloUsersRepository>().As<ITrelloUsersRepository>();
        builder.RegisterType<NotificationsRepository>().As<INotificationsRepository>();

        return builder;
    }
}