using System.Diagnostics;
using DSharpPlus.VoiceNext;
using HotelLib.Logging;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace Fukuda;

public class PlaylistManager
{
    private bool started;
    private CancellationTokenSource? tokenSource;
    private VoiceTransmitSink? transmit;

    private YoutubeClient youtube { get; }
    private Queue<QueueInfo> queue { get; } = new();

    public IReadOnlyList<QueueInfo> Queue => queue.ToArray();

    public PlaylistManager()
    {
        youtube = new YoutubeClient();
    }

    public void EnsureStart()
    {
        if (started)
            return;

        started = true;

        var thread = new Thread(async void () =>
        {
            while (started)
            {
                try
                {
                    tokenSource ??= new CancellationTokenSource();

                    if (queue.Count == 0)
                        continue;

                    var current = queue.Peek();

                    using var stream = await youtube.Videos.Streams.GetAsync(current.Stream);
                    using var ffmpeg = prepareOutput();

                    if (ffmpeg is null || transmit is null)
                        return;

                    // ReSharper disable AccessToDisposedClosure
                    var inStream = ffmpeg.StandardInput.BaseStream;
                    var outStream = ffmpeg.StandardOutput.BaseStream;

                    var token = tokenSource!.Token;
                    token.Register(() => ffmpeg.Kill());

                    using var write = Task.Run(async () => await stream.CopyToAsync(inStream, cancellationToken: token), cancellationToken: token);
                    await outStream.CopyToAsync(transmit, cancellationToken: token);
                    // ReSharper restore AccessToDisposedClosure

                    queue.Dequeue();
                }
                catch (Exception e)
                {
                    Logger.Log(e);
                }
            }
        });

        thread.Start();
    }

    public void RegisterSink(VoiceTransmitSink sink) => transmit = sink;

    private string createFileName(string id, string ext = "mp3") => $"cache/{id}.{ext}";

    public bool IsMissingMetadata(string id) => !File.Exists(createFileName(id, "info.json"));
    public bool NeedsDownload(string id) => !File.Exists(createFileName(id, "audio"));

    public async Task<Video?> FetchMetadata(string id)
    {
        try
        {
            var video = await youtube.Videos.GetAsync(id);
            return video;
        }
        catch
        {
            return null;
        }
    }

    public async Task<IStreamInfo?> FindStream(Video video)
    {
        Directory.CreateDirectory("cache");

        try
        {
            var manifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
            var audioOnly = manifest.GetAudioOnlyStreams();
            var audio = audioOnly.GetWithHighestBitrate();

            foreach (var streamInfo in audioOnly)
                Logger.Log($"{streamInfo.Container} {streamInfo.Bitrate} {streamInfo.AudioCodec} {streamInfo.Size}");

            return audio;
        }
        catch
        {
            return null;
        }
    }

    public int QueueSong(Video video, IStreamInfo stream, string requester)
    {
        EnsureStart();

        var count = queue.Count;
        queue.Enqueue(new QueueInfo(video, stream, requester));
        return ++count;
    }

    public void StopAll()
    {
        tokenSource?.Cancel();
        tokenSource = null;
        queue.Clear();
    }

    public void FullStop()
    {
        StopAll();
        started = false;
        transmit = null;
    }

    private static Process? prepareOutput()
    {
        var ffmpeg = Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        });

        return ffmpeg;
    }
}
