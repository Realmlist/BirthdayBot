using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Globalization;
using System.Text;

namespace BirthdayBot
{
    internal class Functions
    {
        public static async Task UpdateListEmbeds(InteractionContext ctx)
        {
            List<(ulong? guildId, ulong? channelId, ulong? messageId)> listMessageIds = await Database.GetListMessageId();
            List<(ulong guildId, ulong userId, DateTime birthday)> birthdays = await Database.GetBirthdays(ctx.Guild.Id);

            foreach (var (guildId, channelId, messageId) in listMessageIds)
            {
                if (guildId.HasValue && channelId.HasValue && messageId.HasValue && guildId.Value == ctx.Guild.Id)
                {
                    DiscordEmbed embed = BuildListEmbed(birthdays, ctx.Guild);
                    DiscordChannel channel = await ctx.Client.GetChannelAsync(channelId.Value);
                    if (channel != null)
                    {
                        DiscordMessage message = await channel.GetMessageAsync(messageId.Value);
                        if (message != null)
                        {
                            await message.ModifyAsync(embed: embed);
                        }
                    }
                }
            }
        }
        
        public static async Task UpdateListEmbeds(DiscordGuild guild, DiscordClient client) // Non-context edition
        {
            List<(ulong? guildId, ulong? channelId, ulong? messageId)> listMessageIds = await Database.GetListMessageId();
            List<(ulong guildId, ulong userId, DateTime birthday)> birthdays = await Database.GetBirthdays(guild.Id);

            foreach (var (guildId, channelId, messageId) in listMessageIds)
            {
                if (guildId.HasValue && channelId.HasValue && messageId.HasValue && guildId.Value == guild.Id)
                {
                    DiscordEmbed embed = BuildListEmbed(birthdays, guild);
                    DiscordChannel channel = await client.GetChannelAsync(channelId.Value);
                    if (channel != null)
                    {
                        DiscordMessage message = await channel.GetMessageAsync(messageId.Value);
                        if (message != null)
                        {
                            await message.ModifyAsync(embed: embed);
                        }
                    }
                }
            }
        }

        public static DiscordEmbed BuildListEmbed(List<(ulong guildId, ulong userId, DateTime birthday)> birthdays, DiscordGuild guild)
        {
            DateTime today = DateTime.UtcNow.Date;

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                                .WithTitle($"Birthday List - {guild.Name}")
                                .WithColor(DiscordColor.Gold)
                                .WithDescription("Register your birthday with\r\n" +
                                                    "`/birthday add <day> <month> [year]`");

            var groupedBirthdays = birthdays
                .OrderBy(b => b.birthday.Month)
                .ThenBy(b => b.birthday.Day)
                .GroupBy(b => b.birthday.Month);

            foreach (var monthGroup in groupedBirthdays)
            {
                string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthGroup.Key);
                StringBuilder monthFieldContent = new();

                foreach (var birthday in monthGroup)
                {
                    ulong userId = birthday.userId;
                    DiscordMember user = guild.GetMemberAsync(userId).Result;
                    int? age = null;

                    if (birthday.birthday.Year != 0001)
                    {
                        DateTime nextBirthday = new(today.Year, birthday.birthday.Month, birthday.birthday.Day);
                        age = today.Year - birthday.birthday.Year;
                        if (today < nextBirthday)
                            age--;
                    }

                    monthFieldContent.AppendLine($"{user.Mention} - {birthday.birthday:dd MMMM}{(age.HasValue && age.Value > 1 ? $" ({age.Value} years old)" : "")}");
                }
                embedBuilder.WithThumbnail(guild.GetIconUrl(ImageFormat.Auto, 1024));
                embedBuilder.AddField(monthName, monthFieldContent.ToString(), false);
            }

            // Add footer with bot name and last edit datetime
            embedBuilder.WithFooter($"Realm's BirthdayBot | Last edited: {DateTime.UtcNow:yyyy-MMM-dd HH:mm:ss} UTC");

            return embedBuilder.Build();
        }

        public static async Task CheckConfig(InteractionContext ctx)
        {
            ulong? channelId = await Database.GetChannelId(ctx.Guild.Id);
            if (channelId == 0 || channelId == null)
            {
                await ctx.CreateResponseAsync("**Birthday channel has not been set up!**\r\nRegister a channel with `/birthday channel`");
                return;
            }


            ulong? roleId = await Database.GetRoleId(ctx.Guild.Id);
            if (roleId == 0 || roleId == null)
            {
                await ctx.CreateResponseAsync("**Birthday role has not been set up!**\r\nRegister a role with `/birthday role`");
                return;
            }
            
        }

        public static async Task<Stream> DownloadFile(string url)
        {
            HttpClient _httpClient = new();

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }
    }
}
