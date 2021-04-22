using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CognitiveSearch.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoController : ControllerBase
    {
        private readonly ILogger<VideoController> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _location;
        private readonly string _accountId;
        private readonly string _accountKey;

        public VideoController(ILogger<VideoController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("VideoIndexer");
            _location = configuration["VideoIndexerLocation"];
            _accountId = configuration["VideoIndexerAccountId"];
            _accountKey = configuration["VideoIndexerAccountKey"];
        }

        /// <summary>
        /// Looks for the summary thumbnail in the insights for the video. An optimisation would be to cache this when we have indexed the video so we can return it from blob storage.
        /// </summary>
        [Route("{videoId}/thumbnail/{thumbnailId}")]
        public async Task<IActionResult> ThumbnailImage(string videoId, string thumbnailId)
        {
            _logger.LogInformation("Generating Thumbnail Uri for image {VideoId}", videoId);
            
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.videoindexer.ai/auth/{_location}/Accounts/{_accountId}/AccessToken?allowEdit=false");
            request.Headers.Add("Ocp-Apim-Subscription-Key", _accountKey);
            var accessToken = JsonConvert.DeserializeObject<string>(await (await _httpClient.SendAsync(request)).Content.ReadAsStringAsync());

            return base.File(
                await _httpClient.GetByteArrayAsync($"https://api.videoindexer.ai/{_location}/Accounts/{_accountId}/Videos/{videoId}/Thumbnails/{thumbnailId}?format=Jpeg&accessToken={accessToken}"),
                "image/jpeg");

        }

        [Route("{videoId}/insights")]
        public async Task<IActionResult> VideoInsights(string videoId)
        {
            _logger.LogInformation("Generating Thumbnail Uri for image {VideoId}", videoId);

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.videoindexer.ai/auth/{_location}/Accounts/{_accountId}/Videos/{videoId}/AccessToken?allowEdit=false");
            request.Headers.Add("Ocp-Apim-Subscription-Key", _accountKey);
            var accessToken = JsonConvert.DeserializeObject<string>(await (await _httpClient.SendAsync(request)).Content.ReadAsStringAsync());

            //Warning - leaks an access token back to the client so we can embed the insights widget. The token is scoped to the one video.
            return Redirect($"https://api.videoindexer.ai/{_location}/Accounts/{_accountId}/Videos/{videoId}/InsightsWidget?widgetType=People&widgetType=Sentiments&widgetType=Keywords&widgetType=Search&accessToken={accessToken}");
        }

        [Route("{videoId}/player")]
        public async Task<IActionResult> VideoPlayer(string videoId)
        {
            _logger.LogInformation("Generating Thumbnail Uri for image {VideoId}", videoId);

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.videoindexer.ai/auth/{_location}/Accounts/{_accountId}/Videos/{videoId}/AccessToken?allowEdit=false");
            request.Headers.Add("Ocp-Apim-Subscription-Key", _accountKey);
            var accessToken = JsonConvert.DeserializeObject<string>(await (await _httpClient.SendAsync(request)).Content.ReadAsStringAsync());
            
            //Warning - leaks an access token back to the client so we can embed the insights widget. The token is scoped to the one video.
            return Redirect($"https://api.videoindexer.ai/{_location}/Accounts/{_accountId}/Videos/{videoId}/PlayerWidget?accessToken={accessToken}");
        }

    }
}