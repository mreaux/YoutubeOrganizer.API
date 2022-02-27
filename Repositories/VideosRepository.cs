using MongoDB.Bson;
using MongoDB.Driver;
using YoutubeOrganizer.Dtos;
using YoutubeOrganizer.Models;
using YoutubeOrganizer.Utilities;

namespace YoutubeOrganizer.Repositories;

public class VideosRepository : IVideosRepository
{
    private readonly IMongoCollection<Video> _videosCollection;

    private const string databaseName = "YoutubeOrganizer";
    private const string collectionName = "Videos";


    private readonly FilterDefinitionBuilder<Video> _filterBuilder = Builders<Video>.Filter;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<VideosRepository> _logger;
    public VideosRepository(IMongoClient mongoClient, IHttpClientFactory httpClientFactory, ILogger<VideosRepository> logger)
    {
        _logger = logger;
        IMongoDatabase db = mongoClient.GetDatabase(databaseName);
        _logger.LogInformation("Connected to Mongo");
        _videosCollection = db.GetCollection<Video>(collectionName);
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IEnumerable<Video>> GetAllVideosAsync()
    {
        try
        {
            var videos = await _videosCollection.FindAsync<Video>(new BsonDocument());
            return videos?.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<Video> GetVideoAsync(Guid id)
    {
        var filter = _filterBuilder.Eq(v => v.Id, id);
        return (await _videosCollection.FindAsync(filter)).SingleOrDefault();
    }

    public async Task<Video> CreateVideoAsync(CreateVideoDto dto)
    {
        if (!YoutubeHelper.IsValidYoutubeUrl(dto.VideoUrl))
            throw new ArgumentException($"{dto.VideoUrl} is not a YouTube video.");

        var (YoutubeResponse, ReformedUrl) = await YoutubeHelper.GetValuesFromYoutube(dto.VideoUrl, _httpClientFactory, _logger);

        if ((await _videosCollection.FindAsync(x => x.YoutubeId == YoutubeResponse.Items[0].Id))
            .ToList().Any())
        {
            throw new ArgumentException("Video already exists");
        }

        var vid = new Video
        {
            Id = Guid.NewGuid(),
            CreatedDate = DateTimeOffset.UtcNow,
            Category = dto.Category,
            Tags = dto.Tags,
            YoutubeId = YoutubeResponse.Items[0].Id,
            Title = YoutubeResponse.Items[0].Snippet.Title,
            ChannelName = YoutubeResponse.Items[0].Snippet.ChannelTitle,
            ChannelId = YoutubeResponse.Items[0].Snippet.ChannelId,
            PostedDate = YoutubeResponse.Items[0].Snippet.PublishedAt,
            SmallThumbnailHeight = YoutubeResponse.Items[0].Snippet.Thumbnails.Default.Height,
            SmallThumbnailWidth = YoutubeResponse.Items[0].Snippet.Thumbnails.Default.Width,
            SmallThumbnailUrl = YoutubeResponse.Items[0].Snippet.Thumbnails.Default.Url,

            ThumbnailHeight = YoutubeResponse.Items[0].Snippet.Thumbnails.Medium.Height,
            ThumbnailWidth = YoutubeResponse.Items[0].Snippet.Thumbnails.Medium.Width,
            ThumbnailUrl = YoutubeResponse.Items[0].Snippet.Thumbnails.Medium.Url,

            //            VideoUrl = youtubeResults.ReformedUrl
        };
        await _videosCollection.InsertOneAsync(vid);
        return vid;
    }

    public async Task UpdateVideoAsync(Guid id, UpdateVideoDto dto)
    {
        var filter = _filterBuilder.Eq(v => v.Id, id);
        var vid = (await _videosCollection.FindAsync(filter)).FirstOrDefault();
        if (vid is null)
            throw new ArgumentException("Video not found.");
        var updatedVideo = vid with
        {
            Category = dto.Category,
            Tags = dto.Tags
        };
        await _videosCollection.ReplaceOneAsync(filter, updatedVideo);
    }

    public async Task DeleteVideoAsync(Guid id)
    {
        var filter = _filterBuilder.Eq(v => v.Id, id);
        var vid = (await _videosCollection.FindAsync(filter)).FirstOrDefault();
        if (vid is null)
            throw new ArgumentException("Video not found.");
        _videosCollection.DeleteOne(filter);
    }

    public async Task ResetDatabase()
    {
        // wipe everything
        var filter = _filterBuilder.Empty;
        await _videosCollection.DeleteManyAsync(filter);
        // create & add starting data
        IList<CreateVideoDto> dtos = new List<CreateVideoDto>();
        AddData(dtos);
        foreach (var dto in dtos)
        {
            _ = await CreateVideoAsync(dto);
        }
    }

    private static void AddData(IList<CreateVideoDto> dtoList)
    {
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Music,
                VideoUrl = "https://www.youtube.com/watch?v=7KJjVMqNIgA"
            });
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Music,
                VideoUrl = "https://www.youtube.com/watch?v=pRu5wxl5frk"
            });
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Music,
                VideoUrl = "https://www.youtube.com/watch?v=M3T_xeoGES8"
            });
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Music,
                VideoUrl = "https://www.youtube.com/watch?v=lcWVL4B-4pI&feature=youtu.be"
            });
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Development,
                VideoUrl = "https://www.youtube.com/watch?v=_Hp_dI0DzY4&list=WL&index=1&t=2404s",
                Tags = new string[]
                {
                    "ui"
                }
            });
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Development,
                VideoUrl = "https://www.youtube.com/watch?v=au7066pUA9M&list=WL&index=2&t=38s",
                Tags = new string[]
                {
                    "ui"
                }
            });
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Development,
                VideoUrl = "https://www.youtube.com/watch?v=0yQxb0fCRGE&list=WL&index=3",
                Tags = new string[]
                {
                    "ui"
                }
            });
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Development,
                VideoUrl = "https://www.youtube.com/watch?v=y_MTsIQuNEo&list=WL&index=4",
                Tags = new string[]
                {
                    "ui"
                }
            });
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Development,
                VideoUrl = "https://www.youtube.com/watch?v=FJDVKeh7RJI&list=PLZHoZjt2GMjyjzeK6PBmz2RLlug5f4mI0&index=27",
                Tags = new string[]
                {
                    "typescript", "react"
                }
            });
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Development,
                VideoUrl = "https://www.youtube.com/watch?v=5wtnKulcquA&list=WL&index=8",
                Tags = new string[]
                {
                    "angular"
                }
            });
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Development,
                VideoUrl = "https://www.youtube.com/watch?v=RGtDqHK8XGo&list=WL&index=10",
                Tags = new string[]
                {
                    "ui", "color"
                }
            });
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Development,
                VideoUrl = "https://www.youtube.com/watch?v=yibJDhrVlYk&list=WL&index=11&t=163s",
                Tags = new string[]
                {
                    "ui", "color"
                }
            });
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Development,
                VideoUrl = "https://www.youtube.com/watch?v=roywYSEPSvc&list=WL&index=12",
                Tags = new string[]
                {
                    "ui", "sass"
                }
            });
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Development,
                VideoUrl = "https://www.youtube.com/watch?v=olE86OdKYQs&list=WL&index=13",
                Tags = new string[]
                {
                    "ui", "css"
                }
            });
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Development,
                VideoUrl = "https://www.youtube.com/watch?v=kDumlq8bL7Y&list=WL&index=16",
                Tags = new string[]
                {
                    "ui","css","flex"
                }
            });
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Pool,
                VideoUrl = "https://www.youtube.com/watch?v=qzjousgGLjU&list=WL&index=17&t=2026s"
            });
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Development,
                VideoUrl = "https://www.youtube.com/watch?v=aUbXGs7YTGo&list=WL&index=19",
                Tags = new string[]
                {
                    "c#"
                }
            });

        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Development,
                VideoUrl = "https://www.youtube.com/watch?v=qkJ9keBmQWo&list=WL&index=22&t=4s",
                Tags = new string[]
                {
                    "c#","entityFramework"
                }
            });
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Gaming,
                VideoUrl = "https://www.youtube.com/watch?v=HOehtx-dAOU&list=WL&index=26",
                Tags = new string[]
                {
                    "rimworld"
                }
            });
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Gaming,
                VideoUrl = "https://www.youtube.com/watch?v=vY5N7Rbcjrc&list=WL&index=29&t=148s",
                Tags = new string[]
                {
                    "crusaderKings"
                }
            });
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Miscellaneous,
                VideoUrl = "https://www.youtube.com/watch?v=tdQk4eFIw0M&list=WL&index=35"
            });

        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Miscellaneous,
                VideoUrl = "https://www.youtube.com/watch?v=0yPcJD7RVuY&list=WL&index=38"
            });
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Development,
                VideoUrl = "https://www.youtube.com/watch?v=Lk-uVEVGxOA&list=WL&index=45",
                Tags = new string[]
                {
                    "javascript", "node"
                }
            });
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Cooking,
                VideoUrl = "https://www.youtube.com/watch?v=5_NU8Fw5-g8&list=WL&index=53&t=680s",
                Tags = new string[]
                {
                    "noodles"
                }
            });
        dtoList.Add(
            new CreateVideoDto
            {
                Category = (int)VideoCategory.Cooking,
                VideoUrl = "https://www.youtube.com/watch?v=eY1FF6SEggk&list=WL&index=56&t=444s",
                Tags = new string[]
                {
                    "rice"
                }
            });

    }
}
