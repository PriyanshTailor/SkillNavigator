using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;

namespace SkillScape.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MentorsController : ControllerBase
{
    private readonly IMentorService _mentorService;

    public MentorsController(IMentorService mentorService)
    {
        _mentorService = mentorService;
    }

    /// <summary>
    /// Get all available mentors
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<MentorDto>>>> GetAllMentors()
    {
        try
        {
            var result = await _mentorService.GetAllMentorsAsync();
            return Ok(ApiResponse<List<MentorDto>>.SuccessResponse(result, "Mentors retrieved"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<MentorDto>>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get mentor by ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<MentorDto>>> GetMentorById(string id)
    {
        try
        {
            var result = await _mentorService.GetMentorByIdAsync(id);
            return Ok(ApiResponse<MentorDto>.SuccessResponse(result, "Mentor retrieved"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<MentorDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<MentorDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Request a mentor session
    /// </summary>
    [HttpPost("request")]
    public async Task<ActionResult<ApiResponse<MentorRequestDto>>> CreateMentorRequest([FromBody] CreateMentorRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<MentorRequestDto>.ErrorResponse("User not authenticated"));

            var result = await _mentorService.CreateMentorRequestAsync(userId, request);
            return Ok(ApiResponse<MentorRequestDto>.SuccessResponse(result, "Mentor request created"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MentorRequestDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<MentorRequestDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get pending mentor requests for current mentor
    /// </summary>
    [HttpGet("requests/pending")]
    public async Task<ActionResult<ApiResponse<List<MentorRequestDto>>>> GetPendingRequests()
    {
        try
        {
            var mentorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(mentorId))
                return Unauthorized(ApiResponse<List<MentorRequestDto>>.ErrorResponse("User not authenticated"));

            var result = await _mentorService.GetPendingRequestsAsync(mentorId);
            return Ok(ApiResponse<List<MentorRequestDto>>.SuccessResponse(result, "Pending requests retrieved"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<MentorRequestDto>>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Accept or reject mentor request
    /// </summary>
    [HttpPatch("requests/{requestId}")]
    public async Task<ActionResult<ApiResponse<MentorRequestDto>>> UpdateRequestStatus(string requestId, [FromBody] UpdateMentorRequestStatusDto request)
    {
        try
        {
            var result = await _mentorService.UpdateRequestStatusAsync(requestId, request);
            return Ok(ApiResponse<MentorRequestDto>.SuccessResponse(result, "Request status updated"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<MentorRequestDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<MentorRequestDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Apply to become a mentor
    /// </summary>
    [HttpPost("apply")]
    public async Task<ActionResult<ApiResponse<MentorDto>>> ApplyAsMentor([FromBody] ApplyMentorDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<MentorDto>.ErrorResponse("User not authenticated"));

            var result = await _mentorService.ApplyAsMentorAsync(userId, request);
            return Ok(ApiResponse<MentorDto>.SuccessResponse(result, "Successfully registered as a mentor"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MentorDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<MentorDto>.ErrorResponse(ex.Message));
        }
    }
}
