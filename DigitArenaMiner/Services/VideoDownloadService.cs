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

        public async Task Init()
        {
            await YoutubeDLSharp.Utils.DownloadYtDlp(_youtubeDdpPath);
            //await ExecuteUnixCommand("echo XDDDDDDDDDDDDDDDDDDDDDDDDd");
            await YoutubeDLSharp.Utils.DownloadFFmpeg(_FFmpegPath);
        }

        public async Task<string> ExecuteUnixCommand(string command)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = "/bin/bash";
            psi.Arguments = command;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            using var process = Process.Start(psi);

            process.WaitForExitAsync();

            var output = process.StandardOutput.ReadToEnd();

            return output;
        }

        public async Task<string> DownloadVideo(string url, ExampleCommands.VideoFormat format,  Func<string, string> onProgress = null)
        {
            var formatString = format == ExampleCommands.VideoFormat.Best ? "bestvideo+bestaudio/best" : "worstvideo+worstaudio/worst";
            
            var ytdl = CreateYoutubeDl();
            
            
            var data = await ytdl.RunVideoDataFetch(url);

            if (data.Data == null || data.Data.Duration == null) throw new Exception("Data o videu jsou null.");
            
            ytdl.OutputFolder = Path.Combine(_downloadPath, data.Data.ID);

            var res = await ytdl.RunVideoDownload(url, formatString, mergeFormat: DownloadMergeFormat.Mp4, overrideOptions: new OptionSet()
            {
                RestrictFilenames = true,
                WindowsFilenames = true,
                RecodeVideo = VideoRecodeFormat.Webm,
            }, progress: new Progress<DownloadProgress>((progress =>
            {
                var size = ByteSize.Parse(progress.TotalDownloadSize);
                var maxSize = ByteSize.Parse("25MB");
                if (size.Bytes > maxSize.Bytes)
                {
                    throw new Exception($"Video je větší než {maxSize.ToString()}");
                }
                
                var message = $"{progress.State}: {((int)(progress.Progress * 100)).ToString()}%";
                if (progress.State == DownloadState.Success) message = "Success";
                
                onProgress?.Invoke(message);
                Console.WriteLine(message);
            })));


            // var maxDiscordFileSize = ByteSize.Parse("22MB");
            //
            // var file = File.OpenRead(res.Data);
            // var fileSize = new ByteSize(file.Length);

            // if (fileSize > maxDiscordFileSize)
            // {
            // Console.WriteLine(fileSize.ToString());
            //     var a = await _catBox.UploadImage(new StreamUploadRequest()
            //     {
            //         Stream = file,
            //         FileName = Path.GetFileName(res.Data)
            //     });
            // }
            
            // Console.WriteLine("FileURL" + a);
            return res.Data;
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
            ytdl.YoutubeDLPath = Path.Combine(_youtubeDdpPath, "yt-dlp");
            ytdl.FFmpegPath = Path.Combine(_FFmpegPath, "ffmpeg");
            return ytdl;
        }
    }
}