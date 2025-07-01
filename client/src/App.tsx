import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import './styles/App.css';
import Dashboard from './components/Dashboard';
import SessionView from './components/SessionView';
import Login from './components/Login';
import { apiService } from './services/api';

function App() {
  const isAuthenticated = apiService.isAuthenticated();

  return (
    <div className="app">
      <Router>
        <div className="header">
          <h1>WorkoutGamifier</h1>
          <nav className="nav">
            {isAuthenticated && (
              <>
                <button className="nav-button" onClick={() => window.location.href = '/'}>
                  Dashboard
                </button>
                <button className="nav-button" onClick={() => {
                  apiService.removeAuthToken();
                  window.location.href = '/login';
                }}>
                  Logout
                </button>
              </>
            )}
          </nav>
        </div>
        
        <main className="main">
          <Routes>
            <Route 
              path="/login" 
              element={!isAuthenticated ? <Login /> : <Navigate to="/" />} 
            />
            <Route 
              path="/" 
              element={isAuthenticated ? <Dashboard /> : <Navigate to="/login" />} 
            />
            <Route 
              path="/session/:id" 
              element={isAuthenticated ? <SessionView /> : <Navigate to="/login" />} 
            />
            <Route path="*" element={<Navigate to="/" />} />
          </Routes>
        </main>
      </Router>
    </div>
  );
}

export default App;