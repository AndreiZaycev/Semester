#!/usr/bin/env bash

# Reset
Color_Off='\033[0m'       # Text Reset

# Regular Colors
Red='\033[0;31m'          # Red
Cyan='\033[0;36m'         # Cyan

set -eu
set -o pipefail

printf "${Red}[info] ${Cyan}Running 'dotnet tool restore'...${Color_Off}\n"
dotnet tool restore

printf "${Red}[info] ${Cyan}Adding token for calculator...${Color_Off}\n"
dotnet paket config add-token "https://nuget.pkg.github.com/kirillgarbar/index.json" "$NUGET_AUTH_TOKEN"

printf "${Red}[info] ${Cyan}Running 'dotnet paket restore'...${Color_Off}\n"
dotnet paket restore

printf "${Red}[info] ${Cyan}Running 'dotnet fake build -t \"\$@\" with specified flags'...${Color_Off}\n"
# Set flags for paket and fake-cli
PAKET_SKIP_RESTORE_TARGETS=true
FAKE_DETAILED_ERRORS=true

# Start fake-cli and build repository with targets
dotnet fake build -t "$@"
