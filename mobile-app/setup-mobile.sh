#!/bin/bash

echo "🚀 Setting up WorkoutGamifier Mobile App..."

# Check if Node.js is installed
if ! command -v node &> /dev/null; then
    echo "❌ Node.js is not installed. Please install Node.js first."
    exit 1
fi

# Check if npm is installed
if ! command -v npm &> /dev/null; then
    echo "❌ npm is not installed. Please install npm first."
    exit 1
fi

echo "✅ Node.js and npm are installed"

# Navigate to the mobile app directory
cd WorkoutGamifierMobile

echo "📦 Installing dependencies..."
npm install

# Check if Expo CLI is installed globally
if ! command -v expo &> /dev/null; then
    echo "📱 Installing Expo CLI globally..."
    npm install -g @expo/cli
fi

echo "✅ Setup complete!"
echo ""
echo "🎯 To start the app:"
echo "   cd WorkoutGamifierMobile"
echo "   npm start"
echo ""
echo "📱 Then choose your platform:"
echo "   - Press 'i' for iOS Simulator"
echo "   - Press 'a' for Android Emulator"
echo "   - Press 'w' for Web"
echo ""
echo "🎮 The app includes:"
echo "   - 10+ built-in exercises"
echo "   - 3 default workout pools"
echo "   - 8 default point-earning actions"
echo "   - Complete offline functionality"
echo ""
echo "Happy working out! 💪"