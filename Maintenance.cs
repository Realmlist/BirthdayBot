using DSharpPlus;
using DSharpPlus.EventArgs;
using System.Threading.Tasks;

namespace BirthdayBot
{
    public class Maintenance
    {
        public static Task Initialize(DiscordClient client)
        {
            client.ChannelDeleted += OnChannelDeleted;
            client.GuildMemberRemoved += OnMemberLeave;
            return Task.CompletedTask;
        }

        // Remove database rows of embeds in deleted channel
        private static async Task OnChannelDeleted(DiscordClient client, ChannelDeleteEventArgs e)
        {
            ulong deletedChannelId = e.Channel.Id;
            await Database.RemoveListEmbedByChannelId(deletedChannelId);
        }


        // Remove member from server's birthday list when they leave server
        private static async Task OnMemberLeave(DiscordClient client, GuildMemberRemoveEventArgs e)
        {
            ulong leftMemberId = e.Member.Id;
            ulong guildId = e.Guild.Id;
            await Database.RemoveBirthday(guildId, leftMemberId);
        }
    }
}