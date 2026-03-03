using System.Threading.Tasks;
using SkillScape.Application.DTOs;

namespace SkillScape.Application.Interfaces;

public interface IRoadmapService
{
    /// <summary>
    /// Gets the roadmap specifically tailored to the user's ML-predicted Career Domain.
    /// Includes completion status for each step based on UserSkill progress.
    /// </summary>
    Task<RoadmapDto> GetMyRoadmapAsync(string userId);

    /// <summary>
    /// Marks a specific roadmap step (Skill) as Complete, updating UserSkill and UserProgress.
    /// </summary>
    Task<bool> MarkStepCompleteAsync(string userId, string stepId);
}
