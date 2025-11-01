#!/bin/bash

echo "Building UiRobotSSE for macOS ARM64 (Apple Silicon)..."

# Clean previous builds
echo "Cleaning previous builds..."
dotnet clean UiRobotSSE/UiRobotSSE.csproj -c Release

# Build and publish
echo "Publishing self-contained executable..."
dotnet publish UiRobotSSE/UiRobotSSE.csproj \
    -c Release \
    -r osx-arm64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -o ./publish-osx-arm64

if [ $? -eq 0 ]; then
    echo "Build successful!"
    
    # Make executable
    chmod +x publish-osx-arm64/UiRobotSSE
    
    # Create zip package
    echo "Creating portable package..."
    zip -j UiRobotSSE-portable-osx-arm64.zip \
        publish-osx-arm64/UiRobotSSE \
        publish-osx-arm64/appsettings.json \
        publish-osx-arm64/appsettings.Development.json
    
    echo "Package created: UiRobotSSE-portable-osx-arm64.zip"
    ls -lh UiRobotSSE-portable-osx-arm64.zip
else
    echo "Build failed!"
    exit 1
fi
