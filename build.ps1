$ErrorActionPreference = "Stop"

dotnet tool restore
dotnet build

AddToPath Clipboard2TTS/bin/Debug
