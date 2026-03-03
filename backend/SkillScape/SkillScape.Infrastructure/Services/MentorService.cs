using Microsoft.EntityFrameworkCore;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;

namespace SkillScape.Infrastructure.Services;

public class MentorService : IMentorService
{
    private readonly ApplicationDbContext _context;

    public MentorService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MentorDto>> GetAllMentorsAsync()
    {
        return await _context.Mentors
            .Include(m => m.User)
            .Where(m => m.IsAvailable)
            .Select(m => new MentorDto
            {
                Id = m.Id,
                UserId = m.UserId,
                UserName = m.User!.FullName,
                Expertise = m.Expertise,
                Bio = m.Bio,
                YearsOfExperience = m.YearsOfExperience,
                HourlyRate = m.HourlyRate,
                IsAvailable = m.IsAvailable,
                TotalSessionCount = m.TotalSessionCount,
                AvgRating = m.AvgRating
            })
            .ToListAsync();
    }

    public async Task<MentorDto> GetMentorByIdAsync(string mentorId)
    {
        var mentor = await _context.Mentors
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == mentorId)
            ?? throw new InvalidOperationException("Mentor not found");

        return new MentorDto
        {
            Id = mentor.Id,
            UserId = mentor.UserId,
            UserName = mentor.User!.FullName,
            Expertise = mentor.Expertise,
            Bio = mentor.Bio,
            YearsOfExperience = mentor.YearsOfExperience,
            HourlyRate = mentor.HourlyRate,
            IsAvailable = mentor.IsAvailable,
            TotalSessionCount = mentor.TotalSessionCount,
            AvgRating = mentor.AvgRating
        };
    }

    public async Task<MentorRequestDto> CreateMentorRequestAsync(string studentId, CreateMentorRequestDto request)
    {
        var student = await _context.Users.FindAsync(studentId)
            ?? throw new InvalidOperationException("Student not found");

        var mentor = await _context.Mentors.FindAsync(request.MentorId)
            ?? throw new InvalidOperationException("Mentor not found");

        var mentorRequest = new MentorRequest
        {
            Id = Guid.NewGuid().ToString(),
            StudentId = studentId,
            MentorId = request.MentorId,
            Topic = request.Topic,
            Message = request.Message,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MentorRequests.Add(mentorRequest);
        await _context.SaveChangesAsync();

        return new MentorRequestDto
        {
            Id = mentorRequest.Id,
            MentorId = mentorRequest.MentorId,
            MentorName = mentor.User?.FullName ?? "",
            Topic = mentorRequest.Topic,
            Status = mentorRequest.Status,
            CreatedAt = mentorRequest.CreatedAt
        };
    }

    public async Task<List<MentorRequestDto>> GetPendingRequestsAsync(string mentorId)
    {
        return await _context.MentorRequests
            .Include(mr => mr.Student)
            .Where(mr => mr.MentorId == mentorId && mr.Status == "Pending")
            .OrderByDescending(mr => mr.CreatedAt)
            .Select(mr => new MentorRequestDto
            {
                Id = mr.Id,
                MentorId = mr.MentorId,
                MentorName = mr.Student!.FullName,
                Topic = mr.Topic,
                Status = mr.Status,
                ScheduledAt = mr.ScheduledAt,
                CreatedAt = mr.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<MentorRequestDto> UpdateRequestStatusAsync(string requestId, UpdateMentorRequestStatusDto request)
    {
        var mentorRequest = await _context.MentorRequests
            .Include(mr => mr.Mentor)
            .FirstOrDefaultAsync(mr => mr.Id == requestId)
            ?? throw new InvalidOperationException("Request not found");

        mentorRequest.Status = request.Status;
        mentorRequest.ScheduledAt = request.ScheduledAt;
        mentorRequest.UpdatedAt = DateTime.UtcNow;

        if (request.Status == "Accepted")
        {
            var mentor = mentorRequest.Mentor;
            if (mentor != null)
            {
                mentor.TotalSessionCount++;
                _context.Mentors.Update(mentor);
            }
        }

        _context.MentorRequests.Update(mentorRequest);
        await _context.SaveChangesAsync();

        return new MentorRequestDto
        {
            Id = mentorRequest.Id,
            MentorId = mentorRequest.MentorId,
            MentorName = mentorRequest.Mentor?.User?.FullName ?? "",
            Topic = mentorRequest.Topic,
            Status = mentorRequest.Status,
            ScheduledAt = mentorRequest.ScheduledAt,
            CreatedAt = mentorRequest.CreatedAt
        };
    }

    public async Task<MentorDto> ApplyAsMentorAsync(string userId, ApplyMentorDto request)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found");

        var existingMentor = await _context.Mentors.FirstOrDefaultAsync(m => m.UserId == userId);
        if (existingMentor != null)
        {
            throw new InvalidOperationException("User is already registered as a mentor.");
        }

        var mentor = new ApplicationMentor
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Expertise = request.Expertise,
            Bio = request.Bio,
            YearsOfExperience = request.YearsOfExperience,
            HourlyRate = request.HourlyRate,
            IsAvailable = true,
            TotalSessionCount = 0,
            AvgRating = 0,
            CreatedAt = DateTime.UtcNow
        };

        // Optionally update the user's role to Mentor if they were a Student
        if (user.Role == "Student")
        {
            user.Role = "Mentor";
            _context.Users.Update(user);
        }

        _context.Mentors.Add(mentor);
        await _context.SaveChangesAsync();

        return new MentorDto
        {
            Id = mentor.Id,
            UserId = mentor.UserId,
            UserName = user.FullName,
            Expertise = mentor.Expertise,
            Bio = mentor.Bio,
            YearsOfExperience = mentor.YearsOfExperience,
            HourlyRate = mentor.HourlyRate,
            IsAvailable = mentor.IsAvailable,
            TotalSessionCount = mentor.TotalSessionCount,
            AvgRating = mentor.AvgRating
        };
    }
}
