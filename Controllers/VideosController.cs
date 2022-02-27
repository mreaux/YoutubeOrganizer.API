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
    public async Task<ActionResult<IEnumerable<VideoDto>>> GetVideosAsync()
    {
        try
        {
            var vids = await _repo.GetAllVideosAsync();
            _logger.LogInformation("Get All Count: {videoCount}", vids.Count());
            var notNulls = vids.Where(x => x is not null);
            return Ok(_mapper.Map<IEnumerable<Video>, IEnumerable<VideoDto>>(notNulls));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetVideosAsync");
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VideoDto>> GetVideo(Guid id)
    {
        try
        {
            var vid = await _repo.GetVideoAsync(id);
            _logger.LogInformation("GetVideo Guid: {guid} Video Id: {vidId}", id, vid?.YoutubeId ?? "Not Found");
            return (vid is null) ?
                NotFound() :
                Ok(_mapper.Map<VideoDto>(vid));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetVideo");
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
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
        catch (TimeoutException timeoutException)
        {
            _logger.LogError(timeoutException, "Timeout Exception in CreateVideo");
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Argument Exception in CreateVideo");
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
            _logger.LogError(ex, "Exception in ResetAll");
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
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
        catch (TimeoutException timeoutException)
        {
            _logger.LogError(timeoutException, "Time Out Exception in UpdateVideoAsync");
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Exception in UpdateVideoAsync");
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteVideoAsync(Guid id)
    {
        try
        {
            var vid = await _repo.GetVideoAsync(id);
            if (vid is null)
                return NotFound();

            await _repo.DeleteVideoAsync(id);
            return NoContent();
        }
        catch (TimeoutException timeoutException)
        {
            _logger.LogError(timeoutException, "Timeout Exception in DeleteVideoAsync");
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    }

}

