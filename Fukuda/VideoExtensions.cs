using DSharpPlus.Entities;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;

namespace Fukuda;

public static class VideoExtensions
{
    public static DiscordEmbedBuilder PopulateEmbed(this Video video, DiscordEmbedBuilder embed)
    {
        return embed.AddField("Title", video.Title)
                    .AddField("Artist", video.Author.ChannelTitle, true)
                    .AddField("Length", video.Duration.ToString(), true)
                    .WithThumbnail(video.FindOptimalThumbnail().Url);
    }

    public static Thumbnail FindOptimalThumbnail(this Video video)
    {
        var thumbs = video.Thumbnails;
        var square = thumbs.Where(x => x.Resolution.Width == x.Resolution.Height).ToArray();
        return square.Length != 0 ? square.GetWithHighestResolution() : thumbs.GetWithHighestResolution();
    }
}
