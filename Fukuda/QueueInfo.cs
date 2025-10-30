using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace Fukuda;

public class QueueInfo
{
    public Video Video { get; }
    public IStreamInfo Stream { get; }
    public string Requester { get; }

    public QueueInfo(Video video, IStreamInfo stream, string requester)
    {
        Video = video;
        Stream = stream;
        Requester = requester;
    }
}
