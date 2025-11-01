#!/bin/bash

echo "Building UiRobotSSE for Windows x64..."

# Clean previous builds
echo "Cleaning previous builds..."
dotnet clean UiRobotSSE/UiRobotSSE.csproj -c Release

# Build and publish
echo "Publishing self-contained executable..."
dotnet publish UiRobotSSE/UiRobotSSE.csproj \
    -c Release \
    -r win-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -o ./publish-win-x64

if [ $? -eq 0 ]; then
    echo "Build successful!"
    
    # Create zip package
    echo "Creating portable package..."
    zip -j UiRobotSSE-portable-win-x64.zip \
        publish-win-x64/UiRobotSSE.exe \
        publish-win-x64/appsettings.json \
        publish-win-x64/appsettings.Development.json
    
    echo "Package created: UiRobotSSE-portable-win-x64.zip"
    ls -lh UiRobotSSE-portable-win-x64.zip
else
    echo "Build failed!"
    exit 1
fi
