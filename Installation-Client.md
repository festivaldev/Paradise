# Client Installation
## Requirements
* UberStrike, as described [[here|Installation-Downloading-UberStrike]]
* [UniversalUnityPatcher](https://github.com/festivaldev/UniversalUnityPatcher)

## Instructions
> :information_source: You'll only need to follow these steps once, as the Paradise client has an included update system. If an update should fail, you can still (re-)install Paradise manually.

* Download the latest Client release for your platform
    * Windows/macOS: [x86](https://github.com/festivaldev/Paradise/releases/latest/download/Paradise-Client-win32.zip)
    * Support for macOS has not been tested
* Extract the downloaded Zip file and copy its contents as is into UberStrike's game directory
    * Windows: `<path to UberStrike>\UberStrike_Data`
    * macOS: `<path to UberStrike.app>/Contents/Data`
* Download and run [UniversalUnityPatcher](https://github.com/festivaldev/UniversalUnityPatcher/releases/latest) (Windows only)
* For **Assembly Directory**, select the `Managed` directory in your UberStrike folder (see above)
* For **Patch File**, select `Paradise.Patch.xml`, which should be located in the `UberStrike_Data` directory
* Finally, click **Patch!** and let the UniversalUnityPatcher do its magic
    * While the patcher is designed to work with .NET assemblies on Windows, it should also work with the macOS binaries. However, this has not been tested and is therefore not guaranteed to work

You are now ready to play UberStrike on the official Paradise server(s)!  
If you're hosting your own server or would like to connect to an external server, please consult the [[Client Configuration|Configuration-Client]].