using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Fukuda.Commands;
using HotelLib;
using HotelLib.Commands;

namespace Fukuda;

internal static class Program
{
    public static Dictionary<ulong, PlaylistManager> Guilds { get; } = new();

    public static async Task Main()
    {
        var bot = new HotelBot(c => c.UseVoiceNext())
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

        bot.Client.VoiceStateUpdated += (_, ev) =>
        {
            var vnext = bot.Client.GetVoiceNext();
            var conn = vnext.GetConnection(ev.Guild);

            if (conn is null)
                return Task.CompletedTask;

            if (conn.TargetChannel.Users.Count <= 1)
            {
                conn.Disconnect();
                var playlist = GetPlaylistForServer(ev.Guild.Id);
                playlist.FullStop();
            }

            return Task.CompletedTask;
        };

        await bot.Start();
    }

    public static PlaylistManager GetPlaylistForServer(ulong id)
    {
        if (Guilds.TryGetValue(id, out var server))
            return server;

        var playlist = new PlaylistManager();
        Guilds[id] = playlist;
        return playlist;
    }
}
