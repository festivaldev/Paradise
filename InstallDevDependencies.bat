@echo off
cls
echo *************************************
echo * Paradise                          *
echo * (c) 2017, 2022-2023 Team FESTIVAL *
echo *************************************
echo.
echo.
echo This script will copy a few required assemblies to your project root directory.
echo Please make sure you have installed UberStrike as described here:
echo https://github.com/festivaldev/Paradise/wiki/Installation-Downloading-UberStrike
echo.
CHOICE /N /C:YN /M "Did you install UberStrike? (Y/N)"%1

IF ERRORLEVEL ==2 GOTO USNotInstalled
IF ERRORLEVEL ==1 GOTO USInstalled

:USNotInstalled
cls

echo Please install UberStrike according to the Paradise install instructions:
echo https://github.com/festivaldev/Paradise/wiki/ClientDownloadingUberStrike

pause
exit

:USInstalled
cls

set AssemblyTargetDir=AssemblyReferences

echo Please enter your UberStrike installation directory:
set USInstallPath=C:\Steam\steamapps\common\UberStrike
set /p USInstallPath="[%USInstallPath%]: "

set USAssemblyDirectory=UberStrike_Data\Managed
set USAssemblyList=(Assembly-CSharp.dll,Assembly-CSharp-firstpass.dll,UnityEngine.dll)

for %%A in %USAssemblyList% do (
	if not exist "%cd%\%AssemblyTargetDir%" (
		echo Cannot find directory: "%cd%\%AssemblyTargetDir%"
		pause
		exit /b 1
	)

	echo Copying "%USInstallPath%\%USAssemblyDirectory%\%%A" to %AssemblyTargetDir%...
	if exist "%USInstallPath%\%USAssemblyDirectory%\%%A" (
		copy "%USInstallPath%\%USAssemblyDirectory%\%%A" "%cd%\%AssemblyTargetDir%" >NUL
	) else (
		echo Cannot find file: "%USInstallPath%\%USAssemblyDirectory%\%%A"
		pause
		exit /b 1
	)
)

echo.
echo Copying required .NET Framework (2.0) assemblies...
echo Please confirm the path to .NET 2.0 is correct
set Net20Dir=C:\Windows\Microsoft.NET\Framework64\v2.0.50727
set /p Net20Dir="[%Net20Dir%]: "

set Net20AssemblyList=(System.Transactions.dll)

for %%A in %Net20AssemblyList% do (
	if not exist "%cd%\%AssemblyTargetDir%" (
		echo Cannot find directory: "%cd%\%AssemblyTargetDir%"
		exit /b 1
	)

	echo Copying %%A to %AssemblyTargetDir%...
	if exist "%Net20Dir%\%%A" (
		copy "%Net20Dir%\%%A" "%cd%\%AssemblyTargetDir%" >NUL
	) else (
		echo Cannot find file: "%Net20Dir%\%%A"
		exit /b 1
	)
)

echo.
echo Copying required .NET Framework (3.0) assemblies...
set Net30Dir=C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.0
set Net30AssemblyList=(System.IdentityModel.dll,System.Runtime.Serialization.dll,System.ServiceModel.dll)

for %%A in %Net30AssemblyList% do (
	if not exist "%cd%\%AssemblyTargetDir%" (
		echo Cannot find directory: "%cd%\%AssemblyTargetDir%"
		exit /b 1
	)

	echo Copying %%A to %AssemblyTargetDir%...
	if exist "%Net30Dir%\%%A" (
		copy "%Net30Dir%\%%A" "%cd%\%AssemblyTargetDir%" >NUL
	) else (
		echo Cannot find file: "%Net20Dir%\%%A"
		exit /b 1
	)
)

echo.
echo This next step requires Photon to be downloaded and extracted.
echo Photon can be downloaded from https://dashboard.photonengine.com/en-US/download/photon-server-sdk_v4-0-29-11263.exe (extract it using 7-Zip)
echo Please enter the path to your local Photon installation:
set PhotonInstallPath=%userprofile%\Downloads\Photon-OnPremise-Server-SDK_v4-0-29-11263
set /p PhotonInstallPath="[%PhotonInstallPath%]: "

set PhotonLibDirectory=lib
set PhotonAssemblyList=(ExitGames.Logging.Log4Net.dll,ExitGamesLibs.dll,Photon.SocketServer.dll,PhotonHostRuntimeInterfaces.dll)
set PhotonTargetDirs=(AssemblyReferences)

for %%A in %PhotonAssemblyList% do (
	if not exist "%cd%\%AssemblyTargetDir%" (
		echo Cannot find directory: "%cd%\%AssemblyTargetDir%"
		exit /b 1
	)

	echo Copying %%A to %AssemblyTargetDir%...
	if exist "%PhotonInstallPath%\%PhotonLibDirectory%\%%A" (
		copy "%PhotonInstallPath%\%PhotonLibDirectory%\%%A" "%cd%\%AssemblyTargetDir%" >NUL
	) else (
		echo Cannot find file: "%PhotonInstallPath%\%PhotonLibDirectory%\%%A"
		exit /b 1
	)
)

echo DONE.
pause