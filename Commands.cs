using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;


namespace BirthdayBot
{
    [SlashCommandGroup("birthday", "Allows you to add, remove and list birthdays.")]
    public class Commands : ApplicationCommandModule
    {

        // Only server admins can use the setup channel command
        [SlashCommand("channel", "Sets up the bot in the current channel"), SlashRequireUserPermissions(Permissions.Administrator)]
        public static async Task SetupChannel(InteractionContext ctx,
        [Option("channel", "The channel to set up the bot in")] DiscordChannel channel)
        {
            await Database.SetChannelId(ctx.Guild.Id, channel.Id);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"Bot set up in {channel.Mention}!"));
        }

        // Only server admins can use the setup role command
        [SlashCommand("role", "Sets up the role for the bot to assign to the birthday person"), SlashRequireUserPermissions(Permissions.Administrator)]
        public static async Task SetupRole(InteractionContext ctx,
        [Option("role", "Role that will be assigned to the birthday person")] DiscordRole role)
        {
            await Database.SetRole(ctx.Guild.Id, role.Id);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"Configured {role.Mention}!"));
        }


        [SlashCommand("add", "Adds your birthday to the bot")]
        public static async Task AddBirthday(InteractionContext ctx,
            [Option("day", "Your birthday day")] long day,
            [Option("month", "Your birthday month")] long month,
            [Option("year", "Your birthday year (optional)")] long? year = null)
        {
            DateTime birthday;
            if (year.HasValue)
            {
                if (!DateTime.TryParse($"{year.Value}-{month}-{day}", out birthday))
                {
                    await ctx.CreateResponseAsync("Invalid date!", true);
                    return;
                }
            }
            else
            {
                try
                {
                    birthday = new DateTime(0001, (int)month, (int)day);
                }
                catch (ArgumentOutOfRangeException)
                {
                    await ctx.CreateResponseAsync("Invalid date!", true);
                    return;
                }
            }

            await Database.AddBirthday(ctx.Guild.Id, ctx.User.Id, birthday);
            await ctx.CreateResponseAsync("Birthday added!", true);

            await Functions.CheckConfig(ctx);

            await Functions.UpdateListEmbeds(ctx);

            
        }

        [SlashCommand("remove", "Removes your birthday from the bot")]
        public static async Task RemoveBirthday(InteractionContext ctx)
        {
            await Database.RemoveBirthday(ctx.Guild.Id, ctx.User.Id);
            await ctx.CreateResponseAsync("Birthday removed!", true);

            await Functions.UpdateListEmbeds(ctx);

            
        }

        [SlashCommand("list", "Lists all set birthdays and their current age if they have set a year")]
        public static async Task ListBirthdays(InteractionContext ctx)
        {
            // Return something else it throws an error...
            await ctx.CreateResponseAsync("Here's a new list!", true);

            // Fetch birthdays
            List<(ulong guildId, ulong userId, DateTime birthday)> birthdays = await Database.GetBirthdays(ctx.Guild.Id);
            // Build embed
            DiscordEmbed embed = Functions.BuildListEmbed(birthdays, ctx.Guild);
            // Send Embed
            DiscordMessage message = await ctx.Channel.SendMessageAsync(embed: embed);
            // Save its id to the DB
            await Database.SetListMessageId(ctx.Guild.Id, ctx.Channel.Id, message.Id);

        }

        private readonly BirthdayCheckerService _birthdayChecker;

        public Commands(BirthdayCheckerService birthdayChecker)
        {
            _birthdayChecker = birthdayChecker;
        }

        [SlashCommand("check", "Manually checks for birthdays and sends a message in the specified channel"), SlashRequireUserPermissions(Permissions.Administrator)]
        public async Task CheckBirthdays(InteractionContext ctx)
        {
            await Functions.CheckConfig(ctx);

            await _birthdayChecker.CheckBirthdaysAsync();

            await ctx.CreateResponseAsync("Ran birthday check manually!", true);

            await Functions.UpdateListEmbeds(ctx);
            
        }

        [SlashCommand("update", "Updates all lists in the server"), SlashRequireUserPermissions(Permissions.Administrator)]
        public static async Task UpdateListCommand(InteractionContext ctx)
        {
            await Functions.CheckConfig(ctx);

            await ctx.CreateResponseAsync("Ran manual update!", true);

            await Functions.UpdateListEmbeds(ctx);

        }

    }
}