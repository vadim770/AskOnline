using AskOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace AskOnline.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<QuestionTag> QuestionTags { get; set; }
        public DbSet<AnswerRating> AnswerRatings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<QuestionTag>()
            .HasKey(qt => new { qt.QuestionId, qt.TagId });

            modelBuilder.Entity<QuestionTag>()
            .HasOne(qt => qt.Question)
            .WithMany(q => q.QuestionTags)
            .HasForeignKey(qt => qt.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuestionTag>()
                .HasOne(qt => qt.Tag)
                .WithMany(t => t.QuestionTags)
                .HasForeignKey(qt => qt.TagId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique();

            // allow cascade from questions to answers
            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // restrict cascade on User to Answers
            modelBuilder.Entity<Answer>()
                .HasOne(a => a.User)
                .WithMany(u => u.Answers)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // restrict cascade User to Question
            modelBuilder.Entity<Question>()
                .HasOne(q => q.User)
                .WithMany(u => u.Questions)
                .HasForeignKey(q => q.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AnswerRating>()
            .HasOne(ar => ar.Answer)
            .WithMany(a => a.Ratings)
            .HasForeignKey(ar => ar.AnswerId)
            .OnDelete(DeleteBehavior.Cascade); // delete ratings when answer is deleted

            modelBuilder.Entity<AnswerRating>()
                .HasOne(ar => ar.User)
                .WithMany(u => u.AnswerRatings)
                .HasForeignKey(ar => ar.UserId)
                .OnDelete(DeleteBehavior.Restrict); // don't allow deleting users with ratings

            // ensure one vote per user per answer
            modelBuilder.Entity<AnswerRating>()
                .HasIndex(ar => new { ar.AnswerId, ar.UserId })
                .IsUnique();

            modelBuilder.Entity<AnswerRating>()
                .HasKey(ar => ar.RatingId);
        }
    }
}
