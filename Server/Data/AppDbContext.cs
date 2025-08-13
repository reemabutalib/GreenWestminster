using Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL; // Important PostgreSQL namespace
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace Server.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<SustainableActivity> SustainableActivities { get; set; } = null!;
    public DbSet<ActivityCompletion> ActivityCompletions { get; set; } = null!;
    public DbSet<Challenge> Challenges { get; set; } = null!;
    public DbSet<UserChallenge> UserChallenges { get; set; } = null!;
    public DbSet<SustainabilityEvent> SustainabilityEvents { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;


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
        // This is crucial - call the base implementation first to configure Identity tables
        base.OnModelCreating(modelBuilder);

        // Fix table naming for Identity tables to match your PostgreSQL convention
        modelBuilder.Entity<IdentityUser>(entity => entity.ToTable("aspnet_users"));
        modelBuilder.Entity<IdentityRole>(entity => entity.ToTable("aspnet_roles"));
        modelBuilder.Entity<IdentityUserRole<string>>(entity => entity.ToTable("aspnet_user_roles"));
        modelBuilder.Entity<IdentityUserClaim<string>>(entity => entity.ToTable("aspnet_user_claims"));
        modelBuilder.Entity<IdentityRoleClaim<string>>(entity => entity.ToTable("aspnet_role_claims"));
        modelBuilder.Entity<IdentityUserLogin<string>>(entity => entity.ToTable("aspnet_user_logins"));
        modelBuilder.Entity<IdentityUserToken<string>>(entity => entity.ToTable("aspnet_user_tokens"));

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
    entity.ToTable("activitycompletions");

    entity.Property(e => e.Id).HasColumnName("id");
    entity.Property(e => e.UserId).HasColumnName("userid");
    entity.Property(e => e.ActivityId).HasColumnName("activityid");
    entity.Property(e => e.CompletedAt).HasColumnName("completedat");
    entity.Property(e => e.PointsEarned).HasColumnName("pointsearned");

    // 👇 important explicit mappings
    entity.Property(e => e.ImagePath).HasColumnName("imagepath");
    entity.Property(e => e.Notes).HasColumnName("notes");
    entity.Property(e => e.ReviewStatus).HasColumnName("reviewstatus");
    entity.Property(e => e.Quantity).HasColumnName("quantity");
    entity.Property(e => e.AdminNotes).HasColumnName("adminnotes");
    entity.Property(e => e.Co2eReduction).HasColumnName("co2e_reduction");
});

        modelBuilder.Entity<SustainabilityEvent>().ToTable("sustainabilityevents");

        modelBuilder.Entity<SustainabilityEvent>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Location).HasColumnName("location");
            entity.Property(e => e.StartDate).HasColumnName("startdate");
            entity.Property(e => e.EndDate).HasColumnName("enddate");
            entity.Property(e => e.RegistrationLink).HasColumnName("registrationlink");
            entity.Property(e => e.ImageUrl).HasColumnName("imageurl");
            entity.Property(e => e.Organizer).HasColumnName("organizer");
            entity.Property(e => e.MaxAttendees).HasColumnName("maxattendees");
            entity.Property(e => e.Category).HasColumnName("category");
            entity.Property(e => e.IsVirtual).HasColumnName("isvirtual");
            entity.Property(e => e.CreatedAt).HasColumnName("createdat");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedat");
        });

        modelBuilder.Entity<UserChallenge>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("userid");
            entity.Property(e => e.ChallengeId).HasColumnName("challengeid");
            entity.Property(e => e.JoinedDate).HasColumnName("joindate");
            entity.Property(e => e.Completed).HasColumnName("completed");
            entity.Property(e => e.CompletedAt).HasColumnName("completedat");
            entity.Property(e => e.Progress).HasColumnName("progress");
            entity.Property(e => e.Status).HasColumnName("status");

            // Ignore the IsCompleted property since it's just an alias
            entity.Ignore(u => u.IsCompleted);
            // Ignore the CompletedDate property since it's just an alias
            entity.Ignore(u => u.CompletedDate);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
{
    entity.ToTable("refreshtokens");

    entity.Property(e => e.Id).HasColumnName("id");
    entity.Property(e => e.UserId).HasColumnName("userid");
    entity.Property(e => e.Token).HasColumnName("token");
    entity.Property(e => e.Expires).HasColumnName("expires");
    entity.Property(e => e.IsRevoked).HasColumnName("isrevoked");
    entity.Property(e => e.Created).HasColumnName("created");

    entity.HasOne(e => e.User)
        .WithMany()
        .HasForeignKey(e => e.UserId)
        .OnDelete(DeleteBehavior.Cascade);
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
            .Ignore(c => c.Activities);

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