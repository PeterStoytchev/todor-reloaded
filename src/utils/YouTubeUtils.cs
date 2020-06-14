using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace todor_reloaded
{
    public class YouTubeUtils
    {
        private YouTubeService youtubeService { get; }

        public YouTubeUtils()
        {
            youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = global.botConfig.youtubeApiKey,
                ApplicationName = "todor-bot-youtube"
            });
        }

        public async Task<PlaylistItemListResponse> GetPlaylistVideos(string link, int maxVideos)
        {
            var request = youtubeService.PlaylistItems.List("snippet");

            request.PlaylistId = link;

            request.MaxResults = maxVideos;

            var response = await request.ExecuteAsync();

            return response;
        }

        public async Task<Video> GetVideoDetails(string link)
        {
            var videoRequest = youtubeService.Videos.List("snippet");

            videoRequest.Id = utils.ExtractYoutubeId(link);

            videoRequest.RegionCode = "BG";

            videoRequest.MaxResults = 1;

            var response = await videoRequest.ExecuteAsync();

            return response.Items.First();
        }

        public async Task<SearchResult> SearchForVideo(string query)
        {
            var searchListRequest = youtubeService.Search.List("snippet");
            
            searchListRequest.Q = query;

            searchListRequest.RegionCode = "BG";

            searchListRequest.Type = "video";

            searchListRequest.MaxResults = 1;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();

            return searchListResponse.Items.First();
        }
    }
}
