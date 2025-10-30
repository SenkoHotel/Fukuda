using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using HotelLib;
using HotelLib.Commands;
using HotelLib.Logging;
using HotelLib.Utils;

namespace Fukuda.Commands;

public class JoinChannelCommand : SlashCommand
{
    public override string Name => "join";
    public override string Description => "Joins the channel you are currently in.";

    public override async Task Handle(HotelBot bot, DiscordInteraction interaction)
    {
        try
        {
            var member = await interaction.Guild.GetMemberAsync(interaction.User.Id) ?? throw new ArgumentNullException();
            var vc = member.VoiceState?.Channel;

            if (vc is null)
            {
                await interaction.Reply("Please join a voice channel before using this!", true);
                return;
            }

            var vnext = bot.Client.GetVoiceNext();
            var connection = vnext.GetConnection(interaction.Guild);

            if (connection is not null)
            {
                await interaction.Reply("I am already connected to a voice channel!", true);
                return;
            }

            await vc.ConnectAsync();

            connection = vnext.GetConnection(interaction.Guild);
            var playlist = Program.GetPlaylistForServer(interaction.GuildId!.Value);

            var transmit = connection.GetTransmitSink();
            transmit.VolumeModifier = 0.1;
            playlist.RegisterSink(transmit);

            await interaction.Reply($"Joined {vc.Mention}!");
        }
        catch (Exception e)
        {
            Logger.Log(e, "Failed to connect to voice channel!");
            await interaction.Reply("Oh no... something went wrong when connecting.");
        }
    }
}
