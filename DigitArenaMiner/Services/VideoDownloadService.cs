using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace DigitArenaBot.Services
{
    public class VideoDownloadService
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;

        private readonly string _rootPath = AppDomain.CurrentDomain.BaseDirectory;
        private readonly string _downloadPath;

        public VideoDownloadService(DiscordSocketClient client, InteractionService commands, IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;
            _downloadPath = Path.Combine(_rootPath, "Downloads");
            Directory.CreateDirectory(_downloadPath);
        }

        public async Task<string> DownloadVideo(string url)
        {
            var loweredUrl = url.ToLower();

            if (loweredUrl.Contains("youtube.com"))
            {
                return await DownloadYoutubeVideo(url);
            }else if (loweredUrl.Contains("instagram.com"))
            {
                return await DownloadInstagramVideo(url);
            }

            throw new Exception("Unknown URL");
        }

        private async Task<string> DownloadInstagramVideo(string url)
        {
            // https://www.instagram.com/reel/Cx8LPbPIFGo/?igshid=MzRlODBiNWFlZA== 
            var urlChunks = url.Split("/");
            var apiUrl =  string.Join("/",urlChunks.SkipLast(1).ToArray()) + "/?__a=1&__d=1";
 
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36");
            HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
            
            
            if (response.IsSuccessStatusCode)
            {
                // Read the JSON data from the response
                string json = await response.Content.ReadAsStringAsync();
    
                JObject jsonObject = JObject.Parse(json);
                
                string videoUrl = (string)jsonObject["graphql"]["shortcode_media"]["video_url"];
                
                
                Console.WriteLine("Stahuju MP4 - " + videoUrl);
                HttpResponseMessage videoResponse = await httpClient.GetAsync(videoUrl);
                var filePath = Path.Combine(_downloadPath, "test.mp4");
                if (videoResponse.IsSuccessStatusCode)
                {
                    using (Stream fileStream = await videoResponse.Content.ReadAsStreamAsync())
                    {
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            await fileStream.CopyToAsync(fs);
                        }
                    }

                    Console.WriteLine("MP4 file downloaded successfully!");
                    return filePath;
                }
            }
            else
            {
                throw new Exception();
            }
            httpClient.Dispose();

            throw new Exception();
        }

        public Task<FileStream> GetVideoStream(string path)
        {
            return Task.FromResult(File.OpenRead(path));
        }

        public Task DeleteVideo(string path)
        {
            //File.Delete(path);
            return Task.CompletedTask;
        }

        protected async Task<string> DownloadYoutubeVideo(string videoUrl)
        {            
            var youtubeClient = new YoutubeClient();
            var videoInfo = await youtubeClient.Videos.GetAsync(videoUrl);

            string videoTitle = videoInfo.Title;
            string videoId = videoInfo.Id;
            string savePath = Path.Combine(_downloadPath, $"{videoId}.mp4");

            var streamInfoSet = await youtubeClient.Videos.Streams.GetManifestAsync(videoUrl);
            var streamInfo = streamInfoSet.GetMuxedStreams().GetWithHighestVideoQuality();

            if (streamInfo != null)
            {
                Console.WriteLine($"Downloading '{videoTitle}'...");
                await youtubeClient.Videos.Streams.DownloadAsync(streamInfo, savePath);
                Console.WriteLine($"Video '{videoTitle}' downloaded to '{savePath}'.");
            }

            return savePath;
        }
    }
}