namespace WorkoutGamifier.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int TotalPoints { get; set; } = 0;
    
    // Navigation properties
    public virtual ICollection<WorkoutPool> WorkoutPools { get; set; } = new List<WorkoutPool>();
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    public virtual ICollection<UserAction> UserActions { get; set; } = new List<UserAction>();
}