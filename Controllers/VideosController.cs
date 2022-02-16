using Microsoft.AspNetCore.Mvc;
using YoutubeOrganizer.Dtos;
using YoutubeOrganizer.Models;
using YoutubeOrganizer.Repositories;
using AutoMapper;

namespace YoutubeOrganizer.Controllers;

[ApiController]
[Route("Videos")]
public class VideosController : ControllerBase
{
    private readonly IVideosRepository _repo;

    private readonly ILogger<VideosController> _logger;
    private readonly IMapper _mapper;

    public VideosController(IVideosRepository repo, ILogger<VideosController> logger, IMapper mapper)
    {
        _logger = logger;
        _mapper = mapper;
        _repo = repo;
    }

    [HttpGet]
    public async Task<IEnumerable<VideoDto>> GetVideosAsync()
    {
        var vids = await _repo.GetAllVideosAsync();
        _logger.LogInformation("Get All Count: {videoCount}", vids.Count());
        var notNulls = vids.Where(x => x is not null);
        return _mapper.Map<IEnumerable<Video>, IEnumerable<VideoDto>>(notNulls);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VideoDto>> GetVideo(Guid id)
    {
        try
        {
            var vid = await _repo.GetVideoAsync(id);
            if (vid == null)
            {
                _logger.LogInformation("GetVideo Guid: {guid} Video not found.", id);
            }
            else
            {
                _logger.LogInformation("GetVideo Guid: {guid} Video Id: {vidId}", id, vid.YoutubeId);
            }

            return (vid is null) ?
                NotFound() :
                Ok(_mapper.Map<VideoDto>(vid));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    public async Task<ActionResult<VideoDto>> CreateVideo(CreateVideoDto videoDto)
    {
        Video? vid;
        try
        {
            vid = await _repo.CreateVideoAsync(videoDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
        _logger.LogInformation("CreateVideo url: {url}", videoDto.VideoUrl);
        return CreatedAtAction(nameof(GetVideo), new { Controller = "Videos", id = vid.Id }, vid);
    }

    [HttpPost("/reset")]
    public async Task<ActionResult> ResetAll()
    {
        try
        {
            await _repo.ResetDatabase();
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }
        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult> UpdateVideoAsync(Guid id, UpdateVideoDto videoDto)
    {
        try
        {
            _logger.LogInformation("Guid {x}", id);
            await _repo.UpdateVideoAsync(id, videoDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex.Message);
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteVideoAsync(Guid id)
    {
        var vid = await _repo.GetVideoAsync(id);
        if (vid is null)
            return NotFound();

        await _repo.DeleteVideoAsync(id);
        return NoContent();
    }

}

