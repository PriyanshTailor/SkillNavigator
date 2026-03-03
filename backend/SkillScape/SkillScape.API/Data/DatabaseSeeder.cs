using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;
using System.Text.Json;

namespace SkillScape.API.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Skip if data already exists
        if (context.CareerDomains.Any())
            return;

        // Seed Career Domains
        var domains = SeedDomains(context);
        context.CareerDomains.AddRange(domains);
        await context.SaveChangesAsync();

        // Seed Skills
        var skills = SeedSkills(context, domains);
        context.Skills.AddRange(skills);
        await context.SaveChangesAsync();

        // Seed Quiz Questions and Options
        var (questions, options) = SeedQuiz(context, domains);
        context.QuizQuestions.AddRange(questions);
        context.QuizOptions.AddRange(options);
        await context.SaveChangesAsync();

        // Seed Badges
        var badges = SeedBadges();
        context.Badges.AddRange(badges);
        await context.SaveChangesAsync();

        // Seed Roadmap Steps
        var roadmapSteps = SeedRoadmapSteps(context, domains, skills);
        context.RoadmapSteps.AddRange(roadmapSteps);
        await context.SaveChangesAsync();

        // Seed Sample Users
        var users = SeedUsers();
        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        // Seed Sample Mentors
        var mentors = SeedMentors(context, users.Where(u => u.Role == "Mentor").ToList());
        context.Mentors.AddRange(mentors);
        await context.SaveChangesAsync();
    }

    private static List<CareerDomain> SeedDomains(ApplicationDbContext context)
    {
        return new List<CareerDomain>
        {
            new CareerDomain
            {
                Id = "frontend",
                Name = "Frontend Development",
                Description = "Build beautiful and interactive user interfaces with modern web technologies",
                Color = "bg-pink-500",
                IconUrl = "https://api.dicebear.com/7.x/icons/svg?seed=frontend",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CareerDomain
            {
                Id = "backend",
                Name = "Backend Development",
                Description = "Build robust APIs and server-side applications that power modern applications",
                Color = "bg-blue-500",
                IconUrl = "https://api.dicebear.com/7.x/icons/svg?seed=backend",
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CareerDomain
            {
                Id = "fullstack",
                Name = "Full Stack Development",
                Description = "Master both frontend and backend to become a complete developer",
                Color = "bg-purple-500",
                IconUrl = "https://api.dicebear.com/7.x/icons/svg?seed=fullstack",
                DisplayOrder = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CareerDomain
            {
                Id = "data",
                Name = "Data Science",
                Description = "Analyze data and extract insights using Python, SQL, and statistical methods",
                Color = "bg-green-500",
                IconUrl = "https://api.dicebear.com/7.x/icons/svg?seed=data",
                DisplayOrder = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CareerDomain
            {
                Id = "devops",
                Name = "DevOps & Cloud",
                Description = "Master deployment, scaling, and infrastructure automation",
                Color = "bg-orange-500",
                IconUrl = "https://api.dicebear.com/7.x/icons/svg?seed=devops",
                DisplayOrder = 5,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CareerDomain
            {
                Id = "ml",
                Name = "Machine Learning",
                Description = "Build intelligent systems using machine learning algorithms",
                Color = "bg-indigo-500",
                IconUrl = "https://api.dicebear.com/7.x/icons/svg?seed=ml",
                DisplayOrder = 6,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CareerDomain
            {
                Id = "business",
                Name = "Business & Management",
                Description = "Lead companies, manage budgets, and drive strategic growth",
                Color = "bg-yellow-500",
                IconUrl = "https://api.dicebear.com/7.x/icons/svg?seed=business",
                DisplayOrder = 7,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CareerDomain
            {
                Id = "design",
                Name = "Creative & Design",
                Description = "Create stunning visual graphics, brands, and UI/UX",
                Color = "bg-red-500",
                IconUrl = "https://api.dicebear.com/7.x/icons/svg?seed=design",
                DisplayOrder = 8,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CareerDomain
            {
                Id = "healthcare",
                Name = "Healthcare & Medicine",
                Description = "Treat patients, innovate medical research, and save lives",
                Color = "bg-teal-500",
                IconUrl = "https://api.dicebear.com/7.x/icons/svg?seed=healthcare",
                DisplayOrder = 9,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CareerDomain
            {
                Id = "education",
                Name = "Education & Teaching",
                Description = "Inspire students, foster learning, and cultivate growth",
                Color = "bg-lime-500",
                IconUrl = "https://api.dicebear.com/7.x/icons/svg?seed=education",
                DisplayOrder = 10,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    // Classes to deserialize the new curriculum JSON
    private class JsonCurriculumDomain
    {
        public string domainId { get; set; } = string.Empty;
        public List<JsonCurriculumSkill> skills { get; set; } = new();
    }
    private class JsonCurriculumSkill
    {
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string difficultyLevel { get; set; } = string.Empty;
        public int xpReward { get; set; }
        public int displayOrder { get; set; }
        public List<JsonCurriculumStep> steps { get; set; } = new();
    }
    private class JsonCurriculumStep
    {
        public string id { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public int estimatedHours { get; set; }
        public int stepNumber { get; set; }
        public List<JsonCurriculumTopic> topics { get; set; } = new();
    }
    private class JsonCurriculumTopic
    {
        public string title { get; set; } = string.Empty;
        public int displayOrder { get; set; }
    }

    private static List<JsonCurriculumDomain> LoadCurriculum()
    {
        var jsonFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "roadmap_curriculum.json");
        if (!File.Exists(jsonFilePath))
        {
            throw new FileNotFoundException($"Curriculum file not found at {jsonFilePath}");
        }

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var fileContent = File.ReadAllText(jsonFilePath);
        return JsonSerializer.Deserialize<List<JsonCurriculumDomain>>(fileContent, jsonOptions) ?? new List<JsonCurriculumDomain>();
    }

    private static List<Skill> SeedSkills(ApplicationDbContext context, List<CareerDomain> domains)
    {
        var skillsDb = new List<Skill>();
        var curriculum = LoadCurriculum();

        foreach (var cDomain in curriculum)
        {
            var domain = domains.FirstOrDefault(d => d.Id == cDomain.domainId);
            if (domain == null) continue;

            foreach (var cSkill in cDomain.skills)
            {
                skillsDb.Add(new Skill
                {
                    Id = cSkill.id,
                    CareerDomainId = domain.Id,
                    Name = cSkill.name,
                    Description = cSkill.description,
                    DifficultyLevel = cSkill.difficultyLevel,
                    XPReward = cSkill.xpReward,
                    DisplayOrder = cSkill.displayOrder,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        return skillsDb;
    }

    private static List<RoadmapStep> SeedRoadmapSteps(ApplicationDbContext context, List<CareerDomain> domains, List<Skill> skills)
    {
        var stepsDb = new List<RoadmapStep>();
        var curriculum = LoadCurriculum();

        foreach (var cDomain in curriculum)
        {
            var domain = domains.FirstOrDefault(d => d.Id == cDomain.domainId);
            if (domain == null) continue;

            foreach (var cSkill in cDomain.skills)
            {
                var skill = skills.FirstOrDefault(s => s.Id == cSkill.id);
                if (skill == null) continue;

                foreach (var cStep in cSkill.steps)
                {
                    var roadmapStep = new RoadmapStep
                    {
                        Id = cStep.id,
                        CareerDomainId = domain.Id,
                        SkillId = skill.Id,
                        Title = cStep.title,
                        Description = cStep.description,
                        StepNumber = cStep.stepNumber,
                        EstimatedHours = cStep.estimatedHours,
                        IsActive = true,
                        Topics = new List<RoadmapTopic>()
                    };

                    foreach (var cTopic in cStep.topics)
                    {
                        roadmapStep.Topics.Add(new RoadmapTopic
                        {
                            Id = Guid.NewGuid().ToString(),
                            ModuleId = roadmapStep.Id,
                            Title = cTopic.title,
                            DisplayOrder = cTopic.displayOrder,
                            ResourceUrl = "https://developer.mozilla.org"
                        });
                    }
                    
                    stepsDb.Add(roadmapStep);
                }
            }
        }

        return stepsDb;
    }

    private class JsonQuizQuestion
    {
        public string id { get; set; } = string.Empty;
        public string text { get; set; } = string.Empty;
        public string category { get; set; } = string.Empty;
        public List<JsonQuizOption> options { get; set; } = new();
    }
    
    private class JsonQuizOption
    {
        public string id { get; set; } = string.Empty;
        public string text { get; set; } = string.Empty;
        public string domain { get; set; } = string.Empty;
    }

    private static (List<QuizQuestion>, List<QuizOption>) SeedQuiz(ApplicationDbContext context, List<CareerDomain> domains)
    {
        var questionsDb = new List<QuizQuestion>();
        var optionsDb = new List<QuizOption>();

        // Load questions from JSON file instead of hardcoding
        var jsonFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "quiz_150_questions.json");
        if (!File.Exists(jsonFilePath))
        {
            throw new FileNotFoundException($"Seed file not found at {jsonFilePath}");
        }

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var fileContent = File.ReadAllText(jsonFilePath);
        var parsedQuestions = JsonSerializer.Deserialize<List<JsonQuizQuestion>>(fileContent, jsonOptions);

        if (parsedQuestions == null) return (questionsDb, optionsDb);

        int qOrder = 1;
        foreach (var q in parsedQuestions)
        {
            var dbQ = new QuizQuestion 
            { 
                Id = q.id, 
                Text = q.text, 
                Category = q.category, 
                DisplayOrder = qOrder++, 
                IsActive = true, 
                // Just arbitrary mapping for the schema requirement
                CareerDomainId = domains[0].Id 
            };
            questionsDb.Add(dbQ);

            int optOrder = 1;
            foreach (var opt in q.options)
            {
                optionsDb.Add(new QuizOption 
                { 
                    Id = opt.id, 
                    QuizQuestionId = dbQ.Id, 
                    Text = opt.text,
                    // Map the chosen domain with weight 1 (doesn't matter since we use ML now, but keeps old logic safe)
                    DomainWeightJson = JsonSerializer.Serialize(new Dictionary<string, int> { { opt.domain, 1 } }), 
                    DisplayOrder = optOrder++ 
                });
            }
        }

        return (questionsDb, optionsDb);
    }

    private static List<Badge> SeedBadges()
    {
        return new List<Badge>
        {
            new Badge { Id = Guid.NewGuid().ToString(), Name = "First Steps", Description = "Complete your first skill", IconUrl = "👣", Rarity = "Common", XPRequired = 0, SkillsCompletedRequired = 1, IsActive = true },
            new Badge { Id = Guid.NewGuid().ToString(), Name = "Quick Learner", Description = "Complete 5 skills", IconUrl = "⚡", Rarity = "Rare", XPRequired = 0, SkillsCompletedRequired = 5, IsActive = true },
            new Badge { Id = Guid.NewGuid().ToString(), Name = "Skill Master", Description = "Complete 10 skills", IconUrl = "🏆", Rarity = "Epic", XPRequired = 0, SkillsCompletedRequired = 10, IsActive = true },
            new Badge { Id = Guid.NewGuid().ToString(), Name = "Legendary", Description = "Reach 500 XP", IconUrl = "👑", Rarity = "Legendary", XPRequired = 500, IsActive = true },
            new Badge { Id = Guid.NewGuid().ToString(), Name = "Domain Expert", Description = "Complete a full domain", IconUrl = "🔥", Rarity = "Epic", XPRequired = 100, IsActive = true }
        };
    }

    private static List<ApplicationUser> SeedUsers()
    {
        return new List<ApplicationUser>
        {
            new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "student@example.com",
                FullName = "John Learner",
                Role = "Student",
                Bio = "Aspiring developer learning web technologies",
                Level = 1,
                TotalXP = 0,
                CurrentStreak = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "mentor@example.com",
                FullName = "Sarah Chen",
                Role = "Mentor",
                Bio = "Senior developer with 10 years of experience",
                Level = 10,
                TotalXP = 1000,
                CurrentStreak = 15,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "admin@example.com",
                FullName = "Admin User",
                Role = "Admin",
                Level = 20,
                TotalXP = 5000,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    private static List<ApplicationMentor> SeedMentors(ApplicationDbContext context, List<ApplicationUser> mentorUsers)
    {
        return mentorUsers.Select(user => new ApplicationMentor
        {
            Id = Guid.NewGuid().ToString(),
            UserId = user.Id,
            Expertise = "Full Stack Development, Web Technologies",
            Bio = user.Bio,
            YearsOfExperience = 10,
            HourlyRate = 50,
            IsAvailable = true,
            TotalSessionCount = 25,
            AvgRating = 4.8,
            CreatedAt = DateTime.UtcNow
        }).ToList();
    }
}
