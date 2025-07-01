import axios, { AxiosInstance, AxiosResponse } from 'axios';
import * as SecureStore from 'expo-secure-store';
import { 
  AuthResponse, 
  LoginDto, 
  CreateUserDto, 
  Session, 
  CreateSessionDto, 
  SessionStats,
  CompleteActionDto,
  Workout,
  UserAction,
  WorkoutPool
} from '../types';

class ApiService {
  private api: AxiosInstance;
  private readonly TOKEN_KEY = 'authToken';
  private readonly USER_KEY = 'user';

  constructor() {
    this.api = axios.create({
      baseURL: __DEV__ ? 'http://localhost:5000/api' : 'https://your-production-api.com/api',
      headers: {
        'Content-Type': 'application/json',
      },
      timeout: 10000,
    });

    // Add request interceptor to include auth token
    this.api.interceptors.request.use(
      async (config) => {
        const token = await this.getAuthToken();
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => {
        return Promise.reject(error);
      }
    );

    // Add response interceptor to handle auth errors
    this.api.interceptors.response.use(
      (response) => response,
      async (error) => {
        if (error.response?.status === 401) {
          await this.removeAuthToken();
          // Navigate to login - this would be handled by navigation context
        }
        return Promise.reject(error);
      }
    );
  }

  // Auth endpoints
  async login(loginData: LoginDto): Promise<AuthResponse> {
    const response: AxiosResponse<AuthResponse> = await this.api.post('/auth/login', loginData);
    return response.data;
  }

  async register(userData: CreateUserDto): Promise<AuthResponse> {
    const response: AxiosResponse<AuthResponse> = await this.api.post('/auth/register', userData);
    return response.data;
  }

  // Session endpoints
  async getSessions(): Promise<Session[]> {
    const response: AxiosResponse<Session[]> = await this.api.get('/sessions');
    return response.data;
  }

  async getSession(id: string): Promise<Session> {
    const response: AxiosResponse<Session> = await this.api.get(`/sessions/${id}`);
    return response.data;
  }

  async createSession(sessionData: CreateSessionDto): Promise<Session> {
    const response: AxiosResponse<Session> = await this.api.post('/sessions', sessionData);
    return response.data;
  }

  async getSessionStats(id: string): Promise<SessionStats> {
    const response: AxiosResponse<SessionStats> = await this.api.get(`/sessions/${id}/stats`);
    return response.data;
  }

  async completeAction(sessionId: string, actionData: CompleteActionDto): Promise<Session> {
    const response: AxiosResponse<Session> = await this.api.post(`/sessions/${sessionId}/actions`, actionData);
    return response.data;
  }

  async getRandomWorkout(sessionId: string, pointsCost: number = 1): Promise<Workout> {
    const response: AxiosResponse<Workout> = await this.api.post(`/sessions/${sessionId}/workouts/random`, { pointsCost });
    return response.data;
  }

  async completeWorkout(sessionId: string, workoutId: string, notes?: string): Promise<Session> {
    const response: AxiosResponse<Session> = await this.api.post(`/sessions/${sessionId}/workouts/${workoutId}/complete`, { notes });
    return response.data;
  }

  async endSession(sessionId: string): Promise<Session> {
    const response: AxiosResponse<Session> = await this.api.post(`/sessions/${sessionId}/end`);
    return response.data;
  }

  // Workout endpoints
  async getWorkouts(): Promise<Workout[]> {
    const response: AxiosResponse<Workout[]> = await this.api.get('/workouts');
    return response.data;
  }

  // UserAction endpoints
  async getUserActions(): Promise<UserAction[]> {
    const response: AxiosResponse<UserAction[]> = await this.api.get('/useractions');
    return response.data;
  }

  // WorkoutPool endpoints
  async getWorkoutPools(): Promise<WorkoutPool[]> {
    const response: AxiosResponse<WorkoutPool[]> = await this.api.get('/workoutpools');
    return response.data;
  }

  // Secure storage methods for mobile
  async setAuthToken(token: string): Promise<void> {
    await SecureStore.setItemAsync(this.TOKEN_KEY, token);
  }

  async removeAuthToken(): Promise<void> {
    await SecureStore.deleteItemAsync(this.TOKEN_KEY);
    await SecureStore.deleteItemAsync(this.USER_KEY);
  }

  async getAuthToken(): Promise<string | null> {
    return await SecureStore.getItemAsync(this.TOKEN_KEY);
  }

  async setUser(user: any): Promise<void> {
    await SecureStore.setItemAsync(this.USER_KEY, JSON.stringify(user));
  }

  async getUser(): Promise<any | null> {
    const userString = await SecureStore.getItemAsync(this.USER_KEY);
    return userString ? JSON.parse(userString) : null;
  }

  async isAuthenticated(): Promise<boolean> {
    const token = await this.getAuthToken();
    return !!token;
  }
}

export const apiService = new ApiService();
export default apiService;