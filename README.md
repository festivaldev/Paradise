# Project Paradise
UberStrike Server Emulation. Reimagined.

## Background
Back in 2017, three students of Applied Computer Sciences had the idea to bring back their favorite first-person shooter called [UberStrike](https://steamdb.info/app/291210), which was shut down on June 13, 2016.  
The idea was at first to just emulate the web service responses using Windows' built-in Internet Information Services (IIS), and resulted in the first iteration of [Project Dropshot](https://github.com/festivaldev/dropshot).

When it came to realtime communication however, the three students were overwhelmed at first and stopped working on the project for a while, when @FICTURE7 took it on their own with [UberStrok](https://github.com/FICTURE7/UberStrok). Dropshot was never finished, and UberStrok, even with the fork by @SirAnuse, was never fully completed.

It was until February 2022 when the three students, since then grouped as Team FESTIVAL, took on this project again, with the goal of implementing every feature that made UberStrike this popular as truthfully as possible. This is Project Paradise.

## Installation
### Client
> *Note*: While UberStrike on macOS is technically supported by Paradise, using Windows for now is strongly recommended. Additionally, UberStrike is a 32-bit app and won't run on Catalina (10.15) or later. Also, patching is currently only supported on Windows, but may also succeed with assemblies from the macOS build.

1. Download UberStrike  
   * Using Steam:  
   Enter the following line in a Run dialog (Windows Key + R) or in your browser (requires Steam to be installed, of course):  
   ```steam://install/291210```  
   Even though UberStrike is not listed in the Steam Shop anymore, new and existing UberStrike players can still download it (as of February 2022).
   
   * Using [DepotDownloader](https://github.com/SteamRE/DepotDownloader)  
   ```DepotDownloader.exe -app 291210 -depot 291212 -username <user>```  
   Use `-depot 291212` to download the latest Windows build or `-depot 291211` for the macOS build. You will need a Steam account for downloading the game. Running without Steam requires a Steam emulator (which will not be further dicussed here).
2. Download the latest Client release archive for your platform (eg. `win32`)
3. Extract the contents (not the folder itself) of `runtime` in the archive as-is to `<path to UberStrike>\UberStrike_Data\`. Yes, that means both the `Managed` folder and the `ParadiseSettings.Client.xml`.
4. Extract the `patch` directory somewhere (preferably outside the UberStrike directory) and run Universal Unity Patcher located inside:  
   For **Assembly Directory**, select `<path to UberStrike>\UberStrike_Data\Managed\`, and for **Patch File** select the extracted `Paradise.Patch.xml`.   
   Finally, click **Patch!**

If you're playing on a locally hosted server (or any server other than default), have a look at *Configuration* → *Client settings* for details on how to switch to a different server.

### Server
1. Run `build\server\Paradise.WebServices\Paradise.WebServices.GUI.exe` as an Administrator.  
A UberStrike icon will appear in your system tray, allowing you to control each service, the File Server and the database connection.
2. Run `build\server\photon\PhotonControl.exe`. If asked for a license, you may add your own, but for testing running without any license is acceptable. Keep in mind running without a license means you're limited to 20 players (or more precisely 20 concurrent connections).
3. Open the PhotonControl tray icon, select **Paradise** and click **Start as application**.

#### Setting up HTTPS/SSL
By default, both the Web Services and the File Server are configured to run via HTTPS on `localhost`, ports `8080` and `8081` respectively. Support for HTTPS can be disabled (although not recommended) by setting `EnableSSL` to `false` in `ParadiseSettings.Server.xml`. (See *Configuration* → *Server settings*)

To allow your own server to be accessed via HTTPS, you need your SSL certificate [imported into Windows Certificate Store](https://docs.microsoft.com/en-us/biztalk/adapters-and-accelerators/accelerator-swift/adding-certificates-to-the-certificates-store-on-the-client). You also need to bind the certificates to the ports you're running the servers on, either by using [`netsh`](https://docs.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-configure-a-port-with-an-ssl-certificate?redirectedfrom=MSDN) or a little tool called [HttpConfig](http://www.stevestechspot.com/default.aspx). When binding a certificate to a port, always use `0.0.0.0` as the IP address.  
The domain or IP address the server is allowing connections to from UberStrike is configured using `WebServiceHostName` and `FileServerHostName` in `ParadiseSettings.Server.xml`. The SSL certificate name **must** match the address for either server! 

## Building
### Requirements
* All projects
	* UberStrike installed locally for assembly references.  
	Copy the following files from `<path to UberStrike>\UberStrike_Data\Managed` into `AssemblyReferences`:
		* Assembly-CSharp.dll
		* Assembly-CSharp-firstpass.dll
		* UnityEngine.dll
* Client Runtime
	* .NET Framework 3.5 (maximum supported by UberStrike)
* Web Services & File Server
	* .NET Framework 4.7.2
* Realtime Servers
	* .NET Framework 4.7.2
	* [Photon OnPremise Server SDK v4.0.29-11263](https://dashboard.photonengine.com/en-US/download/photon-server-sdk_v4-0-29-11263.exe) (self-extracting 7Zip archive)  
	Additionally, copy the following files from `Photon-OnPremise-Server-SDK_v4-0-29-11263\lib` into `AssemblyReferences`:
		* ExitGames.Logging.Log4Net.dll
		* ExitGamesLibs.dll
		* Photon.SocketServer.dll
		* PhotonHostRuntimeInterfaces
		
### Instructions
1. Clone this repository
2. Download the Photon OnPremise Server SDK (specifically v4.0.29-11263) and extract the contents of `Photon-OnPremise-Server-SDK_v4-0-29-11263\deploy\bin_Win64` (or `bin_Win32`, depending on your architecture) into the project's `build\server\photon\` directory.
3. Copy the project's `PhotonServer.config` into `build\server\photon\`.
4. Open the project solution in Visual Studio, select the **Release** build scheme and hit **Solution** → **Rebuild Solution**.  
Feel free to open an issue for any compile-time error.
5. The compiled client binaries are located in `build\client_win32`, while server binaries are located in `build\server\Paradise.Realtime\bin\` and `build\server\Paradise.WebServices`.

## Configuration
### Client settings (`ParadiseSettings.Client.xml`)
| Key | Type | Default | Description |
|---|---|---|---|
| WebServiceBaseUrl | String | `https://localhost:8080/2.0/` | The URL for the client to connect to the Web Services. If connecting to a server other than `localhost`, you need to enter its IP or FQDN, followed by the port and the API Version (always `2.0` for UberStrike 4.7.3)
| ImagePath | String | `https://localhost:8081/` | The URL for the client to download map thumbnails. If connecting to a server other than `localhost`, you need to enter its IP or FQDN, followed by the port.
| WebServicePrefix | String | UberStrike.DataCenter.WebService.CWS. | **Currently unused!** The prefix used for web service names.
| WebServiceSuffix | String | Contract.svc | **Currently unused!** The suffix used for web service names.

### Web Service settings (`ParadiseSettings.Server.xml`)
| Key | Type | Default | Description |
|---|---|---|---|
| EnableSSL | Boolean | `true` | Controls the usage of HTTPS. Requires SSL certificates to be bound to selected ports (see "Setting up HTTPS/SSL").
| WebServiceHostName | String | localhost | Sets the hostname that Web Services are listening on. Can also be an IP address or FQDN. Certificate names must match server hostnames!
| WebServicePort | Number | `8080` | Sets the port that Web Services are listening on. Allowed values are `1024`-`65535`.
| WebServicePrefix | String | UberStrike.DataCenter.WebService.CWS. | **Currently unused!** The prefix used for web service names.
| WebServiceSuffix | String | Contract.svc | **Currently unused!** The suffix used for web service names.
| FileServerHostName | String | localhost | Sets the hostname that the File Server is listening on. Can also be an IP address or FQDN. Certificate names must match server hostnames!
| FileServerPort | Number | `8081` | Sets the port that File Server listening on. Allowed values are `1024`-`65535`.

## Credits
_Developers_
* [@SniperGER](https://github.com/SniperGER)
* [@vainamov](https://github.com/vainamov)
* [@jonaszadach](https://github.com/jonaszadach)

_Supporters_  
> Please consider [donating](https://paypal.me/SniperGER) to keep the project running.  
Donations are optional and will be used to cover server and license costs.  
If you wish, your name will be listed as a supporter.  

_Used libraries_
* _Photon SDK_ by Exit Games GmbH
* _Lib.Harmony_ by Andreas Pardeike
* _LiteDB_ by Mauricio David
* _log4net_ by The Apache Software Foundation
* _Newtonsoft.Json_ by James Newton-King
* _YamlDotNet_ by Antoine Aubry

_Honorable mentions_
* [@FICTURE7](https://github.com/FICTURE7) - for initially forking Dropshot and investing a lot of time to get things working. Also, you've got us to work on UberStrok with you after we gave up on the realtime stuff, thank you very much!
* [@SirAnuse](https://github.com/SirAnuse) - for a more complete fork of UberStrok that helped us reimplement and complete the realtime part of Paradise
* Cmune (especially Shaun LeLacheur Sales) - the developers of UberStrike, and being supportive in the initial attempts of Dropshot. Also, thank you for not obfuscating the UberStrike code, otherwise this wouldn't have been possible in the first place.

_Testers_
* None so far