@echo off

set GOOS=darwin
set GOARCH=386

echo Building uberdaemon for %GOOS%-%GOARCH%...

go build -o build\uberdaemon_paradise src\uberdaemon.go

pause