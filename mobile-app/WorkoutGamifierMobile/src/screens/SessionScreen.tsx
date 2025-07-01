import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  Alert,
} from 'react-native';
import { RouteProp } from '@react-navigation/native';
import { StackNavigationProp } from '@react-navigation/stack';
import { RootStackParamList } from '../navigation/AppNavigator';
import dataService from '../services/dataService';
import { Session, UserAction, Workout } from '../types';

type SessionScreenRouteProp = RouteProp<RootStackParamList, 'Session'>;
type SessionScreenNavigationProp = StackNavigationProp<RootStackParamList, 'Session'>;

interface Props {
  route: SessionScreenRouteProp;
  navigation: SessionScreenNavigationProp;
}

const SessionScreen: React.FC<Props> = ({ route, navigation }) => {
  const { sessionId } = route.params;
  const [session, setSession] = useState<Session | null>(null);
  const [userActions, setUserActions] = useState<UserAction[]>([]);
  const [currentWorkout, setCurrentWorkout] = useState<Workout | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    loadSessionData();
  }, [sessionId]);

  const loadSessionData = async () => {
    try {
      const [sessionData, actionsData] = await Promise.all([
        dataService.getSession(sessionId),
        dataService.getUserActions(),
      ]);

      setSession(sessionData);
      setUserActions(actionsData);
    } catch (error) {
      console.error('Error loading session data:', error);
      Alert.alert('Error', 'Failed to load session data');
    } finally {
      setIsLoading(false);
    }
  };

  const handleCompleteAction = async (action: UserAction) => {
    if (!session) return;

    try {
      const updatedSession = await dataService.completeAction(session.id, {
        userActionId: action.id,
      });
      setSession(updatedSession);
      Alert.alert(
        'Action Completed! 🎉',
        `You earned ${action.pointReward} points for: ${action.description}`
      );
    } catch (error) {
      console.error('Error completing action:', error);
      Alert.alert('Error', 'Failed to complete action');
    }
  };

  const handleGetRandomWorkout = async () => {
    if (!session) return;

    if (session.currentPoints < 1) {
      Alert.alert(
        'Insufficient Points',
        'You need at least 1 point to get a random workout. Complete some actions first!'
      );
      return;
    }

    try {
      const workout = await dataService.getRandomWorkout(session.id, 1);
      setCurrentWorkout(workout);
      
      // Refresh session data to show updated points
      const updatedSession = await dataService.getSession(session.id);
      setSession(updatedSession);
      
      Alert.alert(
        'New Workout! 💪',
        `You got: ${workout.name}\nDifficulty: ${workout.difficulty}/10\nDuration: ${workout.durationMinutes} minutes`
      );
         } catch (error: any) {
       console.error('Error getting random workout:', error);
       Alert.alert('Error', error.message || 'Failed to get random workout');
     }
  };

  const handleEndSession = () => {
    Alert.alert(
      'End Session',
      'Are you sure you want to end this session?',
      [
        { text: 'Cancel', style: 'cancel' },
        {
          text: 'End Session',
          style: 'destructive',
          onPress: async () => {
            try {
              await dataService.endSession(sessionId);
              navigation.goBack();
            } catch (error) {
              console.error('Error ending session:', error);
              Alert.alert('Error', 'Failed to end session');
            }
          },
        },
      ]
    );
  };

  if (isLoading || !session) {
    return (
      <View style={styles.loadingContainer}>
        <Text>Loading session...</Text>
      </View>
    );
  }

  return (
    <ScrollView style={styles.container}>
      {/* Session Header */}
      <View style={styles.header}>
        <Text style={styles.sessionName}>{session.name}</Text>
        <Text style={styles.sessionDescription}>{session.description}</Text>
        
        <View style={styles.pointsContainer}>
          <Text style={styles.pointsText}>Current Points: {session.currentPoints}</Text>
          <Text style={styles.statsText}>
            Earned: {session.totalPointsEarned} | Spent: {session.totalPointsSpent}
          </Text>
        </View>
      </View>

      {/* Current Workout */}
      {currentWorkout && (
        <View style={styles.workoutCard}>
          <Text style={styles.workoutTitle}>🏋️ Current Workout</Text>
          <Text style={styles.workoutName}>{currentWorkout.name}</Text>
          <Text style={styles.workoutDescription}>{currentWorkout.description}</Text>
          <Text style={styles.workoutDetails}>
            Difficulty: {currentWorkout.difficulty}/10 • Duration: {currentWorkout.durationMinutes} min
          </Text>
          <Text style={styles.workoutInstructions}>{currentWorkout.instructions}</Text>
          <TouchableOpacity
            style={styles.completeWorkoutButton}
            onPress={() => {
              setCurrentWorkout(null);
              Alert.alert('Workout Completed! 🎉', 'Great job! Ready for the next one?');
            }}
          >
            <Text style={styles.completeWorkoutButtonText}>Mark as Completed</Text>
          </TouchableOpacity>
        </View>
      )}

      {/* Get Random Workout Button */}
      <View style={styles.section}>
        <TouchableOpacity
          style={[
            styles.randomWorkoutButton,
            session.currentPoints < 1 && styles.disabledButton
          ]}
          onPress={handleGetRandomWorkout}
          disabled={session.currentPoints < 1}
        >
          <Text style={styles.randomWorkoutButtonText}>
            🎲 Get Random Workout (1 point)
          </Text>
        </TouchableOpacity>
      </View>

      {/* Actions */}
      <View style={styles.section}>
        <Text style={styles.sectionTitle}>Complete Actions to Earn Points</Text>
        {userActions.map(action => (
          <TouchableOpacity
            key={action.id}
            style={styles.actionCard}
            onPress={() => handleCompleteAction(action)}
          >
            <View style={styles.actionContent}>
              <Text style={styles.actionDescription}>{action.description}</Text>
              <View style={styles.actionReward}>
                <Text style={styles.actionPoints}>+{action.pointReward}</Text>
              </View>
            </View>
          </TouchableOpacity>
        ))}
      </View>

      {/* End Session Button */}
      <View style={styles.section}>
        <TouchableOpacity style={styles.endSessionButton} onPress={handleEndSession}>
          <Text style={styles.endSessionButtonText}>End Session</Text>
        </TouchableOpacity>
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
    backgroundColor: '#fff',
    padding: 20,
    marginBottom: 20,
  },
  sessionName: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#1f2937',
    marginBottom: 8,
  },
  sessionDescription: {
    fontSize: 16,
    color: '#6b7280',
    marginBottom: 16,
  },
  pointsContainer: {
    backgroundColor: '#f3f4f6',
    borderRadius: 8,
    padding: 12,
  },
  pointsText: {
    fontSize: 18,
    fontWeight: '600',
    color: '#6366f1',
    marginBottom: 4,
  },
  statsText: {
    fontSize: 14,
    color: '#6b7280',
  },
  workoutCard: {
    backgroundColor: '#fff',
    marginHorizontal: 20,
    marginBottom: 20,
    borderRadius: 12,
    padding: 20,
    borderLeftWidth: 4,
    borderLeftColor: '#10b981',
  },
  workoutTitle: {
    fontSize: 18,
    fontWeight: '600',
    color: '#1f2937',
    marginBottom: 8,
  },
  workoutName: {
    fontSize: 20,
    fontWeight: 'bold',
    color: '#10b981',
    marginBottom: 8,
  },
  workoutDescription: {
    fontSize: 16,
    color: '#374151',
    marginBottom: 8,
  },
  workoutDetails: {
    fontSize: 14,
    color: '#6b7280',
    marginBottom: 12,
  },
  workoutInstructions: {
    fontSize: 14,
    color: '#374151',
    lineHeight: 20,
    marginBottom: 16,
  },
  completeWorkoutButton: {
    backgroundColor: '#10b981',
    borderRadius: 8,
    paddingVertical: 12,
    alignItems: 'center',
  },
  completeWorkoutButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
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
  randomWorkoutButton: {
    backgroundColor: '#6366f1',
    borderRadius: 12,
    paddingVertical: 16,
    alignItems: 'center',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 3,
  },
  disabledButton: {
    backgroundColor: '#9ca3af',
  },
  randomWorkoutButtonText: {
    color: '#fff',
    fontSize: 18,
    fontWeight: '600',
  },
  actionCard: {
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
  actionContent: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  actionDescription: {
    fontSize: 16,
    color: '#374151',
    flex: 1,
  },
  actionReward: {
    backgroundColor: '#10b981',
    borderRadius: 16,
    paddingHorizontal: 12,
    paddingVertical: 6,
  },
  actionPoints: {
    color: '#fff',
    fontSize: 14,
    fontWeight: '600',
  },
  endSessionButton: {
    backgroundColor: '#ef4444',
    borderRadius: 8,
    paddingVertical: 12,
    alignItems: 'center',
  },
  endSessionButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
  },
});

export default SessionScreen;