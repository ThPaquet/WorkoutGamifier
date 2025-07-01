import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { apiService } from '../services/api';
import { Session, SessionStatus, User } from '../types';

const Dashboard: React.FC = () => {
  const [sessions, setSessions] = useState<Session[]>([]);
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const userStr = localStorage.getItem('user');
      if (userStr) {
        setUser(JSON.parse(userStr));
      }
      
      const sessionsData = await apiService.getSessions();
      setSessions(sessionsData);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  const getStatusBadgeClass = (status: SessionStatus): string => {
    switch (status) {
      case SessionStatus.Active:
        return 'status-badge status-active';
      case SessionStatus.Completed:
        return 'status-badge status-completed';
      case SessionStatus.Paused:
        return 'status-badge status-paused';
      case SessionStatus.Cancelled:
        return 'status-badge status-cancelled';
      default:
        return 'status-badge';
    }
  };

  const getStatusText = (status: SessionStatus): string => {
    switch (status) {
      case SessionStatus.Active:
        return 'Active';
      case SessionStatus.Completed:
        return 'Completed';
      case SessionStatus.Paused:
        return 'Paused';
      case SessionStatus.Cancelled:
        return 'Cancelled';
      default:
        return 'Unknown';
    }
  };

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  if (loading) {
    return <div className="loading">Loading...</div>;
  }

  return (
    <div className="fade-in">
      {error && (
        <div className="error">
          {error}
        </div>
      )}

      {user && (
        <div className="card">
          <div className="card-header">
            <h2 className="card-title">Welcome back, {user.username}!</h2>
          </div>
          <div className="stats-grid">
            <div className="stat-item">
              <span className="stat-value">{user.totalPoints}</span>
              <span className="stat-label">Total Points</span>
            </div>
            <div className="stat-item">
              <span className="stat-value">{sessions.length}</span>
              <span className="stat-label">Total Sessions</span>
            </div>
            <div className="stat-item">
              <span className="stat-value">
                {sessions.filter(s => s.status === SessionStatus.Active).length}
              </span>
              <span className="stat-label">Active Sessions</span>
            </div>
            <div className="stat-item">
              <span className="stat-value">
                {sessions.filter(s => s.status === SessionStatus.Completed).length}
              </span>
              <span className="stat-label">Completed</span>
            </div>
          </div>
        </div>
      )}

      <div className="card">
        <div className="card-header">
          <h3 className="card-title">Quick Actions</h3>
        </div>
        <div className="flex flex-col gap-1">
          <button className="btn">
            ➕ Create New Session
          </button>
          <button className="btn btn-secondary">
            🏋️ Manage Workout Pools
          </button>
          <button className="btn btn-secondary">
            ⚡ Manage Actions
          </button>
        </div>
      </div>

      <div className="card">
        <div className="card-header">
          <h3 className="card-title">Recent Sessions</h3>
        </div>

        {sessions.length === 0 ? (
          <div className="text-center text-muted">
            <p>No sessions yet. Create your first session to get started!</p>
          </div>
        ) : (
          <div className="flex flex-col gap-1">
            {sessions.slice(0, 5).map((session) => (
              <div key={session.id} className="card" style={{ marginBottom: 0 }}>
                <div className="card-header">
                  <div>
                    <h4 className="card-title" style={{ fontSize: '1rem', marginBottom: '0.25rem' }}>
                      {session.name}
                    </h4>
                    <p className="card-subtitle">
                      {session.workoutPoolName} • {formatDate(session.startTime)}
                    </p>
                  </div>
                  <span className={getStatusBadgeClass(session.status)}>
                    {getStatusText(session.status)}
                  </span>
                </div>

                <div className="stats-grid" style={{ gridTemplateColumns: 'repeat(3, 1fr)' }}>
                  <div className="stat-item">
                    <span className="stat-value">{session.currentPoints}</span>
                    <span className="stat-label">Current Points</span>
                  </div>
                  <div className="stat-item">
                    <span className="stat-value">{session.totalPointsEarned}</span>
                    <span className="stat-label">Earned</span>
                  </div>
                  <div className="stat-item">
                    <span className="stat-value">{session.totalPointsSpent}</span>
                    <span className="stat-label">Spent</span>
                  </div>
                </div>

                <Link 
                  to={`/session/${session.id}`} 
                  className="btn"
                  style={{ textDecoration: 'none', marginTop: '1rem' }}
                >
                  {session.status === SessionStatus.Active ? '▶️ Continue Session' : '👁️ View Session'}
                </Link>
              </div>
            ))}

            {sessions.length > 5 && (
              <p className="text-center text-muted mt-2">
                And {sessions.length - 5} more sessions...
              </p>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default Dashboard;