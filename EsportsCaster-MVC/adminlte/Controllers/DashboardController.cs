using adminlte.Models;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;

namespace adminlte.Controllers
{
    public class DashboardController : Controller
    {
        private YouTubeService youtubeService;
        // GET: Dashboard
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Dashboardv1()
        {
            return View();
        }

        public ActionResult Dashboardv2()
        {

            return View();
        }
        private void InitialyoutubeService()
        {
            string apiKey = ConfigurationManager.AppSettings["YoutubeApiKey"];
            youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                ApplicationName = this.GetType().ToString()
            });
        }
        public ActionResult GetVideoLists()
        {
            InitialyoutubeService();
            var lists = GetVideos("lol competition");
            return Json(lists, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Watch()
        {
           
            var video = new adminlte.Models.VideoModel
            {
                VideoUrl = "/Content/videos/sample.mp4",
                SubtitleUrl = "/Content/videos/sample.vtt"
            };
            return View(video);
        }
        public List<VideoModel> GetVideos(string query, int maxResults = 10)
        {
            List<VideoModel> videos = new List<VideoModel>();
            string videoFolder = Server.MapPath("/Content/videos/");
            string[] videoFiles = Directory.GetFiles(videoFolder, "*.mp4");
            foreach (var videoFile in videoFiles)
            {
                string videoId = Guid.NewGuid().ToString().Substring(0, 8);
                string title = Path.GetFileNameWithoutExtension(videoFile);
                string thumbnailUrl = string.Empty;
                string thumbnailPath = Path.Combine(videoFolder, $"{videoId}.jpg");

                videos.Add(new VideoModel
                {
                    Id = videos.Count,
                    VideoId = videoId,
                    Title = title,
                    Description = "Sample description",
                    ThumbnailUrl = thumbnailUrl,
                    VideoUrl = $"/Content/videos/{Path.GetFileName(videoFile)}",
                    SubtitleUrl = $"/Content/videos/{Path.GetFileNameWithoutExtension(videoFile)}.vtt"
                });
            }
            try
            {
                var searchListRequest = youtubeService.Search.List("snippet");
                searchListRequest.Q = query;
                searchListRequest.MaxResults = maxResults;
                var searchListResponse = searchListRequest.Execute();


                foreach (var searchResult in searchListResponse.Items)
                {
                    if (searchResult.Id.Kind == "youtube#video")
                    {
                        videos.Add(new VideoModel
                        {
                            Id = videos.Count,
                            VideoId = searchResult.Id.VideoId,
                            Title = searchResult.Snippet.Title,
                            Description = searchResult.Snippet.Description,
                            ThumbnailUrl = searchResult.Snippet.Thumbnails.Medium.Url,
                            VideoUrl = "https://www.youtube.com/watch?v=" + searchResult.Id.VideoId
                        }); 
                    }
                }
            }
            catch (Exception) { }
            return videos;
        }
        [HttpGet]
        public JsonResult GetSubtitles()
        {
            string subtitlePath = Server.MapPath("~/Content/videos/sample.vtt");
            string subtitleContent = System.IO.File.ReadAllText(subtitlePath);
            return Json(new { subtitleContent = subtitleContent }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult LoadSubtitle()
        {
            // You may need to adjust the path to the subtitle file
            string subtitleUrl = "/Content/videos/sample.vtt";
            return Json(new { success = true, subtitleUrl = subtitleUrl });
        }
    }
}