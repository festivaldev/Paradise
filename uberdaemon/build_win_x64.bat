@echo off

set GOOS=windows
set GOARCH=amd64

echo Building uberdaemon for %GOOS%-%GOARCH%...

go build -o build\uberdaemon_paradise_x64.exe src\uberdaemon.go

pause