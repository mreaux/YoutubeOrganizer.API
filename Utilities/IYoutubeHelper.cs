
namespace YoutubeOrganizer.Utilities;

public interface IYoutubeHelper
{
    Task<(LaunchDto YoutubeResponse, string ReformedUrl)> GetValuesFromYoutube(string videoUrl);
    bool IsValidYoutubeUrl(string videoUrl);
}
