using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using YoutubeOrganizer.Repositories;
using YoutubeOrganizer.Settings;
using YoutubeOrganizer.Utilities;
using Serilog;
using AutoMapper;
using YoutubeOrganizer.Models;
using YoutubeOrganizer.Dtos;

var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    //.AddUserSecrets<Program>(optional: true)
    //.AddEnvironmentVariables()
    .Build();

builder.Logging.ClearProviders();

// Serilog setup
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .ReadFrom.Configuration(ctx.Configuration));

// AutoMapper setup
builder.Services.AddAutoMapper(typeof(Program).Assembly);

var mapperConfig = new MapperConfiguration(cfg => {
    cfg.CreateMap<Video, VideoDto>();
});
IMapper mapper = mapperConfig.CreateMapper();

builder.Services.AddSingleton(mapper);

// MongoDb setup
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    // serialize Guids and DateTimeOffsets as strings
    BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
    BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

    var settings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder
                //.WithOrigins("https://localhost:44351", "http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddSingleton<IVideosRepository, VideosRepository>();

builder.Services.AddHttpClient();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

try
{
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseCors(x => x
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
    //.AllowCredentials()
    );

    app.UseAuthorization();

    app.MapControllers();

    Log.Information("Starting web host");

    app.Run();

    return 0;
}
catch (Exception e)
{
    Log.Fatal(e, "Host terminated unexpectedly");

    return 1;
}
finally
{
    Log.CloseAndFlush();
}