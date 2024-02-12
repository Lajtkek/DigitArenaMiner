using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ByteSizeLib;
using CatBox.NET.Client;
using CatBox.NET.Requests;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace DigitArenaBot.Services
{
    public class VideoDownloadService
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;
        private readonly IConfigurationRoot _config;

        private readonly string _rootPath = AppDomain.CurrentDomain.BaseDirectory;
        private readonly string _downloadPath;

        private readonly string _youtubeDdpPath;
        private readonly string _FFmpegPath;

        private readonly float _maxVideoLengthSeconds;

        // private readonly ICatBoxClient _catBox;

        public VideoDownloadService(DiscordSocketClient client, InteractionService commands, IServiceProvider services, IConfigurationRoot config)
        {
            _client = client;
            _commands = commands;
            _services = services;
            _config = config;
            _downloadPath = Path.Combine(_rootPath, "Downloads");
            Directory.CreateDirectory(_downloadPath);

            _youtubeDdpPath = Path.Combine(_downloadPath, "YTDLP");
            _FFmpegPath = Path.Combine(_downloadPath, "FFMPEG");
            
            Directory.CreateDirectory(_youtubeDdpPath);
            Directory.CreateDirectory(_FFmpegPath);
            
            _maxVideoLengthSeconds = _config.GetSection("MaxVideoDuration").Get<float>();
            
        }

        public async Task<string> DownloadVideo(string url,  Func<string, string> onProgress = null, bool ignoreFormat = false)
        {
            var ytdl = CreateYoutubeDl();
            
            ytdl.OutputFolder = Path.Combine(_downloadPath, "TempVideoFolder");

            var tokenSource = new CancellationTokenSource();

            var optionSet = new OptionSet()
            {
                RestrictFilenames = true,
                WindowsFilenames = true,
                ConcurrentFragments = 4,
                NoRestrictFilenames = false,
                TrimFilenames = 16
            };

            if (!ignoreFormat)
            {
                optionSet.FormatSort = "vcodec:h264,size:25M";
                optionSet.Format = "b[ext=mp4]";
            }
            
            Console.WriteLine(ytdl.OutputFolder);
            
            try
            {
                var lastProgress = new DownloadProgress(DownloadState.None);
                var timer = new Timer(state =>
                {
                    onProgress?.Invoke($"{lastProgress.State}: {lastProgress.Progress} (eta:{lastProgress.ETA})");
                }, null, 0, 3000);
                
                var res = await ytdl.RunVideoDownload(url, overrideOptions: optionSet,
                    ct: tokenSource.Token, progress: new Progress<DownloadProgress>((progress =>
                    {
                        lastProgress = progress;
                        var downloaded = string.IsNullOrWhiteSpace(progress.TotalDownloadSize)
                            ? "0B"
                            : progress.TotalDownloadSize;
                        var size = ByteSize.Parse(downloaded);
                        var maxSize = ByteSize.Parse("25MB");
                        if (size.Bytes > maxSize.Bytes)
                        {
                            tokenSource.Cancel();
                        }

                        var message = $"{progress.State}: {((int)(progress.Progress * 100)).ToString()}%";
                        if (progress.State == DownloadState.Success) message = "Success";

                        Console.WriteLine(message);
                    })));

                await timer.DisposeAsync();

                if (string.IsNullOrWhiteSpace(res.Data))
                {
                    if (ignoreFormat)
                    {
                        if (url.Contains("instagram"))
                            throw new Exception(
                                $"Video se nepodařilo stáhnout, ALE zkusíme toto: {url.Replace("instagram", "ddinstagram")}");
                        throw new Exception("NEJDE TO a neni to instagram");
                    }

                    return await DownloadVideo(url, onProgress, true);
                }
                
                return res.Data;
            }
            catch (Exception e)
            {
                if (tokenSource.Token.IsCancellationRequested) throw new Exception("Video bylo větší než 25MB");
                throw e;
            }
        }

        public Task<FileStream> GetVideoStream(string path)
        {
            return Task.FromResult(File.OpenRead(path));
        }

        public Task DeleteVideo(string path)
        {
            var dir = Path.GetDirectoryName(path);
            Directory.Delete(dir, true);
            return Task.CompletedTask;
        }
        
        protected YoutubeDL CreateYoutubeDl()
        {
            var ytdl = new YoutubeDL();
            ytdl.YoutubeDLPath = "yt-dlp";
            ytdl.FFmpegPath = "ffmpeg";
            return ytdl;
        }
    }
}