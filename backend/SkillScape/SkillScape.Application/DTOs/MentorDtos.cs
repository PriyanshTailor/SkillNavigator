namespace SkillScape.Application.DTOs;

/// <summary>
/// Mentor DTO
/// </summary>
public class MentorDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Expertise { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public int YearsOfExperience { get; set; }
    public decimal HourlyRate { get; set; }
    public bool IsAvailable { get; set; }
    public int TotalSessionCount { get; set; }
    public double AvgRating { get; set; }
}

/// <summary>
/// Mentor request DTO
/// </summary>
public class MentorRequestDto
{
    public string Id { get; set; } = string.Empty;
    public string MentorId { get; set; } = string.Empty;
    public string MentorName { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ScheduledAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Create mentor request
/// </summary>
public class CreateMentorRequestDto
{
    public string MentorId { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Accept/Reject mentor request
/// </summary>
public class UpdateMentorRequestStatusDto
{
    public string Status { get; set; } = string.Empty; // Accepted, Rejected
    public DateTime? ScheduledAt { get; set; }
}

/// <summary>
/// Apply as Mentor DTO
/// </summary>
public class ApplyMentorDto
{
    public string Expertise { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public decimal HourlyRate { get; set; }
}
