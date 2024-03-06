@echo off
:Prompt
cls
echo *************************************
echo * Paradise                          *
echo * (c) 2017, 2022-2024 Team FESTIVAL *
echo *************************************
echo.
echo.
echo This tool automates copying required assemblies to this project.
echo Please make sure you have installed UberStrike as described here:
echo https://github.com/festivaldev/Paradise/wiki/Installation-Downloading-UberStrike
echo.

set AssemblyTargetDir=AssemblyReferences

echo Select something to install.
echo [1] 4.7.1/Steam
echo [2] 4.3.10.1
echo [3] 4.3.9 (not supported yet)
echo [4] Photon SDK
echo [5] .NET Framework 2.0 - Required Assemblies
echo [6] .NET Framework 3.0 - Required Assemblies
echo [c] Clean AssemblyReferences
echo.
echo [0] Exit
echo.
choice /c 0123456c /n /m "Enter your choice:"

IF ERRORLEVEL 8 (
	echo Cleaning AssemblyReferences...
	del /s /q %AssemblyTargetDir%\*.dll >nul 2>&1
	goto Continue
)
IF ERRORLEVEL 7 (
	cls
	call :CopyNet30
	goto Continue
)
IF ERRORLEVEL 6 (
	cls
	call :CopyNet20
	goto Continue
)
IF ERRORLEVEL 5 (
	cls
	call :CopyPhotonSDK
	goto Continue
)
IF ERRORLEVEL 4 call :US439
IF ERRORLEVEL 3 call :US43101
IF ERRORLEVEL 2 call :US471
IF ERRORLEVEL 1 (
	exit
)

:Continue
echo.
echo Press RETURN to continue.
set /p=
goto Prompt

:Exit
echo.
echo Press RETURN to exit.
set /p=
exit

:CopyNet20
echo Copying required .NET Framework (2.0) assemblies...
echo.
echo Please confirm the path to .NET Framework 2.0 is correct, otherwise enter the correct path.
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
EXIT /B 0

:CopyNet30
echo Copying required .NET Framework (3.0) assemblies...
echo.
echo Please confirm the path to .NET Framework 3.0 is correct, otherwise enter the correct path.
set Net30Dir=C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.0
set /p Net30Dir="[%Net30Dir%]: "

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
EXIT /B 0

:CopyPhotonSDK
echo Copying Photon SDK...
echo.
echo This step requires Photon to be downloaded and extracted.
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
EXIT /B 0



:US439
cls
echo Requested 4.3.9 assemblies
echo.
echo Sorry, UberStrike 4.3.9 is not supported (yet)
goto Exit

:US43101
cls
echo Requested 4.3.10.1 assemblies
echo.
echo Please enter your UberStrike installation directory:
set USInstallPath=%PROGRAMFILES(x86)%\UberStrike
set /p USInstallPath="[%USInstallPath%]: "

set USAssemblyDirectory=UberStrike_Data\Managed
set USAssemblyList=(Assembly-CSharp.dll,Assembly-CSharp-firstpass.dll,Photon3Unity3D.dll,UberStrike.Core.Models.dll,UberStrike.Core.Serialization.dll,UberStrike.DataCenter.UnitySdk.dll,UberStrike.Realtime.UnitySdk.dll,UnityEngine.dll)

for %%A in %USAssemblyList% do (
	if not exist "%cd%\%AssemblyTargetDir%\4.3.10" (
		echo Cannot find directory: "%cd%\%AssemblyTargetDir%\4.3.10"
		pause
		exit /b 1
	)

	echo Copying "%USInstallPath%\%USAssemblyDirectory%\%%A" to %AssemblyTargetDir%\4.3.10...
	if exist "%USInstallPath%\%USAssemblyDirectory%\%%A" (
		copy "%USInstallPath%\%USAssemblyDirectory%\%%A" "%cd%\%AssemblyTargetDir%\4.3.10" >NUL
	) else (
		echo Cannot find file: "%USInstallPath%\%USAssemblyDirectory%\%%A"
		pause
		exit /b 1
	)
)

echo.
call :CopyNet20
echo.
call :CopyNet30
@REM echo.
@REM call :CopyPhotonSDK
goto Exit

:US471
cls
echo Requested 4.7.1/Steam assemblies
echo.
echo Please enter your UberStrike installation directory:
set USInstallPath=C:\Steam\steamapps\common\UberStrike
set /p USInstallPath="[%USInstallPath%]: "

set USAssemblyDirectory=UberStrike_Data\Managed
set USAssemblyList=(Assembly-CSharp.dll,Assembly-CSharp-firstpass.dll,Photon3Unity3D.dll,UnityEngine.dll)

for %%A in %USAssemblyList% do (
	if not exist "%cd%\%AssemblyTargetDir%\4.7.1" (
		echo Cannot find directory: "%cd%\%AssemblyTargetDir%\4.7.1"
		pause
		exit /b 1
	)

	echo Copying "%USInstallPath%\%USAssemblyDirectory%\%%A" to %AssemblyTargetDir%\4.7.1...
	if exist "%USInstallPath%\%USAssemblyDirectory%\%%A" (
		copy "%USInstallPath%\%USAssemblyDirectory%\%%A" "%cd%\%AssemblyTargetDir%\4.7.1" >NUL
	) else (
		echo Cannot find file: "%USInstallPath%\%USAssemblyDirectory%\%%A"
		pause
		exit /b 1
	)
)

echo.
call :CopyNet20
echo.
call :CopyNet30
echo.
call :CopyPhotonSDK
goto Exit