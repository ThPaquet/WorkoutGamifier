using WorkoutGamifier.Core.Models;

namespace WorkoutGamifier.Core.Services;

public class PointCalculator
{
    public int CalculateSessionBalance(Session session)
    {
        return session.PointsEarned - session.PointsSpent;
    }

    public bool CanAffordWorkout(Session session, int workoutCost)
    {
        return CalculateSessionBalance(session) >= workoutCost;
    }

    public Session AddPointsToSession(Session session, int points)
    {
        if (points <= 0)
            throw new ArgumentException("Points must be positive", nameof(points));

        session.PointsEarned += points;
        session.UpdatedAt = DateTime.UtcNow;
        return session;
    }

    public Session SpendPointsFromSession(Session session, int points)
    {
        if (points <= 0)
            throw new ArgumentException("Points must be positive", nameof(points));

        if (!CanAffordWorkout(session, points))
            throw new InvalidOperationException($"Insufficient points. Current balance: {CalculateSessionBalance(session)}, Required: {points}");

        session.PointsSpent += points;
        session.UpdatedAt = DateTime.UtcNow;
        return session;
    }

    public TimeSpan CalculateSessionDuration(Session session)
    {
        if (session.EndTime.HasValue)
        {
            return session.EndTime.Value - session.StartTime;
        }
        else
        {
            return DateTime.UtcNow - session.StartTime;
        }
    }

    public double CalculateAverageSessionDuration(IEnumerable<Session> sessions)
    {
        var completedSessions = sessions.Where(s => s.Status == SessionStatus.Completed && s.EndTime.HasValue).ToList();
        
        if (!completedSessions.Any())
            return 0;

        var totalMinutes = completedSessions.Sum(s => (s.EndTime!.Value - s.StartTime).TotalMinutes);
        return totalMinutes / completedSessions.Count;
    }
}