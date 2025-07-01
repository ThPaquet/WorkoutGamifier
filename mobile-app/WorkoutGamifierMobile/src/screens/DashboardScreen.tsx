import React, { useState, useEffect, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  Alert,
  RefreshControl,
} from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import { useAuth } from '../services/AuthContext';
import dataService from '../services/dataService';
import { Session, WorkoutPool, SessionStatus } from '../types';

interface Props {
  navigation: any;
}

const DashboardScreen: React.FC<Props> = ({ navigation }) => {
  const { user } = useAuth();
  const [sessions, setSessions] = useState<Session[]>([]);
  const [workoutPools, setWorkoutPools] = useState<WorkoutPool[]>([]);
  const [currentSession, setCurrentSession] = useState<Session | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const loadData = async () => {
    try {
      const [sessionsData, poolsData, currentSessionData] = await Promise.all([
        dataService.getSessions(),
        dataService.getWorkoutPools(),
        dataService.getCurrentSession(),
      ]);

      setSessions(sessionsData.slice(-5)); // Show last 5 sessions
      setWorkoutPools(poolsData);
      setCurrentSession(currentSessionData);
    } catch (error) {
      console.error('Error loading dashboard data:', error);
      Alert.alert('Error', 'Failed to load dashboard data');
    } finally {
      setIsLoading(false);
      setRefreshing(false);
    }
  };

  useFocusEffect(
    useCallback(() => {
      loadData();
    }, [])
  );

  const onRefresh = () => {
    setRefreshing(true);
    loadData();
  };

  const handleCreateSession = () => {
    if (workoutPools.length === 0) {
      Alert.alert(
        'No Workout Pools',
        'You need to create workout pools first. Go to the Pools tab to create one.',
        [{ text: 'OK' }]
      );
      return;
    }

    Alert.alert(
      'Create New Session',
      'Choose a workout pool for your session:',
      [
        ...workoutPools.map(pool => ({
          text: pool.name,
          onPress: () => createSession(pool),
        })),
        { text: 'Cancel', style: 'cancel' },
      ]
    );
  };

  const createSession = async (pool: WorkoutPool) => {
    try {
      const sessionData = {
        name: `${pool.name} Session`,
        description: `Workout session using ${pool.name} pool`,
        workoutPoolId: pool.id,
      };

      const session = await dataService.createSession(sessionData);
      setCurrentSession(session);
      navigation.navigate('Session', { sessionId: session.id });
    } catch (error) {
      console.error('Error creating session:', error);
      Alert.alert('Error', 'Failed to create session');
    }
  };

  const handleContinueSession = () => {
    if (currentSession) {
      navigation.navigate('Session', { sessionId: currentSession.id });
    }
  };

  const getStatusColor = (status: SessionStatus) => {
    switch (status) {
      case SessionStatus.Active:
        return '#10b981';
      case SessionStatus.Completed:
        return '#6366f1';
      case SessionStatus.Paused:
        return '#f59e0b';
      default:
        return '#6b7280';
    }
  };

  const getStatusText = (status: SessionStatus) => {
    switch (status) {
      case SessionStatus.Active:
        return 'Active';
      case SessionStatus.Completed:
        return 'Completed';
      case SessionStatus.Paused:
        return 'Paused';
      default:
        return 'Unknown';
    }
  };

  if (isLoading) {
    return (
      <View style={styles.loadingContainer}>
        <Text>Loading...</Text>
      </View>
    );
  }

  return (
    <ScrollView
      style={styles.container}
      refreshControl={
        <RefreshControl refreshing={refreshing} onRefresh={onRefresh} />
      }
    >
      {/* Header */}
      <View style={styles.header}>
        <Text style={styles.title}>Welcome back!</Text>
        <Text style={styles.subtitle}>
          {user?.username} • {user?.totalPoints || 0} total points
        </Text>
      </View>

      {/* Current Session Card */}
      {currentSession ? (
        <View style={styles.card}>
          <Text style={styles.cardTitle}>🎯 Active Session</Text>
          <Text style={styles.sessionName}>{currentSession.name}</Text>
          <View style={styles.sessionStats}>
            <Text style={styles.statText}>
              Points: {currentSession.currentPoints}
            </Text>
            <Text style={styles.statText}>
              Earned: {currentSession.totalPointsEarned}
            </Text>
            <Text style={styles.statText}>
              Spent: {currentSession.totalPointsSpent}
            </Text>
          </View>
          <TouchableOpacity
            style={styles.primaryButton}
            onPress={handleContinueSession}
          >
            <Text style={styles.primaryButtonText}>Continue Session</Text>
          </TouchableOpacity>
        </View>
      ) : (
        <View style={styles.card}>
          <Text style={styles.cardTitle}>🚀 Ready to Start?</Text>
          <Text style={styles.cardDescription}>
            Create a new workout session to start earning points and getting random workouts!
          </Text>
          <TouchableOpacity
            style={styles.primaryButton}
            onPress={handleCreateSession}
          >
            <Text style={styles.primaryButtonText}>Start New Session</Text>
          </TouchableOpacity>
        </View>
      )}

      {/* Quick Stats */}
      <View style={styles.statsGrid}>
        <View style={styles.statCard}>
          <Text style={styles.statNumber}>{sessions.length}</Text>
          <Text style={styles.statLabel}>Total Sessions</Text>
        </View>
        <View style={styles.statCard}>
          <Text style={styles.statNumber}>{workoutPools.length}</Text>
          <Text style={styles.statLabel}>Workout Pools</Text>
        </View>
        <View style={styles.statCard}>
          <Text style={styles.statNumber}>
            {sessions.filter(s => s.status === SessionStatus.Completed).length}
          </Text>
          <Text style={styles.statLabel}>Completed</Text>
        </View>
      </View>

      {/* Recent Sessions */}
      <View style={styles.section}>
        <Text style={styles.sectionTitle}>Recent Sessions</Text>
        {sessions.length === 0 ? (
          <Text style={styles.emptyText}>No sessions yet. Start your first session!</Text>
        ) : (
          sessions.map(session => (
            <View key={session.id} style={styles.sessionCard}>
              <View style={styles.sessionHeader}>
                <Text style={styles.sessionCardTitle}>{session.name}</Text>
                <View style={[styles.statusBadge, { backgroundColor: getStatusColor(session.status) }]}>
                  <Text style={styles.statusText}>{getStatusText(session.status)}</Text>
                </View>
              </View>
              <Text style={styles.sessionPool}>Pool: {session.workoutPoolName}</Text>
              <View style={styles.sessionCardStats}>
                <Text style={styles.sessionStat}>Points: {session.currentPoints}</Text>
                <Text style={styles.sessionStat}>Earned: {session.totalPointsEarned}</Text>
                <Text style={styles.sessionStat}>Spent: {session.totalPointsSpent}</Text>
              </View>
            </View>
          ))
        )}
      </View>
    </ScrollView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f8fafc',
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#f8fafc',
  },
  header: {
    padding: 20,
    paddingTop: 60,
  },
  title: {
    fontSize: 28,
    fontWeight: 'bold',
    color: '#1f2937',
    marginBottom: 4,
  },
  subtitle: {
    fontSize: 16,
    color: '#6b7280',
  },
  card: {
    backgroundColor: '#fff',
    marginHorizontal: 20,
    marginBottom: 20,
    borderRadius: 12,
    padding: 20,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 3,
  },
  cardTitle: {
    fontSize: 18,
    fontWeight: '600',
    color: '#1f2937',
    marginBottom: 8,
  },
  cardDescription: {
    fontSize: 14,
    color: '#6b7280',
    marginBottom: 16,
    lineHeight: 20,
  },
  sessionName: {
    fontSize: 16,
    fontWeight: '500',
    color: '#374151',
    marginBottom: 12,
  },
  sessionStats: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginBottom: 16,
  },
  statText: {
    fontSize: 14,
    color: '#6b7280',
  },
  primaryButton: {
    backgroundColor: '#6366f1',
    borderRadius: 8,
    paddingVertical: 12,
    alignItems: 'center',
  },
  primaryButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
  },
  statsGrid: {
    flexDirection: 'row',
    marginHorizontal: 20,
    marginBottom: 20,
    gap: 12,
  },
  statCard: {
    flex: 1,
    backgroundColor: '#fff',
    borderRadius: 8,
    padding: 16,
    alignItems: 'center',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 2,
    elevation: 1,
  },
  statNumber: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#6366f1',
    marginBottom: 4,
  },
  statLabel: {
    fontSize: 12,
    color: '#6b7280',
    textAlign: 'center',
  },
  section: {
    marginHorizontal: 20,
    marginBottom: 20,
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: '600',
    color: '#1f2937',
    marginBottom: 12,
  },
  emptyText: {
    fontSize: 14,
    color: '#6b7280',
    textAlign: 'center',
    fontStyle: 'italic',
  },
  sessionCard: {
    backgroundColor: '#fff',
    borderRadius: 8,
    padding: 16,
    marginBottom: 12,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 2,
    elevation: 1,
  },
  sessionHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8,
  },
  sessionCardTitle: {
    fontSize: 16,
    fontWeight: '500',
    color: '#1f2937',
    flex: 1,
  },
  statusBadge: {
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 12,
  },
  statusText: {
    fontSize: 12,
    color: '#fff',
    fontWeight: '500',
  },
  sessionPool: {
    fontSize: 14,
    color: '#6b7280',
    marginBottom: 8,
  },
  sessionCardStats: {
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  sessionStat: {
    fontSize: 12,
    color: '#6b7280',
  },
});

export default DashboardScreen;