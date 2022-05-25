# Server Installation
## Requirements
* .NET Framework 4.7.2
* UberStrike [[installed from Steam|Installation-Client]] for required assemblies
* [Photon OnPremise Server SDK v4.0.29-11263](https://dashboard.photonengine.com/en-US/download/photon-server-sdk_v4-0-29-11263.exe) (login required)

## Instructions
* Download the [latest Server release](https://github.com/festivaldev/Paradise/releases/latest/download/Paradise-Server-win3264.zip) (Windows only)
* Extract the Zip file into a directory of your choice
* Download and extract the [Photon OnPremise Server SDK](https://dashboard.photonengine.com/en-US/download/photon-server-sdk_v4-0-29-11263.exe) to a directory of your choice. As it's a self extracting 7-Zip file, you can just open this in 7-Zip instead of running the `.exe`
* Due to legal reasons, some assembly files cannot be shipped with Paradise, so you'll need to do some manual copying
    * **Paradise.Realtime** requires the following assemblies to be copied into the `bin` folder
        * `<path to Photon>/Photon-OnPremise-Server-SDK_v4-0-29-11263/lib/`
            * ExitGames.Logging.Log4Net.dll
            * ExitGamesLibs.dll
            * Photon.SocketServer.dll
        * `<path to UberStrike>/UberStrike_Data/Managed`
            * UnityEngine.dll
    * **Paradise.WebServices** requires the following assemblies to be copied into its root folder
        * `<path to UberStrike>/UberStrike_Data/Managed`
            * UnityEngine.dll
* Copy the contents of `<path to Photon>/Photon-OnPremise-Server-SDK_v4-0-29-11263/deploy/bin_Win64/` (or `bin_Win32` for 32-bit systems) into the `photon` directory
* _Optional_: If you have a Photon license, you may copy it into the `photon` directory. If you do not add any license, you will be limited to 20 concurrent players.
* Install the Paradise Windows Service by running `Paradise.WebServices/InstallParadiseService.bat` as an administrator. The service will be started automatically.
    * To uninstall the service, simply run `DeleteParadiseService.bat` as an administrator
* _Optional_: Run `Paradise.WebServices/Paradise.WebServices.GUI.exe`.  
A UberStrike icon will appear in your system tray, allowing you to control each service, the file server (used for map icons and client updates) and the database connection.

As HTTPS/SSL is enabled by default, you may want to check [[Setting up HTTPS/SSL|Configuration-Setting-up-HTTPS-SSL]], or else clients won't be able to connect to your server at all. Alternatively, you can disable HTTPS/SSL as described in [[Server|Configuration-Server]], but it's recommended to just leave it enabled.