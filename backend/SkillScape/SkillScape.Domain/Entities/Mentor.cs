namespace SkillScape.Domain.Entities;

/// <summary>
/// Mentor - user who can mentor others
/// </summary>
public class ApplicationMentor
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string UserId { get; set; } = string.Empty;
    
    public string Expertise { get; set; } = string.Empty; // Domain expertise
    
    public string? Bio { get; set; }
    
    public int YearsOfExperience { get; set; } = 0;
    
    public decimal HourlyRate { get; set; } = 0;
    
    public bool IsAvailable { get; set; } = true;
    
    public int TotalSessionCount { get; set; } = 0;
    
    public double AvgRating { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ApplicationUser? User { get; set; }
    public ICollection<MentorRequest> MentorRequests { get; set; } = new List<MentorRequest>();
}

/// <summary>
/// Mentor request from student
/// </summary>
public class MentorRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string StudentId { get; set; } = string.Empty;
    
    public string MentorId { get; set; } = string.Empty;
    
    public string Topic { get; set; } = string.Empty;
    
    public string Message { get; set; } = string.Empty;
    
    public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected, Cancelled
    
    public DateTime? ScheduledAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ApplicationUser? Student { get; set; }
    public ApplicationMentor? Mentor { get; set; }
}
