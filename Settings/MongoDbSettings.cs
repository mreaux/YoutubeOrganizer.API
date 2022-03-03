namespace YoutubeOrganizer.Settings;
public class MongoDbSettings
{
    public string Host { get; init; }
    public string Database { get; init; }
    public string UserName { get; init; }

    public string GetConnectionString(string password)
    {
        return
            $"mongodb+srv://{UserName}:{password}@{Host}/{Database}?retryWrites=true&w=majority";
    }
}
