import React, { useState } from 'react';
import { apiService } from '../services/api';
import { LoginDto, CreateUserDto } from '../types';

const Login: React.FC = () => {
  const [isLogin, setIsLogin] = useState(true);
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    username: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    try {
      if (isLogin) {
        const loginData: LoginDto = {
          email: formData.email,
          password: formData.password
        };
        const response = await apiService.login(loginData);
        apiService.setAuthToken(response.token);
        localStorage.setItem('user', JSON.stringify(response.user));
      } else {
        const registerData: CreateUserDto = {
          username: formData.username,
          email: formData.email,
          password: formData.password
        };
        const response = await apiService.register(registerData);
        apiService.setAuthToken(response.token);
        localStorage.setItem('user', JSON.stringify(response.user));
      }
      
      window.location.href = '/';
    } catch (err: any) {
      setError(err.response?.data?.message || 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="card fade-in">
      <div className="card-header">
        <h2 className="card-title">
          {isLogin ? 'Login' : 'Register'}
        </h2>
      </div>

      {error && (
        <div className="error">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit} className="form">
        {!isLogin && (
          <div className="form-group">
            <label className="form-label">Username</label>
            <input
              type="text"
              name="username"
              value={formData.username}
              onChange={handleInputChange}
              className="form-input"
              required={!isLogin}
            />
          </div>
        )}

        <div className="form-group">
          <label className="form-label">Email</label>
          <input
            type="email"
            name="email"
            value={formData.email}
            onChange={handleInputChange}
            className="form-input"
            required
          />
        </div>

        <div className="form-group">
          <label className="form-label">Password</label>
          <input
            type="password"
            name="password"
            value={formData.password}
            onChange={handleInputChange}
            className="form-input"
            required
          />
        </div>

        <button 
          type="submit" 
          className="btn"
          disabled={loading}
        >
          {loading ? 'Please wait...' : (isLogin ? 'Login' : 'Register')}
        </button>

        <button
          type="button"
          className="btn btn-secondary"
          onClick={() => setIsLogin(!isLogin)}
        >
          {isLogin ? 'Need an account? Register' : 'Have an account? Login'}
        </button>
      </form>
    </div>
  );
};

export default Login;