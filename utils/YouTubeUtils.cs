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

        public async Task<SearchResult> SearchForVideo(string query)
        {
            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = HttpUtility.UrlEncode(query);

            searchListRequest.MaxResults = 1;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();

            return searchListResponse.Items.First();
        }
    }
}
