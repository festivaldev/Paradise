# Building Requirements

For building each Paradise project, you'll need the following software:  
* Windows 7 or later (Windows 10 recommended)
* Visual Studio 2019 or later (configured for **.NET Desktop Development**)

Some projects also have additional dependencies, that for legal reasons cannot be redistributed. Copy the following files to `AssemblyReferences`:  
* `<path to UberStrike>\UberStrike_Data\Managed`
    * Assembly-CSharp.dll
    * Assembly-CSharp-firstpass.dll
    * UnityEngine.dll
* [Photon OnPremise Server SDK v4.0.29-11263](https://dashboard.photonengine.com/en-US/download/photon-server-sdk_v4-0-29-11263.exe)
    * `Photon-OnPremise-Server-SDK_v4-0-29-11263\lib`
        * ExitGames.Logging.Log4Net.dll
		* ExitGamesLibs.dll
		* Photon.SocketServer.dll
		* PhotonHostRuntimeInterfaces.dll