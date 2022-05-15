@echo off
sc create "Paradise.WebServices" DisplayName="Paradise Web Services" start="auto" binPath="%~dp0\Paradise.WebServices.ServiceHost.exe"
sc description "Paradise.WebServices" "Service for running the Web Services API and File Server used by Paradise, a UberStrike Server implementation"
net start "Paradise.WebServices"
pause