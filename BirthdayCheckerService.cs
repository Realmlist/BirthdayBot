using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

namespace BirthdayBot
{
    public class BirthdayCheckerService
    {
        private readonly DiscordClient _client;
        private readonly CancellationTokenSource _cts;
        
        public BirthdayCheckerService(DiscordClient client)
        {
            _client = client;
            _cts = new CancellationTokenSource();
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    var now = DateTime.UtcNow;
                    var nextCheck = now.Date.AddDays(1);

                    await Task.Delay(nextCheck - now, _cts.Token);

                    await CheckBirthdaysAsync();
                }
            });
        }

        public void Stop()
        {
            _cts.Cancel();
        }

        public async Task CheckBirthdaysAsync()
        {
            var birthdays = await Database.GetBirthdays(0);
            var today = DateTime.UtcNow.Date;

            foreach (var (guildId, userId, birthday) in birthdays)
            {
                DiscordGuild guild = await _client.GetGuildAsync(guildId);
                DiscordMember member = await guild.GetMemberAsync(userId);

                var roleId = await Database.GetRoleId(guildId);
                
                

                if (birthday.Month == today.Month && birthday.Day == today.Day)
                {
                    var user = await _client.GetUserAsync(userId);
                    int? age = null;

                    if (birthday.Year > 1)
                    {
                        var nextBirthday = new DateTime(today.Year, birthday.Month, birthday.Day);
                        age = today.Year - birthday.Year;
                        if (today < nextBirthday)
                            age--;
                    }

                    await HBDBuilder(guildId, member, age);

                    if (roleId.HasValue)
                    {
                        ulong rId = roleId.Value;
                        DiscordRole bdayRole = guild.GetRole(rId);
                        await member.GrantRoleAsync(bdayRole, "It's their birthday today!");
                    }

                }else{

                    if (roleId.HasValue)
                    {
                        var roles = member.Roles;
                        ulong rId = roleId.Value;
                        DiscordRole bdayRole = guild.GetRole(rId);

                        if (roles.Contains(bdayRole))
                        {
                            await member.RevokeRoleAsync(bdayRole, "It is no longer their birthday.");
                        }
                    }

                }
            }

        }

        public async Task HBDBuilder(ulong guildId, DiscordMember member, int? age)
        {
            var channelId = await Database.GetChannelId(guildId);
            if (!channelId.HasValue)
                return;

            var channel = await _client.GetChannelAsync(channelId.Value);
            if (channel == null)
                return;
            var ageExtra = age.HasValue && age.Value > 1 ? $"They are {age.Value} years old today!" : "";

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Gold,
                Title = $"Birthday Announcement!",
                Description = $"**Please wish {member.Mention} a happy birthday! 🎂**\r\n{ageExtra}",
            };

            var videos = new List<string>
            {
                "https://cdn.discordapp.com/attachments/1106239191559454893/1106239377547473057/Today_is_Birthday_in_VRChat.mp4",
                "https://cdn.discordapp.com/attachments/1106239191559454893/1106239378046591068/Today_is_my_birthday.mp4"
            };
            var random = new Random();
            int randomVid = random.Next(videos.Count);

            await channel.SendMessageAsync(embed: embed);
            await channel.SendMessageAsync(videos[randomVid]);
            
         
        }
    }
}
