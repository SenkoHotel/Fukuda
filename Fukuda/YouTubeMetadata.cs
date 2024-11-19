using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace Fukuda;

public class YouTubeMetadata
{
    [JsonProperty("id")]
    public string ID { get; set; } = string.Empty;

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty("artist")]
    public string Artist { get; set; } = string.Empty;

    [JsonProperty("uploader")]
    public string Uploader { get; set; } = string.Empty;

    [JsonProperty("duration_string")]
    public string Duration { get; set; } = string.Empty;

    [JsonProperty("duration")]
    public long DurationNum { get; set; }

    [JsonProperty("thumbnail")]
    public string Thumbnail { get; set; } = string.Empty;

    [JsonProperty("thumbnails")]
    public YouTubeThumbnail[]? Thumbnails { get; set; }

    [JsonIgnore]
    public string RequestedBy { get; set; } = "";

    [JsonIgnore]
    public string PreferredArtist => string.IsNullOrWhiteSpace(Artist) ? Uploader : Artist;

    [JsonIgnore]
    public string PreferredThumbnail
    {
        get
        {
            if (Thumbnails is null || Thumbnail.Length == 0)
                return Thumbnail;

            var square = Thumbnails.Where(x => x.Width == x.Height).ToList();

            if (square.Count != 0)
                return square.MaxBy(x => x.Width)?.Url ?? Thumbnail;

            return Thumbnails.MaxBy(x => x.Width)?.Url ?? Thumbnail;
        }
    }

    public DiscordEmbedBuilder PopulateEmbed(DiscordEmbedBuilder builder)
    {
        return builder.AddField("Title", Title)
                      .AddField("Artist", PreferredArtist, true)
                      .AddField("Length", Duration, true)
                      .WithThumbnail(PreferredThumbnail);
    }

    public class YouTubeThumbnail
    {
        [JsonProperty("url")]
        public string Url { get; set; } = "";

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }
}
