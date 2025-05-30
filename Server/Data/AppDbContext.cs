using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<SustainableActivity> SustainableActivities { get; set; } = null!;
    public DbSet<ActivityCompletion> ActivityCompletions { get; set; } = null!;
    public DbSet<Challenge> Challenges { get; set; } = null!;
    public DbSet<UserChallenge> UserChallenges { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships
        modelBuilder.Entity<ActivityCompletion>()
            .HasOne(ac => ac.User)
            .WithMany(u => u.CompletedActivities)
            .HasForeignKey(ac => ac.UserId);
            
        modelBuilder.Entity<ActivityCompletion>()
            .HasOne(ac => ac.Activity)
            .WithMany(a => a.Completions)
            .HasForeignKey(ac => ac.ActivityId);
            
        modelBuilder.Entity<UserChallenge>()
            .HasOne(uc => uc.User)
            .WithMany()
            .HasForeignKey(uc => uc.UserId);
            
        modelBuilder.Entity<UserChallenge>()
            .HasOne(uc => uc.Challenge)
            .WithMany(c => c.UserChallenges)
            .HasForeignKey(uc => uc.ChallengeId);
    }
}