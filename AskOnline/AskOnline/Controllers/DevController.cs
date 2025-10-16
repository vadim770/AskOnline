using AskOnline.Data;
using AskOnline.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace AskOnline.API.Controllers;

#if DEBUG
[ApiController]
[Route("api/[controller]")]
public class DevController : ControllerBase
{
    private readonly DatabaseSeeder _seeder;

    public DevController(DatabaseSeeder seeder)
    {
        _seeder = seeder;
    }

    /// <summary>
    /// Seeds the database with fake data for testing
    /// </summary>
    [HttpPost("seed")]
    public async Task<IActionResult> SeedDatabase()
    {
        try
        {
            var userCountBefore = await GetDatabaseStats();
            await _seeder.SeedAsync();
            var userCountAfter = await GetDatabaseStats();

            return Ok(new
            {
                message = "Database seeding process completed!",
                before = userCountBefore,
                after = userCountAfter
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Error seeding database",
                error = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    private async Task<object> GetDatabaseStats()
    {
        // You'll need to inject IUnitOfWork into the controller
        // For now, this is a placeholder
        return new
        {
            users = "Check logs",
            questions = "Check logs",
            answers = "Check logs"
        };
    }

    /// <summary>
    /// Clears all data from the database
    /// </summary>
    [HttpPost("clear")]
    public async Task<IActionResult> ClearDatabase()
    {
        try
        {
            await _seeder.ClearAllDataAsync();
            return Ok(new { message = "Database cleared successfully!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error clearing database", error = ex.Message });
        }
    }

    /// <summary>
    /// Clears and reseeds the database
    /// </summary>
    [HttpPost("reset")]
    public async Task<IActionResult> ResetDatabase()
    {
        try
        {
            await _seeder.ClearAllDataAsync();
            await _seeder.SeedAsync();
            return Ok(new { message = "Database reset successfully!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error resetting database", error = ex.Message });
        }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetDatabaseStats([FromServices] IUnitOfWork unitOfWork)
    {
        var stats = new
        {
            users = await unitOfWork.Users.CountAsync(),
            questions = await unitOfWork.Questions.CountAsync(),
            answers = await unitOfWork.Answers.CountAsync(),
            tags = await unitOfWork.Tags.CountAsync(),
            questionRatings = await unitOfWork.QuestionRatings.CountAsync(),
            answerRatings = await unitOfWork.AnswerRatings.CountAsync(),
            comments = await unitOfWork.Comments.CountAsync()
        };

        return Ok(stats);
    }
}
#endif