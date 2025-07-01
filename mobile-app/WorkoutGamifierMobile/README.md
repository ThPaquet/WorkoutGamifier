# WorkoutGamifier Mobile

A React Native mobile application that gamifies workout routines using a points-based system. This is a self-contained mobile version of the WorkoutGamifier web application.

## 🎯 Features

- **Self-contained**: No external API required - all data stored locally
- **Built-in Exercise Library**: 10+ pre-loaded exercises across different categories
- **Points System**: Earn points by completing real-life actions
- **Random Workouts**: Spend points to get random workouts from curated pools
- **Workout Sessions**: Create and manage workout sessions
- **Default Workout Pools**: Pre-configured workout collections (Quick Cardio, Strength Builder, Full Body Blast)
- **Default Actions**: Pre-loaded point-earning activities

## 🏗️ Architecture

- **React Native with Expo**: For cross-platform mobile development
- **TypeScript**: For type safety and better development experience
- **AsyncStorage**: Local data persistence
- **React Navigation**: Tab and stack navigation
- **Context API**: State management for authentication and user data

## 📱 Screens

1. **Authentication**
   - Login Screen (simplified for self-contained app)
   - Register Screen

2. **Main App (Tab Navigation)**
   - Dashboard: Overview, session management, quick stats
   - Sessions: Session history and management
   - Workout Pools: Manage workout collections
   - Actions: Manage point-earning activities
   - Profile: User info and logout

3. **Session Screen**
   - Active workout session management
   - Complete actions to earn points
   - Get random workouts by spending points
   - Real-time workout instructions

## 🚀 Getting Started

### Prerequisites

- Node.js (14 or higher)
- npm or yarn
- Expo CLI
- iOS Simulator (for iOS development) or Android Emulator (for Android development)

### Installation

1. Navigate to the mobile app directory:
   ```bash
   cd mobile-app/WorkoutGamifierMobile
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   npm start
   ```

4. Run on your preferred platform:
   ```bash
   npm run ios     # iOS Simulator
   npm run android # Android Emulator
   npm run web     # Web browser
   ```

## 📊 Built-in Data

### Default Workouts
- Push-ups (Strength, Difficulty: 3)
- Squats (Strength, Difficulty: 2)
- Jumping Jacks (Cardio, Difficulty: 1)
- Plank (Core, Difficulty: 4)
- Burpees (HIIT, Difficulty: 8)
- Mountain Climbers (Cardio, Difficulty: 5)
- Lunges (Strength, Difficulty: 3)
- High Knees (Cardio, Difficulty: 2)
- Wall Sit (Strength, Difficulty: 4)
- Tricep Dips (Strength, Difficulty: 5)

### Default Workout Pools
- **Quick Cardio**: Jumping Jacks, Mountain Climbers, High Knees
- **Strength Builder**: Push-ups, Squats, Plank, Lunges
- **Full Body Blast**: Push-ups, Squats, Burpees, Wall Sit, Tricep Dips

### Default Actions
- Drink a glass of water (1 point)
- Take a 5-minute walk (2 points)
- Do 10 jumping jacks (1 point)
- Stretch for 2 minutes (1 point)
- Take stairs instead of elevator (2 points)
- Quick meditation (3 points)
- Eat a healthy snack (2 points)
- Stand up and move for 1 minute (1 point)

## 🎮 How to Use

1. **First Launch**: The app automatically creates a default user and initializes workout pools and actions
2. **Start a Session**: Choose a workout pool and start a new session
3. **Earn Points**: Complete real-life actions to earn points
4. **Get Workouts**: Spend points to receive random workouts from your selected pool
5. **Follow Instructions**: Each workout comes with detailed instructions and difficulty rating
6. **Track Progress**: Monitor your points, sessions, and completed workouts

## 🛠️ Technical Details

### Data Storage
- All data is stored locally using AsyncStorage
- No network requests or external dependencies
- Data persists between app launches

### State Management
- React Context for authentication state
- Local state management in components
- Automatic data initialization on first launch

### Navigation Structure
```
App
├── AuthNavigator (Stack)
│   ├── Login
│   └── Register
└── MainNavigator (Stack)
    ├── MainTabs (Bottom Tabs)
    │   ├── Dashboard
    │   ├── Sessions
    │   ├── WorkoutPools
    │   ├── Actions
    │   └── Profile
    └── Session (Modal)
```

## 🎨 Design Principles

- **Mobile-first**: Optimized for touch interactions
- **Intuitive UX**: Clear navigation and feedback
- **Consistent Styling**: Following modern mobile design patterns
- **Accessibility**: Proper contrast and touch targets
- **Performance**: Efficient local data management

## 🔧 Development

### File Structure
```
src/
├── components/          # Reusable UI components
├── navigation/          # Navigation configuration
├── screens/            # Screen components
├── services/           # Data services and context
├── types/              # TypeScript type definitions
├── styles/             # Shared styles (if needed)
└── utils/              # Utility functions
```

### Key Services
- **dataService**: Manages all local data operations
- **AuthContext**: Handles authentication state
- **Navigation**: Manages app navigation flow

## 📦 Dependencies

### Core
- React Native (via Expo)
- TypeScript
- React Navigation
- AsyncStorage
- Expo Secure Store

### UI/UX
- React Native Safe Area Context
- React Native Screens
- Custom styling with StyleSheet

## 🚀 Building for Production

1. **iOS**:
   ```bash
   expo build:ios
   ```

2. **Android**:
   ```bash
   expo build:android
   ```

3. **Web** (optional):
   ```bash
   expo build:web
   ```

## 🔄 Data Migration

Since this is a self-contained app, no data migration from the web version is needed. The app creates its own local dataset on first launch.

## 🎯 Future Enhancements

- Custom workout creation
- Progress tracking and statistics
- Achievement system
- Workout timers
- Social features (if network connectivity added)
- Backup/restore functionality
- Custom action creation interface
- Workout difficulty progression

This mobile app provides a complete, offline-capable fitness gamification experience that can run independently without any backend infrastructure.