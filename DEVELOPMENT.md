# WorkoutGamifier Development Guide

This document provides instructions for setting up, running, and developing the WorkoutGamifier application.

## 🏗️ Architecture Overview

The application follows **Onion Architecture** principles with clear separation of concerns:

```
├── src/
│   ├── WorkoutGamifier.Domain/          # Core business entities and interfaces
│   ├── WorkoutGamifier.Application/     # Business logic and services
│   ├── WorkoutGamifier.Infrastructure/  # Data access and external services
│   └── WorkoutGamifier.API/            # Web API controllers and configuration
├── client/                             # React TypeScript frontend
└── tests/                             # Unit and integration tests
```

## 🛠️ Prerequisites

- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js** (v16 or later) - [Download here](https://nodejs.org/)
- **Git** - [Download here](https://git-scm.com/)

## 🚀 Quick Start

1. **Clone and setup:**
   ```bash
   git clone <repository-url>
   cd WorkoutGamifier
   ./setup.sh
   ```

2. **Start the API server:**
   ```bash
   cd src/WorkoutGamifier.API
   dotnet run
   ```

3. **Start the React client (in a new terminal):**
   ```bash
   cd client
   npm start
   ```

4. **Access the application:**
   - **Frontend:** http://localhost:3000
   - **API/Swagger:** http://localhost:5000

## 🎯 Core Features

### Backend (C# .NET 8)
- **RESTful API** with Swagger documentation
- **Entity Framework Core** with SQLite database
- **JWT Authentication** for secure user sessions
- **Onion Architecture** with dependency injection
- **AutoMapper** for object mapping
- **MSTest** for unit testing

### Frontend (React TypeScript)
- **Mobile-first responsive design**
- **React Router** for navigation
- **Axios** for API communication
- **TypeScript** for type safety
- **CSS** with mobile-first approach (no external CSS frameworks)

## 📊 Database Schema

The application uses SQLite with the following main entities:

- **Users** - User accounts and authentication
- **Workouts** - Exercise definitions with difficulty and instructions
- **WorkoutPools** - User-defined collections of workouts
- **Sessions** - Active workout sessions with point tracking
- **UserActions** - User-defined actions that earn points
- **SessionActions** - Actions completed during sessions
- **SessionWorkouts** - Workouts assigned and completed during sessions

## 🧪 Testing

### Backend Tests
```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/WorkoutGamifier.Domain.Tests/
```

### Frontend Tests
```bash
cd client
npm test
```

## 🔧 Development Commands

### Backend
```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Clean build artifacts
dotnet clean

# Run API with hot reload
cd src/WorkoutGamifier.API
dotnet watch run

# Add Entity Framework migration
dotnet ef migrations add <MigrationName> --project src/WorkoutGamifier.Infrastructure --startup-project src/WorkoutGamifier.API

# Update database
dotnet ef database update --project src/WorkoutGamifier.Infrastructure --startup-project src/WorkoutGamifier.API
```

### Frontend
```bash
cd client

# Install dependencies
npm install

# Start development server
npm start

# Build for production
npm run build

# Run tests
npm test

# Run tests with coverage
npm test -- --coverage
```

## 🏛️ Project Structure Details

### Domain Layer (`WorkoutGamifier.Domain`)
- **Entities:** Core business objects (User, Session, Workout, etc.)
- **Enums:** Status enumerations
- **Interfaces:** Repository and service contracts

### Application Layer (`WorkoutGamifier.Application`)
- **DTOs:** Data transfer objects for API communication
- **Services:** Business logic implementation
- **Mappings:** AutoMapper profiles

### Infrastructure Layer (`WorkoutGamifier.Infrastructure`)
- **Data:** Entity Framework DbContext and configurations
- **Repositories:** Data access implementations

### API Layer (`WorkoutGamifier.API`)
- **Controllers:** HTTP endpoints
- **Program.cs:** Application startup and configuration

### Client (`client/`)
- **Components:** React components
- **Services:** API communication layer
- **Types:** TypeScript type definitions
- **Styles:** CSS stylesheets

## 🎨 Design Guidelines

### Mobile-First Approach
- All CSS is written with mobile devices as the primary target
- Responsive breakpoints: 768px (tablet), 1024px (desktop)
- Touch-friendly button sizes and spacing
- Optimized for portrait orientation

### Code Standards
- **C#:** Follow Microsoft naming conventions
- **TypeScript/React:** Use functional components with hooks
- **CSS:** BEM-like naming convention, utility classes
- **Testing:** Arrange-Act-Assert pattern for unit tests

## 🔒 Security

- **JWT Authentication** with configurable expiration
- **HTTPS** enforcement in production
- **CORS** configured for frontend domain
- **Input validation** on all API endpoints
- **SQL injection protection** via Entity Framework

## 🚀 Deployment

### Production Build
```bash
# Build backend
dotnet publish src/WorkoutGamifier.API -c Release -o publish/

# Build frontend
cd client
npm run build
```

### Environment Variables
Create `appsettings.Production.json` for production settings:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-production-connection-string"
  },
  "JwtSettings": {
    "SecretKey": "your-production-secret-key",
    "ExpiryInHours": 24
  }
}
```

## 🐛 Troubleshooting

### Common Issues

1. **Port conflicts:**
   - API default: http://localhost:5000
   - React default: http://localhost:3000
   - Change ports in `launchSettings.json` (API) or `package.json` (React)

2. **Database issues:**
   ```bash
   # Delete and recreate database
   rm src/WorkoutGamifier.API/workoutgamifier.db
   cd src/WorkoutGamifier.API
   dotnet run
   ```

3. **CORS errors:**
   - Ensure React app URL is in CORS policy in `Program.cs`
   - Check that API is running before starting React app

## 📝 Contributing

1. Follow the existing code style and patterns
2. Write tests for new features
3. Update documentation for significant changes
4. Ensure mobile-first responsive design
5. Test on multiple screen sizes

## 📚 Additional Resources

- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [React Documentation](https://reactjs.org/docs/)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/)
- [Onion Architecture](https://jeffreypalermo.com/2008/07/the-onion-architecture-part-1/)