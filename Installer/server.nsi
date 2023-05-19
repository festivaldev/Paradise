!include "MUI2.nsh"
!include "InstallOptions.nsh"
!include "x64.nsh"

!define INSTNAME "ParadiseServer"
!define VERSION "2.1"

Name "Paradise Server (Update ${VERSION})"
BrandingText "© 2017-2023 Team FESTIVAL"
OutFile "build\Paradise-server-win-x86_64.exe"
InstallDir "$PROGRAMFILES\Paradise"
ShowInstDetails show
ShowUninstDetails show


; Installer Defines
!define MUI_ICON "..\Web Services\Paradise.WebServices\Resources\RunningIcon.ico"
!define MUI_UNICON "..\Web Services\Paradise.WebServices\Resources\RunningIcon.ico"
!define MUI_WELCOMEFINISHPAGE_BITMAP "banner.bmp"

!define MUI_ABORTWARNING_TEXT "Are you sure you want to quit the Paradise Server Setup?"
!define MUI_ABORTWARNING_CANCEL_DEFAULT
!define MUI_ABORTWARNING

!define MUI_WELCOMEPAGE_TITLE "Welcome to the Paradise Server Setup"
!define MUI_WELCOMEPAGE_TEXT "Setup will guide you through installing the Paradise Server (Update ${VERSION}).$\r$\n$\r$\nIf you are updating an existing installation, please make sure the Web Services and the Photon Realtime servers are not running while installing Paradise.$\r$\n$\r$\nClick Next to continue."

!define MUI_LICENSEPAGE_RADIOBUTTONS
!define MUI_LICENSEPAGE_TEXT_TOP "Please read the following license agreement carefully:"
!define MUI_LICENSEPAGE_TEXT_BOTTOM "To install the Paradise Server, you must accept the agreement. Click Next to continue."

!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_FINISHPAGE_TITLE "Completing the Paradise Server Setup"
!define MUI_FINISHPAGE_TEXT "The Paradise Server (Update ${VERSION}) has been installed on your computer.$\r$\n$\r$\nFor more information on how to setup and customize Paradise, please visit the Wiki using the link below. If you need any help with running your Paradise server, feel free to join the official Paradise Discord.$\r$\n$\r$\nClick Finish to close Setup."

!define MUI_FINISHPAGE_LINK "Paradise Wiki"
!define MUI_FINISHPAGE_LINK_LOCATION "https://github.com/festivaldev/Paradise/wiki"
!define MUI_FINISHPAGE_LINK_COLOR "0066CC"

InstType "Full"
InstType "Upgrade"


; Installer Pages
!insertmacro MUI_PAGE_WELCOME

!define MUI_PAGE_HEADER_SUBTEXT "Please review the license terms before installing the Paradise Server."
!insertmacro MUI_PAGE_LICENSE "..\LICENSE"

!define MUI_PAGE_HEADER_TEXT "Choose UberStrike Location"
!define MUI_PAGE_HEADER_SUBTEXT "Choose the UberStrike installation folder in which to install the Paradise Client Runtime."
!insertmacro MUI_PAGE_DIRECTORY

!define MUI_PAGE_HEADER_SUBTEXT "Choose which features of the Paradise Server you want to install."
!insertmacro MUI_PAGE_COMPONENTS

!insertmacro MUI_PAGE_INSTFILES

!insertmacro MUI_PAGE_FINISH


; Uninstaller Defines
!define MUI_UNWELCOMEFINISHPAGE_BITMAP "banner.bmp"

!undef MUI_UNABORTWARNING_TEXT

!define MUI_UNABORTWARNING_TEXT "${MUI_ABORTWARNING_TEXT}"
!define MUI_UNABORTWARNING_CANCEL_DEFAULT
!define MUI_UNABORTWARNING

!define MUI_WELCOMEPAGE_TITLE "Welcome to the Paradise Server Uninstaller"
!define MUI_WELCOMEPAGE_TEXT "Setup will guide you through uninstalling the Paradise Server.$\r$\n$\r$\nClick Next to continue."

!define MUI_PAGE_HEADER_TEXT "Uninstall Paradise"
!define MUI_PAGE_HEADER_SUBTEXT "Remove the Paradise Server from your computer."
!define MUI_UNCONFIRMPAGE_TEXT_TOP "The Paradise Server will be uninstalled from the following folder."

!define MUI_UNFINISHPAGE_NOAUTOCLOSE
!define MUI_FINISHPAGE_TITLE "Completing the Paradise Server Uninstall"
!define MUI_FINISHPAGE_TEXT "The Paradise Server has been uninstalled successfully from computer.$\r$\n$\r$\nClick Finish to close Setup."

!define MUI_FINISHPAGE_LINK "Paradise Wiki"
!define MUI_FINISHPAGE_LINK_LOCATION "https://github.com/festivaldev/Paradise/wiki"
!define MUI_FINISHPAGE_LINK_COLOR "0066CC"


; Uninstaller Pages
!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH


!insertmacro MUI_LANGUAGE "English"


; Sections
Section "Paradise Server Runtime" ParadiseServerRuntime
	SectionIn RO

	SetOutPath "$INSTDIR\Paradise.Realtime"
	File /r /x "Cache" /x "logs" /x "ExitGames*.dll" /x "UnityEngine.dll" "..\build\server\Paradise.Realtime\*.*"

	SetOutPath "$INSTDIR\Paradise.WebServices"
	File /r /x "logs" /x "*.litedb" /x "UberStrike" /x "updates" /x "*.lnk" /x "UnityEngine.dll" /x "*.plugin" "..\build\server\Paradise.WebServices\*.*"
SectionEnd

Section "Photon Server SDK (4.0.29.11263)" PhotonSDK
	SectionIn 1
	CreateDirectory "$INSTDIR\photon"

	InitPluginsDir

	SetOutPath $PLUGINSDIR

	NSISdl::download "https://archive.org/download/photon-server-sdk_v4-0-29-11263/photon-server-sdk_v4-0-29-11263.exe" "$PLUGINSDIR\photon-server-sdk_v4-0-29-11263.exe"
	Nsis7z::Extract "$PLUGINSDIR\photon-server-sdk_v4-0-29-11263.exe"

	${If} ${RunningX64}
		CopyFiles /SILENT "$PLUGINSDIR\Photon-OnPremise-Server-SDK_v4-0-29-11263\deploy\bin_Win64\*.*" "$INSTDIR\photon\"
	${Else}
		CopyFiles /SILENT "$PLUGINSDIR\Photon-OnPremise-Server-SDK_v4-0-29-11263\deploy\bin_Win32\*.*" "$INSTDIR\photon\"
	${EndIf}

	CreateDirectory "$INSTDIR\Paradise.Realtime"

	CopyFiles /SILENT "$PLUGINSDIR\Photon-OnPremise-Server-SDK_v4-0-29-11263\lib\ExitGames.Logging.Log4Net.dll" "$INSTDIR\Paradise.Realtime\bin\"
	CopyFiles /SILENT "$PLUGINSDIR\Photon-OnPremise-Server-SDK_v4-0-29-11263\lib\ExitGamesLibs.dll" "$INSTDIR\Paradise.Realtime\bin\"
	CopyFiles /SILENT "$PLUGINSDIR\Photon-OnPremise-Server-SDK_v4-0-29-11263\lib\Photon.SocketServer.dll" "$INSTDIR\Paradise.Realtime\bin\"
	CopyFiles /SILENT "$PLUGINSDIR\Photon-OnPremise-Server-SDK_v4-0-29-11263\lib\UnityEngine.dll" "$INSTDIR\Paradise.Realtime\bin\"

	CreateDirectory "$INSTDIR\Paradise.WebServices"

	CopyFiles /SILENT "$PLUGINSDIR\Photon-OnPremise-Server-SDK_v4-0-29-11263\lib\UnityEngine.dll" "$INSTDIR\Paradise.WebServices\"

	SetOutPath $INSTDIR
SectionEnd

SectionGroup /e "Web Service Plugins" WebServicePlugins
	Section "Web Services" WSPluginServices
		SectionIn RO

		SetOutPath "$INSTDIR\Paradise.WebServices\plugins\WebServices.plugin"
		File /r "..\build\server\Paradise.WebServices\plugins\WebServices.plugin\*.*"
	SectionEnd

	Section "Legacy Web Services" WSPluginLegacy
		SectionIn 1

		SetOutPath "$INSTDIR\Paradise.WebServices\plugins\LegacyServices.plugin"
		File /r "..\build\server\Paradise.WebServices\plugins\LegacyServices.plugin\*.*"
	SectionEnd

	Section "Discord Bot Integration" WSPluginDiscord
		SectionIn 1

		SetOutPath "$INSTDIR\Paradise.WebServices\plugins\Discord.plugin"
		File /r "..\build\server\Paradise.WebServices\plugins\Discord.plugin\*.*"
	SectionEnd
SectionGroupEnd

SectionGroup /e "Setup Windows Services" Services
	Section "Web Services" WSService
		SectionIn 1
		DetailPrint "Creating service for Paradise.WebServices"
		nsExec::Exec "$INSTDIR\Paradise.WebServices\Paradise.WebServices.exe --install --silent"
	SectionEnd

	Section "Realtime Server" RTService
		SectionIn 1
		DetailPrint "Creating service for Paradise.Realtime"
		nsExec::Exec "$INSTDIR\photon\PhotonSocketServer.exe /noMessages /install ParadiseApplication /configPath $INSTDIR\photon"

		DetailPrint 'Starting service "Photon Socket Server: ParadiseApplication" (this might take a moment)'
		nsExec::Exec 'net start "Photon Socket Server: ParadiseApplication"'
	SectionEnd
SectionGroupEnd

Section "Add Firewall Rules" Firewall
	SectionIn 1
	DetailPrint "Adding Firewall rules"
	nsExec::Exec 'netsh advfirewall firewall add rule name="Paradise Server" dir=in localport="8080,8081,8082,5055,5155" protocol=TCP action=allow'
	nsExec::Exec 'netsh advfirewall firewall add rule name="Paradise Server" dir=in localport="8080,8081,8082,5055,5155" protocol=UDP action=allow'
SectionEnd

Section
	CreateDirectory "$INSTDIR\photon"
	File "/oname=$INSTDIR\photon\PhotonServer.config" "..\PhotonServer.config"

	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${INSTNAME}" "DisplayName" "Paradise Server"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${INSTNAME}" "DisplayIcon" '"$INSTDIR\uninstall.exe"'
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${INSTNAME}" "DisplayVersion" "${VERSION}"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${INSTNAME}" "Publisher" "Team FESTIVAL"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${INSTNAME}" "URLInfoAbout" "https://github.com/festivaldev/Paradise/"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${INSTNAME}" "URLUpdateInfo" "https://github.com/festivaldev/Paradise/releases"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${INSTNAME}" "HelpLink" "https://github.com/festivaldev/Paradise/wiki"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${INSTNAME}" "UninstallString" '"$INSTDIR\uninstall.exe"'
	WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${INSTNAME}" "NoModify" 1
	WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${INSTNAME}" "NoRepair" 1

	WriteUninstaller "$INSTDIR\uninstall.exe"
SectionEnd

Section "Uninstall"
	DetailPrint 'Stopping service "Photon Socket Server: ParadiseApplication" (this might take a moment)'
	nsExec::Exec 'net stop "Photon Socket Server: ParadiseApplication"'

	DetailPrint "Removing service for Paradise.Realtime"
	nsExec::Exec "$INSTDIR\photon\PhotonSocketServer.exe /noMessages /remove ParadiseApplication /configPath $INSTDIR\photon"

	DetailPrint "Removing service for Paradise.WebServices"
	nsExec::Exec "$INSTDIR\Paradise.WebServices\Paradise.WebServices.exe --uninstall --silent"

	MessageBox MB_YESNO "Would you like to keep the Web Services database?" IDYES true IDNO false
		true:
			RMDir /r "$INSTDIR\Paradise.Realtime\"

			Delete "$INSTDIR\Paradise.WebServices\*.*"
			RMDir /r "$INSTDIR\Paradise.WebServices\logs"
			RMDir /r "$INSTDIR\Paradise.WebServices\Plugins"
			RMDir /r "$INSTDIR\Paradise.WebServices\wwwroot"
			RMDir /r "$INSTDIR\Paradise.WebServices\ServiceData\ApplicationWebService"
			RMDir /r "$INSTDIR\Paradise.WebServices\ServiceData\ShopWebService"

			RMDir /r "$INSTDIR\photon"

			Delete "$INSTDIR\uninstall.exe"

			goto end
		false:
			RMDir /r "$INSTDIR"
		end:

	DetailPrint "Deleting Firewall rules"
	nsExec::Exec 'netsh advfirewall firewall delete rule name="Paradise Server" protocol=TCP'
	nsExec::Exec 'netsh advfirewall firewall delete rule name="Paradise Server" protocol=UDP'

	DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${INSTNAME}"
SectionEnd

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
!insertmacro MUI_DESCRIPTION_TEXT ${ParadiseServerRuntime} "Installs the Paradise Server."
!insertmacro MUI_DESCRIPTION_TEXT ${PhotonSDK} "Download and install the Photon Server SDK (4.0.29.11263).$\r$\n$\r$\nThis will use an additional 13.1 MB on your disk."
!insertmacro MUI_DESCRIPTION_TEXT ${WebServicePlugins} "Installs various plugins for Paradise Web Services."
!insertmacro MUI_DESCRIPTION_TEXT ${WSPluginServices} "Installs Web Services for UberStrike 4.7.1."
!insertmacro MUI_DESCRIPTION_TEXT ${WSPluginLegacy} "Installs Web Services for Legacy UberStrike (4.3.8, 4.3.9 or 4.3.10.1)."
!insertmacro MUI_DESCRIPTION_TEXT ${WSPluginDiscord} "Installs the Discord Bot Integration plugin."
!insertmacro MUI_DESCRIPTION_TEXT ${Services} "Create Windows services."
!insertmacro MUI_DESCRIPTION_TEXT ${WSService} "Create a Windows service for the Web Services that is started automatically after the system has booted."
!insertmacro MUI_DESCRIPTION_TEXT ${RTService} "Create a Windows service for the Realtime Servers that is started automatically after the system has booted."
!insertmacro MUI_DESCRIPTION_TEXT ${Firewall} "Adds Firewall rules to allow incoming connections to the Paradise servers."
!insertmacro MUI_FUNCTION_DESCRIPTION_END