@echo off
net stop "Paradise.WebServices"
taskkill /f /im Paradise.WebServices.ServiceHost.exe
sc delete "Paradise.WebServices"
pause