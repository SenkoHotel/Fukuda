using DSharpPlus;
using DSharpPlus.Entities;
using HotelLib;
using HotelLib.Commands;
using HotelLib.Logging;
using HotelLib.Utils;

namespace Fukuda.Commands;

public class SearchCommand : SlashCommand
{
    public override string Name => "search";
    public override string Description => "search for metadata";

    public override List<SlashOption> Options => new()
    {
        new SlashOption("id", "the id of the youtube video", ApplicationCommandOptionType.String, true)
    };

    public override async Task Handle(HotelBot bot, DiscordInteraction interaction)
    {
        try
        {
            var id = interaction.GetString("id")!;

            var embed = new DiscordEmbedBuilder
            {
                Title = "Metadata Search",
                Description = "Checking...",
                Color = bot.AccentColor
            };

            await interaction.ReplyEmbed(embed);

            if (PlaylistManager.IsMissingMetadata(id))
            {
                await interaction.UpdateEmbed(embed.WithDescription("Fetching metadata..."));

                if (!await PlaylistManager.DownloadMetadata(id))
                    return;
            }

            var meta = PlaylistManager.ReadMetadata(id);

            if (meta is null)
            {
                // error("Failed to get metadata! :<");
                return;
            }

            embed = meta.PopulateEmbed(embed);
            await interaction.UpdateEmbed(embed.WithDescription(""));
        }
        catch (Exception e)
        {
            Logger.Log(e);
        }
    }
}
