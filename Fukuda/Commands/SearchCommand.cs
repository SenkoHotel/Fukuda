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
        new SlashOption("url", "the url of the youtube video", ApplicationCommandOptionType.String, true)
    };

    public override async Task Handle(HotelBot bot, DiscordInteraction interaction)
    {
        try
        {
            var playlist = Program.GetPlaylistForServer(interaction.GuildId!.Value);
            var url = interaction.GetString("url")!;

            var embed = new DiscordEmbedBuilder
            {
                Title = "Metadata Search",
                Description = "Checking...",
                Color = bot.AccentColor
            };

            await interaction.ReplyEmbed(embed);

            var video = await playlist.FetchMetadata(url);

            if (video is null)
            {
                await interaction.UpdateEmbed(embed.WithColor(new DiscordColor("#ff5555"))
                                                   .WithDescription("Failed to fetch metadata!"));
                return;
            }

            embed = video.PopulateEmbed(embed);
            await interaction.UpdateEmbed(embed.WithDescription(""));
        }
        catch (Exception e)
        {
            Logger.Log(e);
        }
    }
}
