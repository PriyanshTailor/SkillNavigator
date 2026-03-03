using Microsoft.EntityFrameworkCore;
using SkillScape.Domain.Entities;

namespace SkillScape.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<ApplicationUser> Users { get; set; } = null!;
    public DbSet<CareerDomain> CareerDomains { get; set; } = null!;
    public DbSet<Skill> Skills { get; set; } = null!;
    public DbSet<UserSkill> UserSkills { get; set; }
    public DbSet<UserProgress> UserProgressions { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
    public DbSet<QuizQuestion> QuizQuestions { get; set; } = null!;
    public DbSet<QuizOption> QuizOptions { get; set; } = null!;
    public DbSet<QuizResponse> QuizResponses { get; set; } = null!;
    public DbSet<QuizResult> QuizResults { get; set; } = null!;
    public DbSet<Badge> Badges { get; set; } = null!;
    public DbSet<UserBadge> UserBadges { get; set; } = null!;
    public DbSet<RoadmapStep> RoadmapSteps { get; set; } = null!;
    public DbSet<RoadmapTopic> RoadmapTopics { get; set; } = null!;
    public DbSet<UserModuleProgress> UserModuleProgressions { get; set; } = null!;
    public DbSet<ApplicationMentor> Mentors { get; set; } = null!;
    public DbSet<MentorRequest> MentorRequests { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ApplicationUser
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // CareerDomain
        modelBuilder.Entity<CareerDomain>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Color).HasMaxLength(50);
            
            entity.HasMany(d => d.Skills)
                .WithOne(s => s.CareerDomain)
                .HasForeignKey(s => s.CareerDomainId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(d => d.RoadmapSteps)
                .WithOne(r => r.CareerDomain)
                .HasForeignKey(r => r.CareerDomainId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Skill
        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.DifficultyLevel).IsRequired().HasMaxLength(50);
        });

        // UserSkill
        modelBuilder.Entity<UserSkill>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(us => us.User)
                .WithMany(u => u.UserSkills)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(us => us.Skill)
                .WithMany(s => s.UserSkills)
                .HasForeignKey(us => us.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.UserId, e.SkillId }).IsUnique();
        });

        // UserProgress
        modelBuilder.Entity<UserProgress>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(up => up.User)
                .WithMany(u => u.UserProgressions)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(up => up.CareerDomain)
                .WithMany(d => d.UserProgressions)
                .HasForeignKey(up => up.CareerDomainId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.UserId, e.CareerDomainId }).IsUnique();
        });

        // QuizQuestion
        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired();
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            
            entity.HasMany(q => q.Options)
                .WithOne(o => o.QuizQuestion)
                .HasForeignKey(o => o.QuizQuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // QuizOption
        modelBuilder.Entity<QuizOption>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired();
            entity.Property(e => e.DomainWeightJson).IsRequired();
        });

        // QuizResponse
        modelBuilder.Entity<QuizResponse>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(qr => qr.User)
                .WithMany(u => u.QuizResponses)
                .HasForeignKey(qr => qr.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(qr => qr.QuizQuestion)
                .WithMany()
                .HasForeignKey(qr => qr.QuizQuestionId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(qr => qr.QuizOption)
                .WithMany()
                .HasForeignKey(qr => qr.QuizOptionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // QuizResult
        modelBuilder.Entity<QuizResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ScoresJson).IsRequired();
            
            entity.HasOne(qr => qr.User)
                .WithMany()
                .HasForeignKey(qr => qr.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Badge
        modelBuilder.Entity<Badge>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Rarity).IsRequired().HasMaxLength(50);
        });

        // UserBadge
        modelBuilder.Entity<UserBadge>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(ub => ub.User)
                .WithMany(u => u.UserBadges)
                .HasForeignKey(ub => ub.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(ub => ub.Badge)
                .WithMany(b => b.UserBadges)
                .HasForeignKey(ub => ub.BadgeId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.UserId, e.BadgeId }).IsUnique();
        });

        // RoadmapStep
        modelBuilder.Entity<RoadmapStep>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(256);
            
            entity.HasOne(r => r.Skill)
                .WithMany(s => s.RoadmapSteps)
                .HasForeignKey(r => r.SkillId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // RoadmapTopic
        modelBuilder.Entity<RoadmapTopic>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(256);
            
            entity.HasOne(t => t.Module)
                .WithMany(m => m.Topics)
                .HasForeignKey(t => t.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserModuleProgress
        modelBuilder.Entity<UserModuleProgress>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(ump => ump.User)
                .WithMany()
                .HasForeignKey(ump => ump.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(ump => ump.Module)
                .WithMany(m => m.UserProgressions)
                .HasForeignKey(ump => ump.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.UserId, e.ModuleId }).IsUnique();
        });

        // ApplicationMentor
        modelBuilder.Entity<ApplicationMentor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Expertise).IsRequired().HasMaxLength(256);
            
            entity.HasOne(m => m.User)
                .WithOne(u => u.MentorProfile)
                .HasForeignKey<ApplicationMentor>(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MentorRequest
        modelBuilder.Entity<MentorRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Topic).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            
            entity.HasOne(mr => mr.Student)
                .WithMany(u => u.SentRequests)
                .HasForeignKey(mr => mr.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(mr => mr.Mentor)
                .WithMany(m => m.MentorRequests)
                .HasForeignKey(mr => mr.MentorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ChatMessage
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            
            entity.HasOne(cm => cm.Sender)
                .WithMany()
                .HasForeignKey(cm => cm.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(cm => cm.Receiver)
                .WithMany()
                .HasForeignKey(cm => cm.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
