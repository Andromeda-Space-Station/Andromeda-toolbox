@echo off
echo ========================================
echo Updating RobustToolbox Engine
echo ========================================

cd ..
echo Working in: %cd%
echo ========================================

REM Check Git
where git >nul 2>nul
if errorlevel 1 (
    echo ERROR: Git is not installed!
    echo Install from: https://git-scm.com/downloads
    pause
    exit /b 1
)

REM Check if engine exists
if exist RobustToolbox (
    echo RobustToolbox found, updating...
    cd RobustToolbox
    
    echo Stashing local changes if any...
    git stash 2>nul
    
    echo Pulling latest changes...
    git pull --recurse-submodules
    
    if errorlevel 1 (
        echo WARNING: Git pull failed, attempting fresh clone...
        cd ..
        goto :fresh_clone
    )
    
    echo Updating submodules...
    git submodule update --init --recursive
    
    echo ========================================
    echo Engine updated in: %cd%
    echo ========================================
    timeout /t 5 /nobreak
    exit /b 0
) else (
    echo RobustToolbox not found, downloading...
)

:fresh_clone
REM Download fresh if doesn't exist or pull failed
echo Downloading RobustToolbox...
git clone --recurse-submodules https://github.com/space-wizards/RobustToolbox.git

if errorlevel 1 (
    echo ERROR: Download failed!
    pause
    exit /b 1
)

echo ========================================
echo Engine downloaded to: %cd%\RobustToolbox
echo ========================================
timeout /t 5 /nobreak