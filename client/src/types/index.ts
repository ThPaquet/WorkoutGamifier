export interface User {
  id: string;
  username: string;
  email: string;
  totalPoints: number;
  createdAt: string;
  updatedAt: string;
}

export interface Workout {
  id: string;
  name: string;
  description: string;
  durationMinutes: number;
  category: string;
  difficulty: number;
  instructions: string;
  createdAt: string;
  updatedAt: string;
}

export interface WorkoutPool {
  id: string;
  name: string;
  description: string;
  userId: string;
  workouts: Workout[];
  createdAt: string;
  updatedAt: string;
}

export interface WorkoutPoolSummary {
  id: string;
  name: string;
  description: string;
  workoutCount: number;
  createdAt: string;
}

export interface Session {
  id: string;
  name: string;
  description: string;
  startTime: string;
  endTime?: string;
  status: SessionStatus;
  currentPoints: number;
  totalPointsEarned: number;
  totalPointsSpent: number;
  userId: string;
  workoutPoolId: string;
  workoutPoolName: string;
  createdAt: string;
  updatedAt: string;
}

export interface SessionStats {
  sessionId: string;
  sessionName: string;
  currentPoints: number;
  totalPointsEarned: number;
  totalPointsSpent: number;
  actionsCompleted: number;
  workoutsCompleted: number;
  duration: string;
}

export interface UserAction {
  id: string;
  description: string;
  pointReward: number;
  userId: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateSessionDto {
  name: string;
  description: string;
  workoutPoolId: string;
}

export interface CreateUserActionDto {
  description: string;
  pointReward: number;
}

export interface CreateWorkoutPoolDto {
  name: string;
  description: string;
  workoutIds: string[];
}

export interface CompleteActionDto {
  userActionId: string;
  notes?: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface CreateUserDto {
  username: string;
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  user: User;
}

export enum SessionStatus {
  Active = 0,
  Paused = 1,
  Completed = 2,
  Cancelled = 3
}

export enum WorkoutStatus {
  Assigned = 0,
  InProgress = 1,
  Completed = 2,
  Skipped = 3
}