#!/usr/bin/env bash

echo "========================================"
echo "Updating RobustToolbox Engine"
echo "========================================"

cd "$(dirname "$0")/.." || exit 1
echo "Working in: $(pwd)"
echo "========================================"

# Check Git
if ! command -v git >/dev/null 2>&1; then
    echo "ERROR: Git is not installed!"
    echo "Install with: sudo apt install git  # for Debian/Ubuntu"
    echo "or visit: https://git-scm.com/downloads"
    exit 1
fi

# Check if engine exists
if [ -d "RobustToolbox" ]; then
    echo "RobustToolbox found, updating..."
    cd RobustToolbox || exit 1
    
    echo "Stashing local changes if any..."
    git stash 2>/dev/null
    
    echo "Pulling latest changes..."
    git pull --recurse-submodules
    
    if [ $? -ne 0 ]; then
        echo "WARNING: Git pull failed, attempting fresh clone..."
        cd ..
        rm -rf RobustToolbox 2>/dev/null
    else
        echo "Updating submodules..."
        git submodule update --init --recursive
        
        echo "========================================"
        echo "Engine updated in: $(pwd)"
        echo "========================================"
        sleep 5
        exit 0
    fi
else
    echo "RobustToolbox not found, downloading..."
fi

# Download fresh if doesn't exist or pull failed
echo "Downloading RobustToolbox..."
git clone --recurse-submodules https://github.com/space-wizards/RobustToolbox.git

if [ $? -ne 0 ]; then
    echo "ERROR: Download failed!"
    exit 1
fi

echo "========================================"
echo "Engine downloaded to: $(pwd)/RobustToolbox"
echo "========================================"
sleep 5