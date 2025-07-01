import axios, { AxiosInstance, AxiosResponse } from 'axios';
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

  constructor() {
    this.api = axios.create({
      baseURL: process.env.REACT_APP_API_URL || 'http://localhost:5000/api',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Add request interceptor to include auth token
    this.api.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('authToken');
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
      (error) => {
        if (error.response?.status === 401) {
          localStorage.removeItem('authToken');
          localStorage.removeItem('user');
          window.location.href = '/login';
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
    const response: AxiosResponse<Workout> = await this.api.post(`/sessions/${sessionId}/workouts/random`, pointsCost);
    return response.data;
  }

  async completeWorkout(sessionId: string, workoutId: string, notes?: string): Promise<Session> {
    const response: AxiosResponse<Session> = await this.api.post(`/sessions/${sessionId}/workouts/${workoutId}/complete`, notes);
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

  // Utility methods
  setAuthToken(token: string): void {
    localStorage.setItem('authToken', token);
  }

  removeAuthToken(): void {
    localStorage.removeItem('authToken');
    localStorage.removeItem('user');
  }

  getAuthToken(): string | null {
    return localStorage.getItem('authToken');
  }

  isAuthenticated(): boolean {
    return !!this.getAuthToken();
  }
}

export const apiService = new ApiService();
export default apiService;