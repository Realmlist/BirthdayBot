using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DSharpPlus.Entities;

namespace BirthdayBot
{
    public class Program
    {
        public static DiscordClient? Client { get; private set; }
        public static SlashCommandsExtension? Slash { get; private set; }
        public static string ConnectionString { get => connectionString; }

    #pragma warning disable CS8601, CS8604 // Possible null reference assignment.
        // Configured in VS2022: Debug -> Debug properties -> Env variables
        // For production use Docker environmental variables
        private static readonly string botToken = Environment.GetEnvironmentVariable("TOKEN");
        private static readonly string server   = Environment.GetEnvironmentVariable("SQLSERVER");
        private static readonly int port        = int.Parse(Environment.GetEnvironmentVariable("PORT"));
        private static readonly string user     = Environment.GetEnvironmentVariable("USER");
        private static readonly string password = Environment.GetEnvironmentVariable("PASSWORD");
        private static readonly string database = Environment.GetEnvironmentVariable("DATABASE");
    #pragma warning restore CS8601, CS8604 // Possible null reference assignment.
        
        private static readonly string connectionString = $"server={server};user={user};database={database};port={port};password={password};";

        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            // Set up the database
            await Database.SetupDatabase();

            // Bot Config
            DiscordConfiguration config = new()
            {
                Token = botToken,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                Intents = DiscordIntents.All,
                MinimumLogLevel = LogLevel.Debug,
                LogTimestampFormat = "yyyy MMM dd - hh:mm:ss tt"
            };

            // Set up config
            Client = new DiscordClient(config);

            // Initialize Maintenance class
            await Maintenance.Initialize(Client);

            // Start the birthday checker
            BirthdayCheckerService birthdayChecker = new(Client);
            birthdayChecker.Start();

            // Set up Slash commands
            ServiceProvider services = new ServiceCollection()
                            .AddSingleton(birthdayChecker)
                            .BuildServiceProvider();

            Slash = Client.UseSlashCommands(new SlashCommandsConfiguration() { Services = services });

            // Only register for test server, remove ulong for global activation
            //ulong guildId = 702106468849156127;
            Slash.RegisterCommands<Commands>();

            // Set a nice discord status for the bot
            DiscordActivity status = new("the calendar", ActivityType.Watching);

            // Connect to Discord
            await Client.ConnectAsync(status, UserStatus.Online);
            await Task.Delay(-1);
        }
    }

    
}
