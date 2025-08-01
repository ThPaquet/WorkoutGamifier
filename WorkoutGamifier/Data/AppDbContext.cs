using Microsoft.EntityFrameworkCore;
using WorkoutGamifier.Models;

namespace WorkoutGamifier.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Workout> Workouts { get; set; }
    public DbSet<WorkoutPool> WorkoutPools { get; set; }
    public DbSet<WorkoutPoolWorkout> WorkoutPoolWorkouts { get; set; }
    public DbSet<Models.Action> Actions { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<ActionCompletion> ActionCompletions { get; set; }
    public DbSet<WorkoutReceived> WorkoutReceived { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Workout entity
        modelBuilder.Entity<Workout>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Instructions).HasMaxLength(2000);
            entity.Property(e => e.DurationMinutes).IsRequired();
            entity.Property(e => e.Difficulty).IsRequired();
            entity.Property(e => e.IsPreloaded).IsRequired();
            entity.Property(e => e.IsHidden).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });

        // Configure WorkoutPool entity
        modelBuilder.Entity<WorkoutPool>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });

        // Configure WorkoutPoolWorkout many-to-many relationship
        modelBuilder.Entity<WorkoutPoolWorkout>(entity =>
        {
            entity.HasKey(e => new { e.WorkoutPoolId, e.WorkoutId });
            
            entity.HasOne(e => e.WorkoutPool)
                .WithMany(e => e.WorkoutPoolWorkouts)
                .HasForeignKey(e => e.WorkoutPoolId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Workout)
                .WithMany(e => e.WorkoutPoolWorkouts)
                .HasForeignKey(e => e.WorkoutId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Action entity
        modelBuilder.Entity<Models.Action>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PointValue).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });

        // Configure Session entity
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.WorkoutPoolId).IsRequired();
            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.PointsEarned).IsRequired();
            entity.Property(e => e.PointsSpent).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.WorkoutPool)
                .WithMany(e => e.Sessions)
                .HasForeignKey(e => e.WorkoutPoolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ActionCompletion entity
        modelBuilder.Entity<ActionCompletion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired();
            entity.Property(e => e.ActionId).IsRequired();
            entity.Property(e => e.CompletedAt).IsRequired();
            entity.Property(e => e.PointsAwarded).IsRequired();

            entity.HasOne(e => e.Session)
                .WithMany(e => e.ActionCompletions)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Action)
                .WithMany(e => e.ActionCompletions)
                .HasForeignKey(e => e.ActionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure WorkoutReceived entity
        modelBuilder.Entity<WorkoutReceived>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired();
            entity.Property(e => e.WorkoutId).IsRequired();
            entity.Property(e => e.ReceivedAt).IsRequired();
            entity.Property(e => e.PointsSpent).IsRequired();

            entity.HasOne(e => e.Session)
                .WithMany(e => e.WorkoutReceived)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Workout)
                .WithMany(e => e.WorkoutReceived)
                .HasForeignKey(e => e.WorkoutId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Workout workout)
            {
                if (entry.State == EntityState.Added)
                    workout.CreatedAt = DateTime.UtcNow;
                workout.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is WorkoutPool pool)
            {
                if (entry.State == EntityState.Added)
                    pool.CreatedAt = DateTime.UtcNow;
                pool.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Models.Action action)
            {
                if (entry.State == EntityState.Added)
                    action.CreatedAt = DateTime.UtcNow;
                action.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Session session)
            {
                if (entry.State == EntityState.Added)
                    session.CreatedAt = DateTime.UtcNow;
                session.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}