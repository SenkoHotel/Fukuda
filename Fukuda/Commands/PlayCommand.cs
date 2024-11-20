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
        new SlashOption("id", "the id of the youtube video", ApplicationCommandOptionType.String, true)
    };

    public override async Task Handle(HotelBot bot, DiscordInteraction interaction)
    {
        var embed = new DiscordEmbedBuilder
        {
            Title = "Adding to queue...",
            Description = "Checking...",
            Color = bot.AccentColor
        };

        try
        {
            var id = interaction.GetString("id")!;

            var vnext = bot.Client.GetVoiceNext();
            var connection = vnext.GetConnection(interaction.Guild);

            if (connection is null)
            {
                await interaction.Reply("Not connected to any channel! Please use /join first.", true);
                return;
            }

            await interaction.ReplyEmbed(embed);

            if (PlaylistManager.IsMissingMetadata(id))
            {
                await interaction.UpdateEmbed(embed.WithDescription("Fetching metadata..."));

                if (!await PlaylistManager.DownloadMetadata(id))
                {
                    error("Failed to download metadata! :<");
                    return;
                }
            }

            var meta = PlaylistManager.ReadMetadata(id);

            if (meta is null)
            {
                error("Failed to get metadata! :<");
                return;
            }

            embed = meta.PopulateEmbed(embed);

            if (meta.DurationNum > 60 * 60)
            {
                error("Song is too long to queue.");
                return;
            }

            if (PlaylistManager.NeedsDownload(id))
            {
                await interaction.UpdateEmbed(embed.WithDescription("Downloading Audio..."));

                if (!await PlaylistManager.DownloadAudio(id))
                {
                    error("Failed to download audio! :<");
                    return;
                }
            }

            await interaction.UpdateEmbed(embed.WithDescription("Queueing..."));

            meta.RequestedBy = interaction.User.Username;
            var idx = PlaylistManager.QueueSong(meta);

            await interaction.UpdateEmbed(embed.WithTitle("Added to playlist!").WithDescription($"Position #{idx}"));
        }
        catch (Exception e)
        {
            Logger.Log(e);
        }

        void error(string message)
        {
            _ = interaction.UpdateEmbed(embed.WithTitle("Failed to add.").WithColor(new DiscordColor("#ff5555")).WithDescription(message));
        }
    }
}
