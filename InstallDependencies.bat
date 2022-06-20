@echo off
cls
echo ********************************
echo * Paradise                     *
echo * (c) 2017, 2022 Team FESTIVAL *
echo ********************************
echo.
echo.
echo This script will copy a few required assembly references to your project directory.
echo Please make sure you have installed UberStrike as described here:
echo https://github.com/festivaldev/Paradise/wiki/Installation-Downloading-UberStrike
echo.
CHOICE /N /C:YN /M "Did you install UberStrike? (Y/N)"%1

IF ERRORLEVEL ==2 GOTO USNotInstalled
IF ERRORLEVEL ==1 GOTO USInstalled

:USInstalled
cls
echo Please enter your UberStrike installation directory:
set USInstallPath=C:\Steam\steamapps\common\UberStrike
set /p USInstallPath="[%USInstallPath%]: "

set USAssemblyDirectory=UberStrike_Data\Managed
set USAssemblyList=(Assembly-CSharp.dll,Assembly-CSharp-firstpass.dll,UnityEngine.dll)
set USTargetDirs=(AssemblyReferences)

for %%D in %USTargetDirs% do (
	for %%A in %USAssemblyList% do (
		if not exist "%cd%\%%D" (
			echo Cannot find directory: "%cd%\%%D"
			exit /b 1
		)

		echo Copying %%A to %%D...
		if exist "%USInstallPath%\%USAssemblyDirectory%\%%A" (
			copy "%USInstallPath%\%USAssemblyDirectory%\%%A" "%cd%\%%D" >NUL
		) else (
			echo Cannot find file: "%USInstallPath%\%USAssemblyDirectory%\%%A"
			exit /b 1
		)
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

for %%D in %PhotonTargetDirs% do (
	for %%A in %PhotonAssemblyList% do (
		if not exist "%cd%\%%D" (
			echo Cannot find directory: "%cd%\%%D"
			exit /b 1
		)

		echo Copying %%A to %%D...
		if exist "%PhotonInstallPath%\%PhotonLibDirectory%\%%A" (
			copy "%PhotonInstallPath%\%PhotonLibDirectory%\%%A" "%cd%\%%D" >NUL
		) else (
			echo Cannot find file: "%PhotonInstallPath%\%PhotonLibDirectory%\%%A"
			exit /b 1
		)
	)
)

echo DONE.

exit /b 0

:USNotInstalled
cls
echo You need a local UberStrike installation in order to copy assemblies.
echo Please install UberStrike as described here:
echo https://github.com/festivaldev/Paradise/wiki/Installation-Downloading-UberStrike
exit /b 1