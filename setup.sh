#!/bin/bash

echo "🚀 Setting up WorkoutGamifier application..."

# Check if .NET 8 is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET 8 SDK is not installed. Please install .NET 8 SDK first."
    echo "Download from: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi

# Check if Node.js is installed
if ! command -v node &> /dev/null; then
    echo "❌ Node.js is not installed. Please install Node.js first."
    echo "Download from: https://nodejs.org/"
    exit 1
fi

echo "✅ Prerequisites check passed"

# Restore .NET packages
echo "📦 Restoring .NET packages..."
dotnet restore

# Build the solution
echo "🔨 Building the solution..."
dotnet build

# Install React dependencies
echo "📦 Installing React dependencies..."
cd client
npm install
cd ..

echo "✅ Setup completed successfully!"
echo ""
echo "🎯 To run the application:"
echo "1. Start the API server:"
echo "   cd src/WorkoutGamifier.API && dotnet run"
echo ""
echo "2. In a new terminal, start the React client:"
echo "   cd client && npm start"
echo ""
echo "3. Open your browser to:"
echo "   - API: http://localhost:5000 (Swagger UI)"
echo "   - Client: http://localhost:3000"
echo ""
echo "📚 Additional commands:"
echo "   - Run tests: dotnet test"
echo "   - Clean build: dotnet clean && dotnet build"