using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;


namespace YoutubeOrganizer.Utilities;

public static class YoutubeHelper
{
    private const string _apiPath = @"https://www.googleapis.com/youtube/v3/videos?part=snippet";
    private const string _baseVideoUrl = @"https://www.youtube.com/watch?v=";
    private const string _shortenedUrl = @"https://youtu.be/";
    private const string _fields = 
        @"&fields=items(id,snippet(title, channelId, publishedAt, channelTitle, thumbnails(default,medium)))";

    private const string _apiKey = "AIzaSyDD306c35igtu6XBdJLY_OzsRVzc - _Vk5M";

    private static bool IsNormalUrl(string videoUrl)
    {
        return videoUrl.ToUpper().Contains("YOUTUBE.COM");
    }

    public static bool IsShortenedUrl(string videoUrl)
    {
        return videoUrl.ToUpper().Contains("YOUTU.BE");
    }

    public static bool IsValidYoutubeUrl(string videoUrl)
    {
        Uri uriResult;
        return
            (videoUrl is not null) &&
            (Uri.TryCreate(videoUrl, UriKind.Absolute, out uriResult) &&
            (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)) &&
            ((IsNormalUrl(videoUrl) || IsShortenedUrl(videoUrl)));
    }

    public static string? GetYoutubeVideoId(string videoUrl)
    {
        var uri = new Uri(videoUrl);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        string? result;
        if (IsNormalUrl(videoUrl))
        {
            result = query.AllKeys.Contains("v") ? query["v"] : null;
        }
        else
        {
            result = uri.Segments.Last();
        }
        return result?.Length == 11 ? result : null;
    }

    public static async Task<(LaunchDto YoutubeResponse, string ReformedUrl)> GetValuesFromYoutube(string videoUrl, IHttpClientFactory httpClientFactory, ILogger logger)
    {
        var videoId = GetYoutubeVideoId(videoUrl);

        var reformedUrl = $"{_baseVideoUrl}{videoId}";
        logger.LogInformation(reformedUrl);
        var youtubeData = await GetDataFromYoutube(videoId, httpClientFactory, logger);
        return (YoutubeResponse: youtubeData, ReformedUrl: reformedUrl);
    }

    private static async Task<LaunchDto> GetDataFromYoutube(string youtubeId, IHttpClientFactory httpClientFactory, ILogger logger)
    {
        var httpRequestMessage = new HttpRequestMessage()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{_apiPath}&id={youtubeId}&key={_apiKey}{_fields}")
        };
        logger.LogInformation(httpRequestMessage.RequestUri.ToString());

        var httpClient = httpClientFactory.CreateClient();
        LaunchDto? youtubeDto = await httpClient.GetFromJsonAsync<LaunchDto>(httpRequestMessage.RequestUri, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return youtubeDto;
        /* *******  Working
                var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    string apiResponse = await httpResponseMessage.Content.ReadAsStringAsync();
                    using (JsonDocument document = JsonDocument.Parse(apiResponse))
                    {
                        JsonElement root = document.RootElement;
                        JsonElement itemsElement = root.GetProperty("items");
                        var x = itemsElement[0].GetProperty("snippet").GetProperty("title");
                        response = x.ToString();
                    }
                }
                return response;
        *******/
    }


}
