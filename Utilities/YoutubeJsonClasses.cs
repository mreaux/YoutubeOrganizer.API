namespace YoutubeOrganizer.Utilities;

public class LaunchDto
{
    public string Kind { get; set; }
    public string Etag { get; set; }
    public Item[] Items { get; set; }
}

public class Item
{
    public string Kind { get; set; }
    public string Id { get; set; }
    public Snippet Snippet { get; set; }
}

public class Snippet
{
    public DateTimeOffset PublishedAt { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public Thumbnails Thumbnails { get; set; }
    public string ChannelTitle { get; set; }
    public string ChannelId { get; set; }
}

public class Thumbnails
{
    public ThumbnailBase Default { get; set; }
    public ThumbnailBase Medium { get; set; }
    /* unused Thumbnails
     * 
    public ThumbnailBase High { get; set; }
    public ThumbnailBase Standard { get; set; }
    public ThumbnailBase Maxres { get; set; }
    */
}

public class ThumbnailBase
{
    public Uri Url { get; set; }
    public long Width { get; set; }
    public long Height { get; set; }
}

