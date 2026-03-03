using Microsoft.EntityFrameworkCore;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;

namespace SkillScape.Infrastructure.Services;

public class ProgressService : IProgressService
{
    private readonly ApplicationDbContext _context;
    private const int XP_PER_SKILL = 10;
    private const long XP_PER_LEVEL = 100;

    public ProgressService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserStatsDto> GetUserStatsAsync(string userId)
    {
        var user = await _context.Users
            .Include(u => u.UserProgressions)
            .Include(u => u.UserSkills)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found");

        var progressions = user.UserProgressions
            .Select(p => new UserProgressDto
            {
                Id = p.Id,
                DomainId = p.CareerDomainId,
                DomainName = p.CareerDomain?.Name ?? "",
                XPInDomain = p.XPInDomain,
                SkillsCompleted = p.SkillsCompleted,
                TotalSkills = p.TotalSkills,
                ProgressPercentage = p.ProgressPercentage
            })
            .ToList();

        var xpToNextLevel = (user.Level * XP_PER_LEVEL) - user.TotalXP;

        return new UserStatsDto
        {
            Level = user.Level,
            TotalXP = user.TotalXP,
            XPToNextLevel = xpToNextLevel,
            CurrentStreak = user.CurrentStreak,
            CompletedSkills = (int)user.UserSkills.Count(us => us.IsCompleted),
            DomainProgressions = progressions
        };
    }

    public async Task CompleteSkillAsync(string userId, string skillId)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found");

        var skill = await _context.Skills
            .Include(s => s.CareerDomain)
            .FirstOrDefaultAsync(s => s.Id == skillId)
            ?? throw new InvalidOperationException("Skill not found");

        var userSkill = await _context.UserSkills
            .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId);

        if (userSkill == null)
        {
            userSkill = new UserSkill
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                SkillId = skillId,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow,
                ProgressPercentage = 100,
                StartedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserSkills.Add(userSkill);
        }
        else if (!userSkill.IsCompleted)
        {
            userSkill.IsCompleted = true;
            userSkill.CompletedAt = DateTime.UtcNow;
            userSkill.ProgressPercentage = 100;
            _context.UserSkills.Update(userSkill);
        }
        else
        {
            throw new InvalidOperationException("Skill already completed");
        }

        // Award XP
        var xpReward = skill.XPReward;
        user.TotalXP += xpReward;

        // Update level
        var newLevel = (int)(user.TotalXP / XP_PER_LEVEL) + 1;
        if (newLevel > user.Level)
        {
            user.Level = newLevel;
        }

        // Update domain progress
        var domainProgress = await _context.UserProgressions
            .FirstOrDefaultAsync(up => up.UserId == userId && up.CareerDomainId == skill.CareerDomainId);

        if (domainProgress == null)
        {
            var totalSkillsInDomain = await _context.Skills
                .CountAsync(s => s.CareerDomainId == skill.CareerDomainId);

            domainProgress = new UserProgress
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                CareerDomainId = skill.CareerDomainId,
                XPInDomain = xpReward,
                SkillsCompleted = 1,
                TotalSkills = totalSkillsInDomain,
                ProgressPercentage = (1.0 / totalSkillsInDomain) * 100,
                StartedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserProgressions.Add(domainProgress);
        }
        else
        {
            domainProgress.XPInDomain += xpReward;
            domainProgress.SkillsCompleted += 1;
            domainProgress.ProgressPercentage = (domainProgress.SkillsCompleted / (double)domainProgress.TotalSkills) * 100;
            domainProgress.UpdatedAt = DateTime.UtcNow;
            _context.UserProgressions.Update(domainProgress);
        }

        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);

        // Check for badge unlocks
        await CheckAndUnlockBadgesAsync(userId, user);

        await _context.SaveChangesAsync();
    }

    public async Task<List<BadgeDto>> GetUserBadgesAsync(string userId)
    {
        var allBadges = await _context.Badges.ToListAsync();
        var earnedBadges = await _context.UserBadges
            .Where(ub => ub.UserId == userId)
            .Select(ub => ub.BadgeId)
            .ToListAsync();

        return allBadges
            .Select(b => new BadgeDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                IconUrl = b.IconUrl,
                Rarity = b.Rarity,
                Earned = earnedBadges.Contains(b.Id),
                EarnedAt = earnedBadges.Contains(b.Id)
                    ? _context.UserBadges
                        .Where(ub => ub.UserId == userId && ub.BadgeId == b.Id)
                        .FirstOrDefault()?.EarnedAt
                    : null
            })
            .ToList();
    }

    private async Task CheckAndUnlockBadgesAsync(string userId, ApplicationUser user)
    {
        var unlockedBadges = await _context.Badges
            .Where(b => b.IsActive && (b.XPRequired <= user.TotalXP || b.SkillsCompletedRequired <= user.UserSkills.Count(us => us.IsCompleted)))
            .ToListAsync();

        foreach (var badge in unlockedBadges)
        {
            var userBadge = await _context.UserBadges
                .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BadgeId == badge.Id);

            if (userBadge == null)
            {
                _context.UserBadges.Add(new UserBadge
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    BadgeId = badge.Id,
                    EarnedAt = DateTime.UtcNow
                });
            }
        }
    }

    public async Task<List<LeaderboardUserDto>> GetLeaderboardAsync(string currentUserId, int limit = 10)
    {
        var topUsers = await _context.Users
            .OrderByDescending(u => u.TotalXP)
            .Take(limit)
            .ToListAsync();

        var leaderboard = new List<LeaderboardUserDto>();
        int rank = 1;
        
        // Random avatars since we don't have a real avatar column yet
        var avatars = new[] { "👨‍💻", "👩‍💻", "👨", "👩‍🎓", "🎯", "🌟", "🚀", "💡" };

        foreach (var user in topUsers)
        {
            leaderboard.Add(new LeaderboardUserDto
            {
                Rank = rank,
                Name = string.IsNullOrWhiteSpace(user.FullName) ? "Anonymous Learner" : user.FullName,
                Xp = user.TotalXP,
                Level = user.Level,
                IsUser = user.Id == currentUserId,
                Avatar = avatars[rank % avatars.Length]
            });
            rank++;
        }

        // If current user is not in top 10, append them at the end with true rank
        if (!leaderboard.Any(l => l.IsUser))
        {
            var currentUser = await _context.Users.FindAsync(currentUserId);
            if (currentUser != null)
            {
                var userRank = await _context.Users.CountAsync(u => u.TotalXP > currentUser.TotalXP) + 1;
                leaderboard.Add(new LeaderboardUserDto
                {
                    Rank = userRank,
                    Name = string.IsNullOrWhiteSpace(currentUser.FullName) ? "You" : currentUser.FullName,
                    Xp = currentUser.TotalXP,
                    Level = currentUser.Level,
                    IsUser = true,
                    Avatar = avatars[userRank % avatars.Length]
                });
            }
        }

        return leaderboard;
    }
}
