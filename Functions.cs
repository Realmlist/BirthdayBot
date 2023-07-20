using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot
{
    internal class Functions
    {
        public static async Task UpdateListEmbeds(DiscordClient client, DiscordGuild guild)
        {
            
            var birthdays = await Database.GetBirthdays(guild.Id);
            var embed = BuildListEmbed(birthdays, guild);

            var listMessageIds = await Database.GetListMessageId();

            foreach (var (channelId, messageId) in listMessageIds)
            {
                if (channelId.HasValue && messageId.HasValue)
                {
                    var channel = await client.GetChannelAsync(channelId.Value);
                    if (channel != null)
                    {
                        var message = await channel.GetMessageAsync(messageId.Value);
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
            var today = DateTime.UtcNow.Date;

            var embedBuilder = new DiscordEmbedBuilder()
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
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthGroup.Key);
                var monthFieldContent = new StringBuilder();

                foreach (var birthday in monthGroup)
                {
                    var userId = birthday.userId;
                    var user = guild.GetMemberAsync(userId).Result;
                    int? age = null;

                    if (birthday.birthday.Year != 0001)
                    {
                        var nextBirthday = new DateTime(today.Year, birthday.birthday.Month, birthday.birthday.Day);
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
            var lastEditDateTime = File.GetLastWriteTimeUtc(System.Reflection.Assembly.GetExecutingAssembly().Location);
            embedBuilder.WithFooter($"Realm's BirthdayBot | Last edited: {lastEditDateTime.ToString("yyyy-MM-dd HH:mm:ss")} UTC");

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
            var _httpClient = new HttpClient();

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }
    }
}
