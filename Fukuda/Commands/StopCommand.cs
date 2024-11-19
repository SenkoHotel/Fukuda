using DSharpPlus.Entities;
using HotelLib;
using HotelLib.Commands;
using HotelLib.Utils;

namespace Fukuda.Commands;

public class StopCommand : SlashCommand
{
    public override string Name => "stop";
    public override string Description => "Stops and clears the playlist.";

    public override void Handle(HotelBot bot, DiscordInteraction interaction)
    {
        PlaylistManager.StopAll();
        _ = interaction.Reply("Stopped playback.");
    }
}
