using System.Diagnostics;
using DSharpPlus.VoiceNext;
using HotelLib.Logging;
using Newtonsoft.Json;

namespace Fukuda;

public static class PlaylistManager
{
    private static bool started;
    private static CancellationTokenSource? tokenSource;
    private static VoiceTransmitSink? transmit;

    private static Queue<YouTubeMetadata> queue { get; } = new();

    public static IReadOnlyList<YouTubeMetadata> Queue => queue.ToList();

    public static void EnsureStart()
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
                    var path = createFileName(current.ID);

                    using var pcm = readAsPcm(path);

                    if (pcm is null || transmit is null)
                        return;

                    await pcm.CopyToAsync(transmit, cancellationToken: tokenSource!.Token);

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

    public static void RegisterSink(VoiceTransmitSink sink) => transmit = sink;

    private static string createFileName(string id, string ext = "mp3") => $"cache/{id}.{ext}";

    public static bool IsMissingMetadata(string id) => !File.Exists(createFileName(id, "info.json"));
    public static bool NeedsDownload(string id) => !File.Exists(createFileName(id));

    public static YouTubeMetadata? ReadMetadata(string id)
    {
        var name = createFileName(id, "info.json");

        if (!File.Exists(name))
            return null;

        var json = File.ReadAllText(name);
        return JsonConvert.DeserializeObject<YouTubeMetadata>(json);
    }

    public static async Task<bool> DownloadMetadata(string id)
    {
        Directory.CreateDirectory("cache");

        var ytdlp = Process.Start(new ProcessStartInfo
        {
            FileName = "yt-dlp",
            Arguments = $"https://music.youtube.com/watch?v={id} --write-info-json --skip-download --cookies \"{Program.Config.YouTubeCookies}\" -o \"cache/{id}.%(ext)s\"",
            RedirectStandardOutput = true,
            RedirectStandardError = false,
            UseShellExecute = false
        });

        if (ytdlp is null)
            return false;

        await ytdlp.WaitForExitAsync();
        return true;
    }

    public static async Task<bool> DownloadAudio(string id)
    {
        Directory.CreateDirectory("cache");

        var ytdlp = Process.Start(new ProcessStartInfo
        {
            FileName = "yt-dlp",
            Arguments = $"-x --audio-format mp3 https://www.youtube.com/watch?v={id} -o \"{createFileName(id)}\" --cookies {Program.Config.YouTubeCookies} --ffmpeg-location {Program.Config.FFmpeg}",
            RedirectStandardOutput = true,
            RedirectStandardError = false,
            UseShellExecute = false
        });

        if (ytdlp is null)
            return false;

        await ytdlp.WaitForExitAsync();
        return true;
    }

    public static int QueueSong(YouTubeMetadata meta)
    {
        EnsureStart();

        var count = queue.Count;
        queue.Enqueue(meta);
        return ++count;
    }

    public static void StopAll()
    {
        tokenSource?.Cancel();
        tokenSource = null;
        queue.Clear();
    }

    private static Stream? readAsPcm(string path)
    {
        var ffmpeg = Process.Start(new ProcessStartInfo
        {
            FileName = Program.Config.FFmpeg,
            Arguments = $"""-i "{path}" -ac 2 -f s16le -ar 48000 pipe:1""",
            RedirectStandardOutput = true,
            RedirectStandardError = false,
            UseShellExecute = false
        });

        return ffmpeg?.StandardOutput.BaseStream;
    }
}
