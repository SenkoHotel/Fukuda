using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using HotelLib;
using HotelLib.Commands;
using HotelLib.Logging;
using HotelLib.Utils;

namespace Fukuda.Commands;

public class PlayCommand : SlashCommand
{
    public override string Name => "play";
    public override string Description => "Play a song!";

    public override List<SlashOption> Options => new()
    {
        new SlashOption("url", "the url of the youtube video", ApplicationCommandOptionType.String, true)
    };

    public override async Task Handle(HotelBot bot, DiscordInteraction interaction)
    {
        var embed = new DiscordEmbedBuilder
        {
            Title = "Adding to queue...",
            Description = "Loading Metadata...",
            Color = bot.AccentColor
        };

        try
        {
            var url = interaction.GetString("url")!;
            var vnext = bot.Client.GetVoiceNext();
            var connection = vnext.GetConnection(interaction.Guild);

            if (connection is null)
            {
                await interaction.Reply("Not connected to any channel! Please use /join first.", true);
                return;
            }

            await interaction.ReplyEmbed(embed);

            var playlist = Program.GetPlaylistForServer(interaction.GuildId!.Value);

            var video = await playlist.FetchMetadata(url);

            if (video is null)
            {
                error("Failed to get metadata! :<");
                return;
            }

            embed = video.PopulateEmbed(embed);

            await interaction.UpdateEmbed(embed.WithDescription("Searching audio stream..."));
            var stream = await playlist.FindStream(video);

            if (stream is null)
            {
                error("Failed to download audio! :<");
                return;
            }

            await interaction.UpdateEmbed(embed.WithDescription("Queueing..."));

            var idx = playlist.QueueSong(video, stream, interaction.User.Username);
            await interaction.UpdateEmbed(embed.WithTitle("Added to playlist!").WithDescription($"Position #{idx}"));
        }
        catch (Exception e)
        {
            Logger.Log(e);
        }

        void error(string message)
        {
            _ = interaction.UpdateEmbed(embed.WithTitle("Failed to add.")
                                             .WithColor(new DiscordColor("#ff5555"))
                                             .WithDescription(message));
        }
    }
}
