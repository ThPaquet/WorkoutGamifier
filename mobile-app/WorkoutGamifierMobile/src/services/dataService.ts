import AsyncStorage from '@react-native-async-storage/async-storage';
import { 
  User, 
  Workout, 
  WorkoutPool, 
  Session, 
  UserAction, 
  SessionStatus,
  CreateSessionDto,
  CreateUserActionDto,
  CreateWorkoutPoolDto,
  CompleteActionDto,
  SessionStats
} from '../types';

// Built-in workout catalog
const DEFAULT_WORKOUTS: Workout[] = [
  {
    id: '1',
    name: 'Push-ups',
    description: 'Classic upper body exercise',
    durationMinutes: 5,
    category: 'Strength',
    difficulty: 3,
    instructions: '1. Start in plank position\n2. Lower your body until chest nearly touches floor\n3. Push back up\n4. Repeat for desired reps',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: '2',
    name: 'Squats',
    description: 'Lower body strength exercise',
    durationMinutes: 5,
    category: 'Strength',
    difficulty: 2,
    instructions: '1. Stand with feet shoulder-width apart\n2. Lower your hips back and down\n3. Keep chest up and knees behind toes\n4. Return to standing',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: '3',
    name: 'Jumping Jacks',
    description: 'Full body cardio exercise',
    durationMinutes: 3,
    category: 'Cardio',
    difficulty: 1,
    instructions: '1. Start standing with feet together\n2. Jump while spreading legs and raising arms\n3. Jump back to starting position\n4. Repeat rapidly',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: '4',
    name: 'Plank',
    description: 'Core strengthening exercise',
    durationMinutes: 2,
    category: 'Core',
    difficulty: 4,
    instructions: '1. Start in push-up position\n2. Lower to forearms\n3. Keep body straight from head to heels\n4. Hold position',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: '5',
    name: 'Burpees',
    description: 'High-intensity full body exercise',
    durationMinutes: 5,
    category: 'HIIT',
    difficulty: 8,
    instructions: '1. Start standing\n2. Drop to squat, hands on floor\n3. Jump feet back to plank\n4. Do push-up\n5. Jump feet to squat\n6. Jump up with arms overhead',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: '6',
    name: 'Mountain Climbers',
    description: 'Cardio and core exercise',
    durationMinutes: 3,
    category: 'Cardio',
    difficulty: 5,
    instructions: '1. Start in plank position\n2. Bring one knee to chest\n3. Switch legs rapidly\n4. Keep hips level',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: '7',
    name: 'Lunges',
    description: 'Single-leg strength exercise',
    durationMinutes: 4,
    category: 'Strength',
    difficulty: 3,
    instructions: '1. Step forward with one leg\n2. Lower hips until both knees at 90 degrees\n3. Push back to starting position\n4. Alternate legs',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: '8',
    name: 'High Knees',
    description: 'Cardio exercise for leg strength',
    durationMinutes: 2,
    category: 'Cardio',
    difficulty: 2,
    instructions: '1. Stand in place\n2. Lift knees as high as possible\n3. Pump arms while running in place\n4. Maintain quick pace',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: '9',
    name: 'Wall Sit',
    description: 'Isometric leg exercise',
    durationMinutes: 3,
    category: 'Strength',
    difficulty: 4,
    instructions: '1. Stand with back against wall\n2. Slide down until thighs parallel to floor\n3. Keep knees at 90 degrees\n4. Hold position',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: '10',
    name: 'Tricep Dips',
    description: 'Upper body exercise using chair or bench',
    durationMinutes: 4,
    category: 'Strength',
    difficulty: 5,
    instructions: '1. Sit on edge of chair, hands beside hips\n2. Slide off chair, supporting weight with arms\n3. Lower body by bending elbows\n4. Push back up',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
];

class DataService {
  private readonly KEYS = {
    USER: 'user',
    SESSIONS: 'sessions',
    WORKOUT_POOLS: 'workoutPools',
    USER_ACTIONS: 'userActions',
    CURRENT_SESSION: 'currentSession',
  };

  // User Management
  async createUser(username: string, email: string): Promise<User> {
    const user: User = {
      id: Date.now().toString(),
      username,
      email,
      totalPoints: 0,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };
    
    await AsyncStorage.setItem(this.KEYS.USER, JSON.stringify(user));
    return user;
  }

  async getUser(): Promise<User | null> {
    const userString = await AsyncStorage.getItem(this.KEYS.USER);
    return userString ? JSON.parse(userString) : null;
  }

  async updateUser(user: User): Promise<User> {
    const updatedUser = { ...user, updatedAt: new Date().toISOString() };
    await AsyncStorage.setItem(this.KEYS.USER, JSON.stringify(updatedUser));
    return updatedUser;
  }

  // Workouts
  async getWorkouts(): Promise<Workout[]> {
    return DEFAULT_WORKOUTS;
  }

  async getWorkout(id: string): Promise<Workout | null> {
    return DEFAULT_WORKOUTS.find(w => w.id === id) || null;
  }

  // Workout Pools
  async getWorkoutPools(): Promise<WorkoutPool[]> {
    const poolsString = await AsyncStorage.getItem(this.KEYS.WORKOUT_POOLS);
    return poolsString ? JSON.parse(poolsString) : [];
  }

  async createWorkoutPool(data: CreateWorkoutPoolDto): Promise<WorkoutPool> {
    const pools = await this.getWorkoutPools();
    const workouts = DEFAULT_WORKOUTS.filter(w => data.workoutIds.includes(w.id));
    
    const pool: WorkoutPool = {
      id: Date.now().toString(),
      name: data.name,
      description: data.description,
      userId: '1', // Since we're self-contained, we'll use a default user ID
      workouts,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };

    pools.push(pool);
    await AsyncStorage.setItem(this.KEYS.WORKOUT_POOLS, JSON.stringify(pools));
    return pool;
  }

  async deleteWorkoutPool(id: string): Promise<void> {
    const pools = await this.getWorkoutPools();
    const filteredPools = pools.filter(p => p.id !== id);
    await AsyncStorage.setItem(this.KEYS.WORKOUT_POOLS, JSON.stringify(filteredPools));
  }

  // User Actions
  async getUserActions(): Promise<UserAction[]> {
    const actionsString = await AsyncStorage.getItem(this.KEYS.USER_ACTIONS);
    return actionsString ? JSON.parse(actionsString) : [];
  }

  async createUserAction(data: CreateUserActionDto): Promise<UserAction> {
    const actions = await this.getUserActions();
    
    const action: UserAction = {
      id: Date.now().toString(),
      description: data.description,
      pointReward: data.pointReward,
      userId: '1',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };

    actions.push(action);
    await AsyncStorage.setItem(this.KEYS.USER_ACTIONS, JSON.stringify(actions));
    return action;
  }

  async deleteUserAction(id: string): Promise<void> {
    const actions = await this.getUserActions();
    const filteredActions = actions.filter(a => a.id !== id);
    await AsyncStorage.setItem(this.KEYS.USER_ACTIONS, JSON.stringify(filteredActions));
  }

  // Sessions
  async getSessions(): Promise<Session[]> {
    const sessionsString = await AsyncStorage.getItem(this.KEYS.SESSIONS);
    return sessionsString ? JSON.parse(sessionsString) : [];
  }

  async getSession(id: string): Promise<Session | null> {
    const sessions = await this.getSessions();
    return sessions.find(s => s.id === id) || null;
  }

  async createSession(data: CreateSessionDto): Promise<Session> {
    const sessions = await this.getSessions();
    const pools = await this.getWorkoutPools();
    const pool = pools.find(p => p.id === data.workoutPoolId);
    
    if (!pool) {
      throw new Error('Workout pool not found');
    }

    const session: Session = {
      id: Date.now().toString(),
      name: data.name,
      description: data.description,
      startTime: new Date().toISOString(),
      status: SessionStatus.Active,
      currentPoints: 0,
      totalPointsEarned: 0,
      totalPointsSpent: 0,
      userId: '1',
      workoutPoolId: data.workoutPoolId,
      workoutPoolName: pool.name,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };

    sessions.push(session);
    await AsyncStorage.setItem(this.KEYS.SESSIONS, JSON.stringify(sessions));
    await AsyncStorage.setItem(this.KEYS.CURRENT_SESSION, JSON.stringify(session));
    return session;
  }

  async completeAction(sessionId: string, actionData: CompleteActionDto): Promise<Session> {
    const sessions = await this.getSessions();
    const actions = await this.getUserActions();
    const sessionIndex = sessions.findIndex(s => s.id === sessionId);
    const action = actions.find(a => a.id === actionData.userActionId);

    if (sessionIndex === -1 || !action) {
      throw new Error('Session or action not found');
    }

    const session = sessions[sessionIndex];
    session.totalPointsEarned += action.pointReward;
    session.currentPoints += action.pointReward;
    session.updatedAt = new Date().toISOString();

    sessions[sessionIndex] = session;
    await AsyncStorage.setItem(this.KEYS.SESSIONS, JSON.stringify(sessions));
    
    // Update user total points
    const user = await this.getUser();
    if (user) {
      user.totalPoints += action.pointReward;
      await this.updateUser(user);
    }

    return session;
  }

  async getRandomWorkout(sessionId: string, pointsCost: number = 1): Promise<Workout> {
    const session = await this.getSession(sessionId);
    if (!session || session.currentPoints < pointsCost) {
      throw new Error('Insufficient points or session not found');
    }

    const pools = await this.getWorkoutPools();
    const pool = pools.find(p => p.id === session.workoutPoolId);
    if (!pool || pool.workouts.length === 0) {
      throw new Error('No workouts available in pool');
    }

    // Get random workout from pool
    const randomIndex = Math.floor(Math.random() * pool.workouts.length);
    const workout = pool.workouts[randomIndex];

    // Deduct points
    const sessions = await this.getSessions();
    const sessionIndex = sessions.findIndex(s => s.id === sessionId);
    if (sessionIndex !== -1) {
      sessions[sessionIndex].currentPoints -= pointsCost;
      sessions[sessionIndex].totalPointsSpent += pointsCost;
      sessions[sessionIndex].updatedAt = new Date().toISOString();
      await AsyncStorage.setItem(this.KEYS.SESSIONS, JSON.stringify(sessions));
    }

    return workout;
  }

  async endSession(sessionId: string): Promise<Session> {
    const sessions = await this.getSessions();
    const sessionIndex = sessions.findIndex(s => s.id === sessionId);
    
    if (sessionIndex === -1) {
      throw new Error('Session not found');
    }

    sessions[sessionIndex].status = SessionStatus.Completed;
    sessions[sessionIndex].endTime = new Date().toISOString();
    sessions[sessionIndex].updatedAt = new Date().toISOString();

    await AsyncStorage.setItem(this.KEYS.SESSIONS, JSON.stringify(sessions));
    await AsyncStorage.removeItem(this.KEYS.CURRENT_SESSION);
    
    return sessions[sessionIndex];
  }

  async getCurrentSession(): Promise<Session | null> {
    const sessionString = await AsyncStorage.getItem(this.KEYS.CURRENT_SESSION);
    return sessionString ? JSON.parse(sessionString) : null;
  }

  async getSessionStats(sessionId: string): Promise<SessionStats> {
    const session = await this.getSession(sessionId);
    if (!session) {
      throw new Error('Session not found');
    }

    const startTime = new Date(session.startTime);
    const endTime = session.endTime ? new Date(session.endTime) : new Date();
    const duration = Math.floor((endTime.getTime() - startTime.getTime()) / 1000 / 60); // minutes

    return {
      sessionId: session.id,
      sessionName: session.name,
      currentPoints: session.currentPoints,
      totalPointsEarned: session.totalPointsEarned,
      totalPointsSpent: session.totalPointsSpent,
      actionsCompleted: 0, // Would need to track this separately
      workoutsCompleted: 0, // Would need to track this separately
      duration: `${duration} minutes`,
    };
  }

  // Initialize default data
  async initializeDefaultData(): Promise<void> {
    const user = await this.getUser();
    if (!user) {
      await this.createUser('Player', 'player@workoutgamifier.com');
    }

    const actions = await this.getUserActions();
    if (actions.length === 0) {
      // Create some default actions
      const defaultActions = [
        { description: 'Drink a glass of water 💧', pointReward: 1 },
        { description: 'Take a 5-minute walk 🚶', pointReward: 2 },
        { description: 'Do 10 jumping jacks 🤸', pointReward: 1 },
        { description: 'Stretch for 2 minutes 🧘', pointReward: 1 },
        { description: 'Take the stairs instead of elevator 🪜', pointReward: 2 },
        { description: 'Do a quick meditation (3 min) 🧠', pointReward: 3 },
        { description: 'Eat a healthy snack 🥗', pointReward: 2 },
        { description: 'Stand up and move for 1 minute 💃', pointReward: 1 },
      ];

      for (const action of defaultActions) {
        await this.createUserAction(action);
      }
    }

    const pools = await this.getWorkoutPools();
    if (pools.length === 0) {
      // Create some default workout pools
      await this.createWorkoutPool({
        name: 'Quick Cardio',
        description: 'Fast cardio exercises for energy boost',
        workoutIds: ['3', '6', '8'], // Jumping Jacks, Mountain Climbers, High Knees
      });

      await this.createWorkoutPool({
        name: 'Strength Builder',
        description: 'Build muscle with bodyweight exercises',
        workoutIds: ['1', '2', '4', '7'], // Push-ups, Squats, Plank, Lunges
      });

      await this.createWorkoutPool({
        name: 'Full Body Blast',
        description: 'Complete workout covering all muscle groups',
        workoutIds: ['1', '2', '5', '9', '10'], // Push-ups, Squats, Burpees, Wall Sit, Tricep Dips
      });
    }
  }
}

export const dataService = new DataService();
export default dataService;