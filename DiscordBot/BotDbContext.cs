using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    class BotDbContext : DbContext
    {
        public DbSet<DiscordGuildModel> discordGuilds { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql("server=127.0.0.1;UserId=Bot;Password=Botdb123;database=botdb;");
        }

        public DiscordGuildModel GetGuild(ulong GuildId)
        {
            foreach (DiscordGuildModel Guild in discordGuilds)
                if (Guild.GuildId == GuildId)
                    return Guild;

            return null;
        }

        public DiscordGuildModel GreateGuild(DiscordGuildModel discordGuild = null)
        {
            if (discordGuild == null)
                discordGuild = new DiscordGuildModel();

            Add(discordGuild);
            SaveChanges();
            return discordGuild;
        }
    }
}
