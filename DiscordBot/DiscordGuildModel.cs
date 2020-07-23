using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    class DiscordGuildModel
    {
        public DiscordGuildModel()
        {
            GuildId = default;
            WellcomeChannelId = default;
        }

        [Key]
        public ulong GuildId { get; set; }
        public ulong WellcomeChannelId { get; set; }
    }
}
