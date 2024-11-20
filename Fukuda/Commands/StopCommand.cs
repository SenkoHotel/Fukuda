using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using HotelLib;
using HotelLib.Commands;
using HotelLib.Utils;

namespace Fukuda.Commands;

public class StopCommand : SlashCommand
{
    public override string Name => "stop";
    public override string Description => "Stops and clears the playlist.";

    public override async Task Handle(HotelBot bot, DiscordInteraction interaction)
    {
        var member = await interaction.Guild.GetMemberAsync(interaction.User.Id) ?? throw new ArgumentNullException();
        var vc = member.VoiceState?.Channel;

        var vnext = bot.Client.GetVoiceNext();
        var connection = vnext.GetConnection(interaction.Guild);

        if (connection is null)
        {
            await interaction.Reply("Not connected to any channel!", true);
            return;
        }

        if (vc is null || vc.Id != connection.TargetChannel.Id)
        {
            await interaction.Reply("You are not in the same channel!", true);
            return;
        }

        PlaylistManager.StopAll();
        await interaction.Reply("Stopped playback.");
    }
}
