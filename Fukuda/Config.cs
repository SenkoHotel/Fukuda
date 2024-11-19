using JetBrains.Annotations;

namespace Fukuda;

[UsedImplicitly]
public class Config
{
    public string Token { get; set; } = "token-here";
    public string FFmpeg { get; set; } = "ffmpeg";
    public string YouTubeCookies { get; set; } = "cookies.txt";
}
