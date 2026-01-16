@echo off
echo ========================================
echo Compiling Andromeda-13 (Release)
echo ========================================

cd ..
dotnet clean
if errorlevel 1 (
    echo WARNING: Clean failed, continuing anyway...
)

echo Building Release configuration...
dotnet build --configuration Release
if errorlevel 1 (
    echo ERROR: Release build failed!
    pause
    exit /b 1
)

echo ========================================
echo Release build successful!
echo ========================================
timeout /t 5 /nobreak