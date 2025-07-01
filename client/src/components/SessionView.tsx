import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { apiService } from '../services/api';
import { Session, SessionStats, SessionStatus, Workout, UserAction } from '../types';

const SessionView: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [session, setSession] = useState<Session | null>(null);
  const [stats, setStats] = useState<SessionStats | null>(null);
  const [userActions, setUserActions] = useState<UserAction[]>([]);
  const [currentWorkout, setCurrentWorkout] = useState<Workout | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [actionLoading, setActionLoading] = useState(false);

  useEffect(() => {
    if (id) {
      loadSessionData();
    }
  }, [id]);

  const loadSessionData = async () => {
    if (!id) return;
    
    try {
      setLoading(true);
      const [sessionData, statsData, actionsData] = await Promise.all([
        apiService.getSession(id),
        apiService.getSessionStats(id),
        apiService.getUserActions()
      ]);
      
      setSession(sessionData);
      setStats(statsData);
      setUserActions(actionsData);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load session data');
    } finally {
      setLoading(false);
    }
  };

  const handleCompleteAction = async (actionId: string) => {
    if (!id) return;
    
    try {
      setActionLoading(true);
      const updatedSession = await apiService.completeAction(id, {
        userActionId: actionId
      });
      setSession(updatedSession);
      
      // Reload stats
      const updatedStats = await apiService.getSessionStats(id);
      setStats(updatedStats);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to complete action');
    } finally {
      setActionLoading(false);
    }
  };

  const handleGetRandomWorkout = async () => {
    if (!id) return;
    
    try {
      setActionLoading(true);
      const workout = await apiService.getRandomWorkout(id, 1);
      setCurrentWorkout(workout);
      
      // Reload session data
      const updatedSession = await apiService.getSession(id);
      setSession(updatedSession);
      
      const updatedStats = await apiService.getSessionStats(id);
      setStats(updatedStats);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to get random workout');
    } finally {
      setActionLoading(false);
    }
  };

  const handleCompleteWorkout = async () => {
    if (!id || !currentWorkout) return;
    
    try {
      setActionLoading(true);
      await apiService.completeWorkout(id, currentWorkout.id);
      setCurrentWorkout(null);
      
      // Reload session data
      const updatedSession = await apiService.getSession(id);
      setSession(updatedSession);
      
      const updatedStats = await apiService.getSessionStats(id);
      setStats(updatedStats);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to complete workout');
    } finally {
      setActionLoading(false);
    }
  };

  const handleEndSession = async () => {
    if (!id) return;
    
    if (!window.confirm('Are you sure you want to end this session?')) {
      return;
    }
    
    try {
      setActionLoading(true);
      await apiService.endSession(id);
      
      // Reload session data
      const updatedSession = await apiService.getSession(id);
      setSession(updatedSession);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to end session');
    } finally {
      setActionLoading(false);
    }
  };

  const formatDuration = (duration: string): string => {
    // Assuming duration is in format like "PT1H30M" or similar
    return duration;
  };

  const getDifficultyStars = (difficulty: number): string => {
    return '⭐'.repeat(Math.min(difficulty, 5));
  };

  if (loading) {
    return <div className="loading">Loading session...</div>;
  }

  if (!session) {
    return (
      <div className="error">
        Session not found
      </div>
    );
  }

  const isActive = session.status === SessionStatus.Active;

  return (
    <div className="fade-in">
      <div className="flex items-center gap-2 mb-2">
        <Link to="/" className="btn btn-secondary" style={{ width: 'auto' }}>
          ← Back to Dashboard
        </Link>
      </div>

      {error && (
        <div className="error">
          {error}
        </div>
      )}

      <div className="card">
        <div className="card-header">
          <div>
            <h2 className="card-title">{session.name}</h2>
            <p className="card-subtitle">{session.description}</p>
          </div>
          <span className={`status-badge ${isActive ? 'status-active' : 'status-completed'}`}>
            {isActive ? 'Active' : 'Completed'}
          </span>
        </div>

        {stats && (
          <div className="stats-grid">
            <div className="stat-item">
              <span className="stat-value">{stats.currentPoints}</span>
              <span className="stat-label">Current Points</span>
            </div>
            <div className="stat-item">
              <span className="stat-value">{stats.totalPointsEarned}</span>
              <span className="stat-label">Points Earned</span>
            </div>
            <div className="stat-item">
              <span className="stat-value">{stats.totalPointsSpent}</span>
              <span className="stat-label">Points Spent</span>
            </div>
            <div className="stat-item">
              <span className="stat-value">{stats.actionsCompleted}</span>
              <span className="stat-label">Actions Done</span>
            </div>
          </div>
        )}

        {isActive && (
          <button 
            className="btn btn-danger"
            onClick={handleEndSession}
            disabled={actionLoading}
          >
            🛑 End Session
          </button>
        )}
      </div>

      {currentWorkout && (
        <div className="card workout-card">
          <div className="card-header">
            <h3 className="card-title">Current Workout</h3>
          </div>
          
          <h4>{currentWorkout.name}</h4>
          <p className="text-muted mb-1">{currentWorkout.description}</p>
          
          <div className="workout-difficulty">
            <span>Difficulty:</span>
            <span className="difficulty-stars">
              {getDifficultyStars(currentWorkout.difficulty)}
            </span>
            <span className="text-muted">({currentWorkout.difficulty}/10)</span>
          </div>
          
          <div className="mt-1">
            <strong>Duration:</strong> {currentWorkout.durationMinutes} minutes
          </div>
          
          <div className="mt-1">
            <strong>Category:</strong> {currentWorkout.category}
          </div>
          
          {currentWorkout.instructions && (
            <div className="mt-2">
              <strong>Instructions:</strong>
              <p className="mt-1">{currentWorkout.instructions}</p>
            </div>
          )}
          
          <button 
            className="btn btn-success"
            onClick={handleCompleteWorkout}
            disabled={actionLoading}
            style={{ marginTop: '1rem' }}
          >
            ✅ Mark as Completed
          </button>
        </div>
      )}

      {isActive && (
        <>
          <div className="card">
            <div className="card-header">
              <h3 className="card-title">Complete Actions</h3>
            </div>
            
            {userActions.length === 0 ? (
              <p className="text-muted">No actions available. Create some actions first!</p>
            ) : (
              <div className="flex flex-col gap-1">
                {userActions.map((action) => (
                  <div key={action.id} className="card" style={{ marginBottom: 0 }}>
                    <div className="flex justify-between items-center">
                      <div>
                        <strong>{action.description}</strong>
                        <p className="text-muted" style={{ fontSize: '0.9rem' }}>
                          Reward: {action.pointReward} points
                        </p>
                      </div>
                      <button
                        className="btn btn-success"
                        onClick={() => handleCompleteAction(action.id)}
                        disabled={actionLoading}
                        style={{ width: 'auto' }}
                      >
                        +{action.pointReward}
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          <div className="card">
            <div className="card-header">
              <h3 className="card-title">Get Workout</h3>
            </div>
            
            <p className="text-muted mb-2">
              Spend 1 point to get a random workout from your selected pool.
            </p>
            
            <button
              className="btn"
              onClick={handleGetRandomWorkout}
              disabled={actionLoading || (session.currentPoints < 1)}
            >
              {session.currentPoints < 1 ? 
                '🚫 Not enough points' : 
                '🎲 Get Random Workout (1 point)'
              }
            </button>
          </div>
        </>
      )}
    </div>
  );
};

export default SessionView;