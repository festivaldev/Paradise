@echo off

set GOOS=windows
set GOARCH=386

echo Building uberdaemon for %GOOS%-%GOARCH%...

go build -o build\uberdaemon_paradise.exe src\uberdaemon.go

pause