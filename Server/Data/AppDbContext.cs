using Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL; // Important PostgreSQL namespace
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace Server.Data;

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
    
    // Add the ConfigureConventions method to handle DateTime UTC conversion
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        
        // Configure all DateTime properties to use UTC time
        configurationBuilder
            .Properties<DateTime>()
            .HaveConversion<UtcValueConverter>();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Explicitly set table names in lowercase (PostgreSQL convention)
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<Challenge>().ToTable("challenges");
        modelBuilder.Entity<SustainableActivity>().ToTable("sustainableactivities");
        modelBuilder.Entity<ActivityCompletion>().ToTable("activitycompletions");
        modelBuilder.Entity<UserChallenge>().ToTable("userchallenges");
        
        // Map properties to lowercase column names for PostgreSQL compatibility
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Username).HasColumnName("username");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Password).HasColumnName("password");
            entity.Property(e => e.JoinDate).HasColumnName("joindate");
            entity.Property(e => e.Points).HasColumnName("points");
            entity.Property(e => e.CurrentStreak).HasColumnName("currentstreak");
            entity.Property(e => e.MaxStreak).HasColumnName("maxstreak");
            entity.Property(e => e.LastActivityDate).HasColumnName("lastactivitydate");
            entity.Property(e => e.Course).HasColumnName("course");
            entity.Property(e => e.YearOfStudy).HasColumnName("yearofstudy");
            entity.Property(e => e.AccommodationType).HasColumnName("accommodationtype");
        });
        
        modelBuilder.Entity<Challenge>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.StartDate).HasColumnName("startdate");
            entity.Property(e => e.EndDate).HasColumnName("enddate");
            entity.Property(e => e.PointsReward).HasColumnName("pointsreward");
            entity.Property(e => e.Category).HasColumnName("category");
        });
        
        modelBuilder.Entity<SustainableActivity>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.PointsValue).HasColumnName("pointsvalue");
            entity.Property(e => e.Category).HasColumnName("category");
            entity.Property(e => e.IsDaily).HasColumnName("isdaily");
            entity.Property(e => e.IsWeekly).HasColumnName("isweekly");
            entity.Property(e => e.IsOneTime).HasColumnName("isonetime");
        });
        
        modelBuilder.Entity<ActivityCompletion>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("userid");
            entity.Property(e => e.ActivityId).HasColumnName("activityid");
            entity.Property(e => e.CompletedAt).HasColumnName("completedat"); // Updated property and column name
            entity.Property(e => e.PointsEarned).HasColumnName("pointsearned");
        });
        
        modelBuilder.Entity<UserChallenge>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("userid");
            entity.Property(e => e.ChallengeId).HasColumnName("challengeid");
            entity.Property(e => e.JoinedDate).HasColumnName("joineddate");
            entity.Property(e => e.Completed).HasColumnName("completed");
            entity.Property(e => e.CompletedAt).HasColumnName("completedat");
            entity.Property(e => e.Progress).HasColumnName("progress");
            entity.Property(e => e.Status).HasColumnName("status");
            
            // Ignore the IsCompleted property since it's just an alias
            entity.Ignore(u => u.IsCompleted);
            // Ignore the CompletedDate property since it's just an alias
            entity.Ignore(u => u.CompletedDate);
        });
        
        // Configure User <-> ActivityCompletion relationship
        modelBuilder.Entity<ActivityCompletion>()
            .HasOne(ac => ac.User)
            .WithMany(u => u.CompletedActivities)
            .HasForeignKey(ac => ac.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Configure SustainableActivity <-> ActivityCompletion relationship
        modelBuilder.Entity<ActivityCompletion>()
            .HasOne(ac => ac.Activity)
            .WithMany(a => a.Completions)
            .HasForeignKey(ac => ac.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Configure User <-> UserChallenge relationship
        modelBuilder.Entity<UserChallenge>()
            .HasOne(uc => uc.User)
            .WithMany(u => u.UserChallenges)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Configure Challenge <-> UserChallenge relationship
        modelBuilder.Entity<UserChallenge>()
            .HasOne(uc => uc.Challenge)
            .WithMany(c => c.UserChallenges)
            .HasForeignKey(uc => uc.ChallengeId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // IMPORTANT: Fix for the ChallengeId error
        // Tell EF Core there's no direct relationship between Challenge and SustainableActivity
        // This will prevent it from trying to query for a non-existent ChallengeId column
        modelBuilder.Entity<Challenge>()
            .Navigation(c => c.Activities)
            .UsePropertyAccessMode(PropertyAccessMode.Property);
    }
}

// Define the UtcValueConverter class within the same namespace
public class UtcValueConverter : ValueConverter<DateTime, DateTime>
{
    public UtcValueConverter() 
        : base(
            v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}