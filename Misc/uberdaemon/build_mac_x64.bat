@echo off

set GOOS=darwin
set GOARCH=amd64

echo Building uberdaemon for %GOOS%-%GOARCH%...

go build -o build\uberdaemon_paradise_x64 src\uberdaemon.go

pause