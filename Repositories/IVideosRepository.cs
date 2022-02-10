using YoutubeOrganizer.Dtos;
using YoutubeOrganizer.Models;

namespace YoutubeOrganizer.Repositories;
public interface IVideosRepository
{
    Task<IEnumerable<Video>> GetAllVideosAsync();
    Task<Video> GetVideoAsync(Guid id);
    Task<Video> CreateVideoAsync(CreateVideoDto dto);
    Task UpdateVideoAsync(Guid id, UpdateVideoDto dto);
    Task DeleteVideoAsync(Guid id);
    Task ResetDatabase();

}
