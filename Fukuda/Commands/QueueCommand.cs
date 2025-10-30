using DSharpPlus.Entities;
using HotelLib;
using HotelLib.Commands;
using HotelLib.Utils;

namespace Fukuda.Commands;

public class QueueCommand : SlashCommand
{
    public override string Name => "queue";
    public override string Description => "Lists all the songs in the current queue.";

    public override async Task Handle(HotelBot bot, DiscordInteraction interaction)
    {
        var playlist = Program.GetPlaylistForServer(interaction.GuildId!.Value);
        var q = playlist.Queue;

        var embed = new DiscordEmbedBuilder
        {
            Title = "Current Playlist",
            Color = bot.AccentColor
        };

        const int max = 10;

        for (var i = 0; i < Math.Min(q.Count, max); i++)
        {
            var item = q[i];

            embed = embed.AddField($"#{i + 1} queued by {item.Requester}", $"`{cropString($"[{item.Video.Duration}] {item.Video.Author.ChannelTitle} - {item.Video.Title}", 1024 - 2)}`");
        }

        var over = q.Count - max;

        if (over > 0)
            embed = embed.WithFooter($"+{over} more");

        await interaction.ReplyEmbed(embed);
    }

    private static string cropString(string str, int len) => str.Length > len ? str[..len] : str;
}
