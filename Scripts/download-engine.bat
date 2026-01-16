@echo off
echo ========================================
echo FORCE Downloading RobustToolbox Engine
echo ========================================

cd ..
echo Working in: %cd%
echo ========================================

where git >nul 2>nul || (
    echo ERROR: Git is not installed!
    echo Install from: https://git-scm.com/downloads
    pause
    exit /b 1
)

REM ALWAYS delete first
echo Deleting old RobustToolbox (if exists)...
rmdir /s /q RobustToolbox 2>nul
timeout /t 3 /nobreak >nul

REM Try one more time with PowerShell
if exist RobustToolbox (
    powershell -Command "Remove-Item -Path 'RobustToolbox' -Recurse -Force -ErrorAction SilentlyContinue"
    timeout /t 2 /nobreak >nul
)

REM If still exists, rename it
if exist RobustToolbox (
    echo WARNING: Cannot delete, renaming...
    ren RobustToolbox RobustToolbox_old 2>nul
)

REM Download fresh
echo Downloading fresh RobustToolbox...
git clone --recurse-submodules https://github.com/space-wizards/RobustToolbox.git

if errorlevel 1 (
    echo ERROR: Download failed!
    echo Please delete RobustToolbox folder manually and try again.
    pause
    exit /b 1
)

echo ========================================
echo Engine downloaded successfully!
echo ========================================@echo off
echo ========================================
echo FORCE Downloading RobustToolbox Engine
echo ========================================

cd ..
echo Working in: %cd%
echo ========================================

where git >nul 2>nul || (
    echo ERROR: Git is not installed!
    echo Install from: https://git-scm.com/downloads
    pause
    exit /b 1
)

REM ALWAYS delete first
echo Deleting old RobustToolbox (if exists)...
rmdir /s /q RobustToolbox 2>nul
timeout /t 3 /nobreak >nul

REM Try one more time with PowerShell
if exist RobustToolbox (
    powershell -Command "Remove-Item -Path 'RobustToolbox' -Recurse -Force -ErrorAction SilentlyContinue"
    timeout /t 2 /nobreak >nul
)

REM If still exists, rename it
if exist RobustToolbox (
    echo WARNING: Cannot delete, renaming...
    ren RobustToolbox RobustToolbox_old 2>nul
)

REM Download fresh
echo Downloading fresh RobustToolbox...
git clone --recurse-submodules https://github.com/space-wizards/RobustToolbox.git

if errorlevel 1 (
    echo ERROR: Download failed!
    echo Please delete RobustToolbox folder manually and try again.
    pause
    exit /b 1
)

echo ========================================
echo Engine downloaded successfully!
echo ========================================