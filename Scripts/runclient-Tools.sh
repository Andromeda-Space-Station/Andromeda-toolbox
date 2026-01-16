#!/usr/bin/env bash
# Go up one directory to Andromeda-toolbox root
cd "$(dirname "$0")/.."
dotnet run --project Content.Client --configuration Tools