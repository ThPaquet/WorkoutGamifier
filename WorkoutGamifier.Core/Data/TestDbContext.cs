using Microsoft.EntityFrameworkCore;
using WorkoutGamifier.Core.Models;

namespace WorkoutGamifier.Core.Data;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<Workout> Workouts { get; set; }
    public DbSet<WorkoutPool> WorkoutPools { get; set; }
    public DbSet<Models.Action> Actions { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<ActionCompletion> ActionCompletions { get; set; }
    public DbSet<WorkoutReceived> WorkoutReceived { get; set; }
    public DbSet<WorkoutPoolWorkout> WorkoutPoolWorkouts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure WorkoutPoolWorkout many-to-many relationship
        modelBuilder.Entity<WorkoutPoolWorkout>()
            .HasKey(wpw => new { wpw.WorkoutPoolId, wpw.WorkoutId });

        modelBuilder.Entity<WorkoutPoolWorkout>()
            .HasOne(wpw => wpw.WorkoutPool)
            .WithMany(wp => wp.WorkoutPoolWorkouts)
            .HasForeignKey(wpw => wpw.WorkoutPoolId);

        modelBuilder.Entity<WorkoutPoolWorkout>()
            .HasOne(wpw => wpw.Workout)
            .WithMany()
            .HasForeignKey(wpw => wpw.WorkoutId);

        // Configure ActionCompletion relationships
        modelBuilder.Entity<ActionCompletion>()
            .HasOne(ac => ac.Session)
            .WithMany()
            .HasForeignKey(ac => ac.SessionId);

        modelBuilder.Entity<ActionCompletion>()
            .HasOne(ac => ac.Action)
            .WithMany()
            .HasForeignKey(ac => ac.ActionId);

        // Configure WorkoutReceived relationships
        modelBuilder.Entity<WorkoutReceived>()
            .HasOne(wr => wr.Session)
            .WithMany()
            .HasForeignKey(wr => wr.SessionId);

        modelBuilder.Entity<WorkoutReceived>()
            .HasOne(wr => wr.Workout)
            .WithMany()
            .HasForeignKey(wr => wr.WorkoutId);

        // Configure Session relationships
        modelBuilder.Entity<Session>()
            .HasOne<WorkoutPool>()
            .WithMany()
            .HasForeignKey(s => s.WorkoutPoolId);
    }
}