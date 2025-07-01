using Microsoft.EntityFrameworkCore;
using WorkoutGamifier.Domain.Entities;

namespace WorkoutGamifier.Infrastructure.Data;

public class WorkoutGamifierDbContext : DbContext
{
    public WorkoutGamifierDbContext(DbContextOptions<WorkoutGamifierDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Workout> Workouts { get; set; }
    public DbSet<WorkoutPool> WorkoutPools { get; set; }
    public DbSet<WorkoutPoolWorkout> WorkoutPoolWorkouts { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<UserAction> UserActions { get; set; }
    public DbSet<SessionAction> SessionActions { get; set; }
    public DbSet<SessionWorkout> SessionWorkouts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // Configure Workout entity
        modelBuilder.Entity<Workout>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Instructions).HasMaxLength(2000);
        });

        // Configure WorkoutPool entity
        modelBuilder.Entity<WorkoutPool>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.WorkoutPools)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure WorkoutPoolWorkout entity (many-to-many junction)
        modelBuilder.Entity<WorkoutPoolWorkout>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.WorkoutPool)
                .WithMany(wp => wp.WorkoutPoolWorkouts)
                .HasForeignKey(e => e.WorkoutPoolId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Workout)
                .WithMany(w => w.WorkoutPoolWorkouts)
                .HasForeignKey(e => e.WorkoutId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.WorkoutPoolId, e.WorkoutId }).IsUnique();
        });

        // Configure Session entity
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.WorkoutPool)
                .WithMany(wp => wp.Sessions)
                .HasForeignKey(e => e.WorkoutPoolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure UserAction entity
        modelBuilder.Entity<UserAction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserActions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure SessionAction entity
        modelBuilder.Entity<SessionAction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            
            entity.HasOne(e => e.Session)
                .WithMany(s => s.SessionActions)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.UserAction)
                .WithMany(ua => ua.SessionActions)
                .HasForeignKey(e => e.UserActionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure SessionWorkout entity
        modelBuilder.Entity<SessionWorkout>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            
            entity.HasOne(e => e.Session)
                .WithMany(s => s.SessionWorkouts)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Workout)
                .WithMany(w => w.SessionWorkouts)
                .HasForeignKey(e => e.WorkoutId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure base entity properties for all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.CreatedAt))
                    .HasDefaultValueSql("GETUTCDATE()");
                    
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.UpdatedAt))
                    .HasDefaultValueSql("GETUTCDATE()");
            }
        }
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
    }
}