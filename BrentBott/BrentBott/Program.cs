using Discord;
using Discord.Commands;
using Discord.Audio;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrentBott
{
    class Program
    {
        static void Main(string[] args)
        {
            MyBot bot = new MyBot();
            
            /*MyBot.discord = new DiscordClient();

            MyBot.discord.MessageReceived += async (s, e) =>
            {
                if (!e.Message.IsAuthor)
                    await e.Channel.SendMessage(e.Message.Text);
            };*/
        }
    }
}
