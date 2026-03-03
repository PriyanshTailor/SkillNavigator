using SkillScape.Application.DTOs;

namespace SkillScape.Application.Interfaces;

/// <summary>
/// Mentor service interface
/// </summary>
public interface IMentorService
{
    Task<List<MentorDto>> GetAllMentorsAsync();
    Task<MentorDto> GetMentorByIdAsync(string mentorId);
    Task<MentorRequestDto> CreateMentorRequestAsync(string studentId, CreateMentorRequestDto request);
    Task<List<MentorRequestDto>> GetPendingRequestsAsync(string mentorId);
    Task<MentorRequestDto> UpdateRequestStatusAsync(string requestId, UpdateMentorRequestStatusDto request);
    Task<MentorDto> ApplyAsMentorAsync(string userId, ApplyMentorDto request);
}
