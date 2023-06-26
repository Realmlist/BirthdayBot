using MySqlConnector;
using System.Threading.Channels;

namespace BirthdayBot
{
    public static class Database
    {

        public static async Task SetupDatabase()
        {
            using (var conn = new MySqlConnection(Program.connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS birthdays (guildId BIGINT PRIMARY KEY, userId BIGINT, birthday DATE)";
                    await cmd.ExecuteNonQueryAsync();

                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS config (guildId BIGINT PRIMARY KEY, channelId BIGINT NOT NULL DEFAULT '0', roleId BIGINT NOT NULL DEFAULT '0')";
                    await cmd.ExecuteNonQueryAsync();

                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS listembeds (channelId BIGINT, messageId BIGINT PRIMARY KEY)";
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task<ulong?> GetChannelId(ulong guildId)
        {
            using (var conn = new MySqlConnection(Program.connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT channelId FROM config WHERE guildId=@guildId";
                    cmd.Parameters.AddWithValue("@guildId", guildId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            ulong channelId = reader.GetUInt64(0);
                            return channelId;
                        }
                        else
                        {
                            return null;
                        }
                        
                    }
                }
            }
        }

        public static async Task SetChannelId(ulong guildId, ulong channelId)
        {
            using (var conn = new MySqlConnection(Program.connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO config (guildId, channelId) VALUES (@guildId, @channelId) ON DUPLICATE KEY UPDATE channelId=@channelId";
                    cmd.Parameters.AddWithValue("@guildId", guildId);
                    cmd.Parameters.AddWithValue("@channelId", channelId);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task SetRole(ulong guildId, ulong roleId)
        {
            using (var conn = new MySqlConnection(Program.connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO config (guildId, channelId) VALUES (@guildId, @roleId) ON DUPLICATE KEY UPDATE roleId=@roleId";
                    cmd.Parameters.AddWithValue("@guildId", guildId);
                    cmd.Parameters.AddWithValue("@roleId", roleId);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task<ulong?> GetRoleId(ulong guildId)
        {
            using (var conn = new MySqlConnection(Program.connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT roleId FROM config WHERE guildId=@guildId";
                    cmd.Parameters.AddWithValue("@guildId", guildId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            ulong roleId = reader.GetUInt64(0);
                            return roleId;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

        public static async Task<List<(ulong guildId, ulong userId, DateTime birthday)>> GetBirthdays()
        {
            var birthdays = new List<(ulong guildId, ulong userId, DateTime birthday)>();

            using (var conn = new MySqlConnection(Program.connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT guildId, userId, birthday FROM birthdays";

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            birthdays.Add((reader.GetUInt64(0), reader.GetUInt64(1), reader.GetDateTime(2)));
                        }
                    }
                }
            }

            return birthdays;
        }

        public static async Task AddBirthday(ulong guildId, ulong userId, DateTime birthday)
        {
            using (var conn = new MySqlConnection(Program.connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO birthdays (guildId, userId, birthday) VALUES (@guildId, @userId, @birthday) ON DUPLICATE KEY UPDATE birthday=@birthday";
                    cmd.Parameters.AddWithValue("@guildId", guildId);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@birthday", birthday);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task RemoveBirthday(ulong guildId, ulong userId)
        {
            using (var conn = new MySqlConnection(Program.connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "DELETE FROM birthdays WHERE guildId=@guildId AND userId=@userId";
                    cmd.Parameters.AddWithValue("@guildId", guildId);
                    cmd.Parameters.AddWithValue("@userId", userId);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task<List<(ulong? channelId, ulong? messageId)>> GetListMessageId()
        {
            var list = new List<(ulong? channelId, ulong? messageId)>();

            using (var conn = new MySqlConnection(Program.connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT channelId, messageId FROM listembeds";

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ulong? channelId = reader.GetUInt64(0);
                            ulong? messageId = reader.GetUInt64(1);
                            list.Add((channelId, messageId));
                        }
                    }
                }
            }

            return list;
        }


        public static async Task SetListMessageId(ulong channelId,ulong messageId)
        {
            using (var conn = new MySqlConnection(Program.connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO listembeds (channelId, messageId) VALUES (@channelId, @messageId) ON DUPLICATE KEY UPDATE messageId=@messageId";
                    cmd.Parameters.AddWithValue("@channelId", channelId.ToString());
                    cmd.Parameters.AddWithValue("@messageId", messageId.ToString());

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task RemoveListEmbedByChannelId(ulong channelId)
        {
            using (var conn = new MySqlConnection(Program.connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "DELETE FROM listembeds WHERE channelId = @channelId";
                    cmd.Parameters.AddWithValue("@channelId", channelId);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }


    }
}
