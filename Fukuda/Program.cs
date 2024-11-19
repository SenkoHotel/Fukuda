using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Fukuda.Commands;
using HotelLib;
using HotelLib.Commands;

namespace Fukuda;

internal static class Program
{
    public static Config Config { get; private set; } = null!;

    public static async Task Main()
    {
        Config = HotelBot.LoadConfig<Config>();
        var bot = new HotelBot(Config.Token, c => c.UseVoiceNext())
        {
            AccentColor = new DiscordColor("#bd7a67"),
            Commands = new List<SlashCommand>
            {
                new JoinChannelCommand(),
                new PlayCommand(),
                new QueueCommand(),
                new SearchCommand(),
                new StopCommand()
            }
        };

        await bot.Start();
    }
}
