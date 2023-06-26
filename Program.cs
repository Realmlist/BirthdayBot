using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using DSharpPlus.Entities;

namespace BirthdayBot
{
    public class Program
    {
        public static DiscordClient? Client { get; private set; }
        public static SlashCommandsExtension? Slash { get; private set; }

        // Configured in VS2022: Debug -> Debug properties -> Env variables
        // For production use Docker environmental variables
        public static string botToken = Environment.GetEnvironmentVariable("TOKEN");
        public static string server = Environment.GetEnvironmentVariable("SQLSERVER");
        public static int port = int.Parse(Environment.GetEnvironmentVariable("PORT"));
        public static string user = Environment.GetEnvironmentVariable("USER");
        public static string password = Environment.GetEnvironmentVariable("PASSWORD");
        public static string database = Environment.GetEnvironmentVariable("DATABASE");

        public static string connectionString = $"server={server};user={user};database={database};port={port};password={password};";

        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            // Set up the database
            await Database.SetupDatabase();

            // Bot Config
            var config = new DiscordConfiguration()
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
            var birthdayChecker = new BirthdayCheckerService(Client);
            birthdayChecker.Start();

            // Set up Slash commands
            var services = new ServiceCollection()
                .AddSingleton(birthdayChecker)
                .BuildServiceProvider();

            Slash = Client.UseSlashCommands(new SlashCommandsConfiguration() { Services = services });

            // Only register for test server, remove ulong for global activation
            ulong guildId = 702106468849156127;
            Slash.RegisterCommands<Commands>(guildId);

            // Set a nice discord status for the bot
            DiscordActivity status = new("the calendar", ActivityType.Watching);

            // Connect to Discord
            await Client.ConnectAsync(status, UserStatus.Online);
            await Task.Delay(-1);
        }
    }

    
}
