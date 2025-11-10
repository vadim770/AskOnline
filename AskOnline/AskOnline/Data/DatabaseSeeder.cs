
using AskOnline.Data;
using AskOnline.Models;
using Bogus;
using Microsoft.AspNetCore.Identity;

namespace AskOnline.Infrastructure.Data;

public class DatabaseSeeder
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly IPasswordHasher<User> _passwordHasher;
    private List<User> _users = new();
    private List<Tag> _tags = new();
    private List<Question> _questions = new();

    public DatabaseSeeder(IUnitOfWork unitOfWork, ILogger<DatabaseSeeder> logger, IPasswordHasher<User> passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync()
    {
        // Check if data already exists
        var userCount = await _unitOfWork.Users.CountAsync();
        _logger.LogInformation("Current user count in database: {UserCount}", userCount);

        if (userCount > 0)
        {
            _logger.LogWarning("Database already contains {UserCount} users. Skipping seed.", userCount);
            Console.WriteLine($"Database already contains {userCount} users. Skipping seed.");
            return;
        }

        _logger.LogInformation("Starting database seeding...");
        Console.WriteLine("Starting database seeding...");

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            await SeedUsers();
            await SeedTags();
            await SeedQuestions();
            await SeedAnswers();
            await SeedQuestionRatings();
            await SeedAnswerRatings();
            await SeedComments();

            await _unitOfWork.CommitTransactionAsync();
            _logger.LogInformation("Database seeding completed successfully!");
            Console.WriteLine("Database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error seeding database: {Message}", ex.Message);
            Console.WriteLine($"Error seeding database: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private async Task SeedUsers()
    {
        Console.WriteLine("Seeding users...");

        var userFaker = new Faker<User>()
            .RuleFor(u => u.Username, f => f.Internet.UserName())
            .RuleFor(u => u.Email, f => f.Internet.Email().ToLower())
            .RuleFor(u => u.Role, f => f.Random.Bool(0.95f) ? Roles.User : Roles.Admin) // 95% regular users, 5% admins
            .RuleFor(u => u.CreatedAt, f => f.Date.Past(2));

        _users = userFaker.Generate(50);

        // Create a specific admin user
        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@admin.com",
            Role = Roles.Admin,
            CreatedAt = DateTime.UtcNow
        };
        adminUser.PasswordHash = _passwordHasher.HashPassword(adminUser, "admin");
        _users.Add(adminUser); // Add the admin user to the list

        // Hash passwords using Identity's PasswordHasher
        foreach (var user in _users)
        {
            // Skip hashing for the admin user as it's already done
            if (user.Email == adminUser.Email) continue;
            user.PasswordHash = _passwordHasher.HashPassword(user, "Password123!");
        }

        await _unitOfWork.Users.AddRangeAsync(_users);
        await _unitOfWork.SaveChangesAsync();

        Console.WriteLine($"Created {_users.Count} users (password for all: Password123!)");
        _logger.LogInformation("Created {UserCount} users", _users.Count);
    }

    private async Task SeedTags()
    {
        Console.WriteLine("Seeding tags...");

        var tagNames = new[]
        {
            "C#", "JavaScript", "React", "ASP.NET", "SQL", "Entity Framework",
            "TypeScript", "Node.js", "Python", "Java", "Angular", "Vue.js",
            "Docker", "Azure", "AWS", "Git", "REST API", "GraphQL",
            "MongoDB", "PostgreSQL", "Redis", "Microservices", "Design Patterns",
            "Authentication", "Security", "Performance", "Testing", "Debugging",
            "HTML", "CSS", "Bootstrap", "Tailwind"
        };

        _tags = tagNames.Select(name => new Tag
        {
            Name = name,
            CreatedAt = DateTime.UtcNow.AddDays(-new Random().Next(1, 365))
        }).ToList();

        await _unitOfWork.Tags.AddRangeAsync(_tags);
        await _unitOfWork.SaveChangesAsync();

        Console.WriteLine($"Created {_tags.Count} tags");
    }

    private async Task SeedQuestions()
    {
        Console.WriteLine("Seeding questions...");

        var questionTitles = new[]
        {
            "How to implement async/await properly in C#?",
            "What's the difference between IEnumerable and IQueryable?",
            "Best practices for React state management",
            "How to optimize SQL queries for large datasets?",
            "Understanding dependency injection in ASP.NET Core",
            "How to handle authentication with JWT tokens?",
            "What are the benefits of using Repository pattern?",
            "How to debug Entity Framework queries?",
            "React hooks vs class components - which to use?",
            "How to implement caching in ASP.NET Core?",
            "Understanding SOLID principles with examples",
            "How to handle errors in async JavaScript?",
            "Best way to structure a React application?",
            "How to prevent SQL injection attacks?",
            "Understanding database indexing and performance",
            "How to implement unit testing in C#?",
            "What's the difference between var and let in JavaScript?",
            "How to use LINQ effectively?",
            "Understanding RESTful API design principles",
            "How to handle concurrent requests in web applications?",
            "What is the Unit of Work pattern?",
            "How to implement pagination in ASP.NET Core?",
            "React Context API vs Redux - when to use each?",
            "How to secure API endpoints in ASP.NET Core?",
            "What are the best practices for error handling in C#?",
            "How to optimize React application performance?",
            "Understanding middleware in ASP.NET Core",
            "How to implement file upload in React?",
            "What is the difference between authentication and authorization?",
            "How to use Entity Framework migrations?",
            "Best practices for database design",
            "How to implement real-time features with SignalR?",
            "Understanding CORS in web applications",
            "How to handle validation in ASP.NET Core?",
            "What are React custom hooks and how to create them?",
            "How to implement logging in ASP.NET Core?",
            "Understanding async/await in JavaScript",
            "How to implement search functionality in SQL?",
            "What is the difference between PUT and PATCH?",
            "How to test React components with Jest?",
            "Understanding database transactions",
            "How to implement role-based authorization?",
            "What are the benefits of using TypeScript?",
            "How to handle database migrations in production?",
            "Understanding React lifecycle methods",
            "How to implement rate limiting in APIs?",
            "What is lazy loading and how to implement it?",
            "How to optimize SQL Server performance?",
            "Understanding HTTP status codes",
            "How to implement OAuth2 authentication?"
        };

        var questionFaker = new Faker<Question>()
            .RuleFor(q => q.Title, f => f.PickRandom(questionTitles))
            .RuleFor(q => q.Body, f => f.Lorem.Paragraphs(2, 4))
            .RuleFor(q => q.UserId, f => f.PickRandom(_users).UserId)
            .RuleFor(q => q.CreatedAt, f => f.Date.Past(1));

        _questions = questionFaker.Generate(100);
        await _unitOfWork.Questions.AddRangeAsync(_questions);
        await _unitOfWork.SaveChangesAsync();

        // Link tags to questions
        Console.WriteLine("Linking tags to questions...");
        var questionTags = new List<QuestionTag>();

        foreach (var question in _questions)
        {
            var numberOfTags = new Random().Next(1, 5);
            var selectedTags = _tags.OrderBy(x => Guid.NewGuid()).Take(numberOfTags);

            foreach (var tag in selectedTags)
            {
                questionTags.Add(new QuestionTag
                {
                    QuestionId = question.QuestionId,
                    TagId = tag.TagId
                });
            }
        }

        await _unitOfWork.QuestionTags.AddRangeAsync(questionTags);
        await _unitOfWork.SaveChangesAsync();

        Console.WriteLine($"Created {_questions.Count} questions with {questionTags.Count} tag associations");
    }

    private async Task SeedAnswers()
    {
        Console.WriteLine("Seeding answers...");

        var answers = new List<Answer>();
        var faker = new Faker();

        foreach (var question in _questions)
        {
            var numberOfAnswers = faker.Random.Int(0, 6);

            for (int i = 0; i < numberOfAnswers; i++)
            {
                var answer = new Answer
                {
                    QuestionId = question.QuestionId,
                    UserId = faker.PickRandom(_users).UserId,
                    Body = faker.Lorem.Paragraphs(1, 3),
                    CreatedAt = faker.Date.Between(question.CreatedAt, DateTime.UtcNow)
                };

                answers.Add(answer);
            }
        }

        await _unitOfWork.Answers.AddRangeAsync(answers);
        await _unitOfWork.SaveChangesAsync();

        Console.WriteLine($"Created {answers.Count} answers");
    }

    private async Task SeedQuestionRatings()
    {
        Console.WriteLine("Seeding question ratings...");

        var ratings = new List<QuestionRating>();
        var faker = new Faker();

        foreach (var question in _questions)
        {
            var numberOfRatings = faker.Random.Int(0, 20);
            var usedUsers = new HashSet<int>();

            for (int i = 0; i < numberOfRatings; i++)
            {
                var user = faker.PickRandom(_users);

                // Ensure one user only votes once per question
                if (usedUsers.Contains(user.UserId))
                    continue;

                usedUsers.Add(user.UserId);

                ratings.Add(new QuestionRating
                {
                    QuestionId = question.QuestionId,
                    UserId = user.UserId,
                    IsUpvote = faker.Random.Bool(0.7f), // 70% upvotes, 30% downvotes
                    CreatedAt = faker.Date.Between(question.CreatedAt, DateTime.UtcNow)
                });
            }
        }

        await _unitOfWork.QuestionRatings.AddRangeAsync(ratings);
        await _unitOfWork.SaveChangesAsync();

        Console.WriteLine($"Created {ratings.Count} question ratings");
    }

    private async Task SeedAnswerRatings()
    {
        Console.WriteLine("Seeding answer ratings...");

        var allAnswers = await _unitOfWork.Answers.GetAllAsync();
        var ratings = new List<AnswerRating>();
        var faker = new Faker();

        foreach (var answer in allAnswers)
        {
            var numberOfRatings = faker.Random.Int(0, 15);
            var usedUsers = new HashSet<int>();

            for (int i = 0; i < numberOfRatings; i++)
            {
                var user = faker.PickRandom(_users);

                if (usedUsers.Contains(user.UserId))
                    continue;

                usedUsers.Add(user.UserId);

                ratings.Add(new AnswerRating
                {
                    AnswerId = answer.AnswerId,
                    UserId = user.UserId,
                    IsUpvote = faker.Random.Bool(0.75f), // 75% upvotes, 25% downvotes
                    CreatedAt = faker.Date.Between(answer.CreatedAt, DateTime.UtcNow)
                });
            }
        }

        await _unitOfWork.AnswerRatings.AddRangeAsync(ratings);
        await _unitOfWork.SaveChangesAsync();

        Console.WriteLine($"Created {ratings.Count} answer ratings");
    }

    private async Task SeedComments()
    {
        Console.WriteLine("Seeding comments...");

        var allAnswers = await _unitOfWork.Answers.GetAllAsync();
        var comments = new List<Comment>();
        var faker = new Faker();

        // Comments on answers only (based on your Comment model)
        foreach (var answer in allAnswers)
        {
            var numberOfComments = faker.Random.Int(0, 4);

            for (int i = 0; i < numberOfComments; i++)
            {
                comments.Add(new Comment
                {
                    AnswerId = answer.AnswerId,
                    UserId = faker.PickRandom(_users).UserId,
                    Text = faker.Lorem.Sentence(faker.Random.Int(5, 15)),
                    CreatedAt = faker.Date.Between(answer.CreatedAt, DateTime.UtcNow)
                });
            }
        }

        await _unitOfWork.Comments.AddRangeAsync(comments);
        await _unitOfWork.SaveChangesAsync();

        Console.WriteLine($"Created {comments.Count} comments");
    }

    public async Task ClearAllDataAsync()
    {
        Console.WriteLine("Clearing all data...");

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Delete in reverse order of dependencies
            var comments = await _unitOfWork.Comments.GetAllAsync();
            foreach (var comment in comments)
                await _unitOfWork.Comments.DeleteAsync(comment);

            var answerRatings = await _unitOfWork.AnswerRatings.GetAllAsync();
            foreach (var rating in answerRatings)
                await _unitOfWork.AnswerRatings.DeleteAsync(rating);

            var questionRatings = await _unitOfWork.QuestionRatings.GetAllAsync();
            foreach (var rating in questionRatings)
                await _unitOfWork.QuestionRatings.DeleteAsync(rating);

            var answers = await _unitOfWork.Answers.GetAllAsync();
            foreach (var answer in answers)
                await _unitOfWork.Answers.DeleteAsync(answer);

            var questionTags = await _unitOfWork.QuestionTags.GetAllAsync();
            foreach (var qt in questionTags)
                await _unitOfWork.QuestionTags.DeleteAsync(qt);

            var questions = await _unitOfWork.Questions.GetAllAsync();
            foreach (var question in questions)
                await _unitOfWork.Questions.DeleteAsync(question);

            var tags = await _unitOfWork.Tags.GetAllAsync();
            foreach (var tag in tags)
                await _unitOfWork.Tags.DeleteAsync(tag);

            var users = await _unitOfWork.Users.GetAllAsync();
            foreach (var user in users)
                await _unitOfWork.Users.DeleteAsync(user);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            Console.WriteLine("All data cleared successfully!");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            Console.WriteLine($"Error clearing data: {ex.Message}");
            throw;
        }
    }
}