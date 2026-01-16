@echo off
echo ========================================
echo Compiling Andromeda-13 (Debug)
echo ========================================

cd ..
dotnet clean
if errorlevel 1 (
    echo WARNING: Clean failed, continuing anyway...
)

echo Building Debug configuration...
dotnet build
if errorlevel 1 (
    echo ERROR: Build failed!
    pause
    exit /b 1
)

echo ========================================
echo Debug build successful!
echo ========================================
timeout /t 5 /nobreak