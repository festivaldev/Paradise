# Project Paradise
UberStrike Server Emulation. Reimagined.

## Background
Back in 2017, three students of Applied Computer Sciences had the idea to bring back their favorite first-person shooter called [UberStrike](https://steamdb.info/app/291210), which was shut down on June 13, 2016.  
The idea was at first to just emulate the web service responses using Windows' built-in Internet Information Services (IIS), and resulted in the first iteration of [Project Dropshot](https://github.com/festivaldev/dropshot).

When it came to realtime communication however, the three students were overwhelmed at first and stopped working on the project for a while, when @FICTURE7 took it on their own with [UberStrok](https://github.com/FICTURE7/UberStrok). Dropshot was never finished, and UberStrok, even with the fork by @SirAnuse, was never fully completed.

It was until February 2022 when the three students, now grouped as Team FESTIVAL, took on this project again, with the goal of implementing everything completely. This is Project Paradise.

## Installation (Client)
*Note*: While UberStrike on macOS is technically supported by Paradise, using Windows for now is strongly recommended. Additionally, UberStrike is a 32-bit app and won't run on Catalina (10.15) or later.

1. Download UberStrike  
   * Using Steam:  
   Enter the following line in a Run dialog (Windows Key + R) or in your browser (requires Steam, of course):  
   ```steam://install/291210```  
   Even though UberStrike is not listed in the Steam Shop anymore, new and existing UberStrike players can still download it (as of February 2022).
   
   * Using [DepotDownloader](https://github.com/SteamRE/DepotDownloader)  
   ```DepotDownloader.exe -app 291210 -depot 291212 -username <user>```  
   Use `-depot 291212` to download the latest Windows build or `-depot 291211` for macOS. You will need a Steam account for downloading the game. Running without Steam requires a Steam emulator (which will not be further dicussed here).
2. Download the latest release archive
3. Extract the contents (not the folder itself) of `runtime` in the archive as-is to `<path to UberStrike>\UberStrike_Data\`. Yes, that means both the `Managed` folder and the `ParadiseSettings.xml`.
4. Extract the `patch` directory somewhere and run the Universal Unity Patcher  
   For **Assembly Directory**, select `<path to UberStrike>\UberStrike_Data\Managed\`, and for **Patch File** select the extracted `Paradise.Patch.xml`.  
   You can also select which patches to apply, but it's recommended to leave everything at default.  
   Finally, click **Patch!**
5. If you're playing on a locally hosted server, you should change the `WebServiceBaseUrl` and `ImagePath` keys in `<path to UberStrike>\UberStrike_Data\ParadiseSettings.xml` to the address of the server you want to connect to (eg. `http://localhost:5053/2.0/`)

## Installation (Server)
*Note*: The servers are not yet production ready, so you'll have to build them yourself for now.

### Requirements
* All projects
	* UberStrike installed for assembly references (you may need to adapt project references to your installation directory)
* Client Runtime
	* .NET Framework 3.5 (maximum)
* Web Services
	* .NET Framework 4.7.1
* File Server, used for serving map images
	* Node.js (v16.x or later recommended)
* Realtime Servers
	* .NET Framework 4.7.1
	* [Photon OnPremise Server SDK v4.0.29-11263](https://dashboard.photonengine.com/en-US/download/photon-server-sdk_v4-0-29-11263.exe)

### Building
1. Clone this repository
2. Download the Photon OnPremise Server SDK (specifically v4.0.29-11263) and extract the contents of `Photon-OnPremise-Server-SDK_v4-0-29-11263` into the project's `photon` directory.
3. Copy the project's `PhotonServer.config` into `photon\deploy\bin_Win64\`.
4. Open the project solution in Visual Studio, adapt assembly references if needed and hit **Solution** → **Rebuild Solution**. (For compiler errors, open an issue)
5. Open a terminal window in `Paradise.WebServices.FileServer` and run `npm install`.

### Running
1. Run `Paradise.WebServices.CLI\bin\Debug\Paradise.WebServices.CLI.exe` as Administrator. For help on commands, type `help`.
2. Open a terminal window in `Paradise.WebServices.FileServer` and run `npm start`. A server should start on port 5054.
3. Run `photon\deploy\bin_Win64\PhotonControl.exe`. If asked for a license, you may add your own, but for testing running without any license is acceptable.
4. Open the PhotonControl tray icon, select **Paradise** and click **Start as application**.

## Credits
_Developers_
* @SniperGER
* @vainamov
* @jonaszadach

_Honorable mentions_
* @FICTURE7
* @SirAnuse
* Cmune, developers of UberStrike (esp. Shaun LeLacheur Sales)