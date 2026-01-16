#!/usr/bin/env bash

echo "========================================"
echo "Compiling Andromeda-13 (Release)"
echo "========================================"

# Переходим в корень проекта
cd "$(dirname "$0")/.." || {
    echo "ERROR: Cannot go to project root!"
    exit 1
}

echo "Cleaning previous build..."
dotnet clean
if [ $? -ne 0 ]; then
    echo "WARNING: Clean failed, continuing anyway..."
fi

echo "Building Release configuration..."
dotnet build --configuration Release
if [ $? -ne 0 ]; then
    echo "ERROR: Release build failed!"
    exit 1
fi

echo "========================================"
echo "Release build successful!"
echo "========================================"
sleep 5