# Downloading UberStrike

As of May 2022, it's still possible to download UberStrike from Steam, although it's not listed on the Store anymore.  
To download UberStrike, you can use either Steam or [DepotDownloader](https://github.com/SteamRE/DepotDownloader).

> :information_source: Paradise only supports the current Steam build of UberStrike (v4.7.1). Basic support for legacy versions (such as v4.3.10.1) is included, but incomplete and unsupported (see [[Legacy Support]]).

> :warning: UberStrike is a 32-bit only application and **WILL NOT** run on macOS Catalina (10.15) or later.

## Using the `steam://` protocol

* Open a browser window
    * Using a browser window is supported on both Windows and macOS
    * If you're running Windows, you can also use a "Run" dialog (Windows + R)
* Enter `steam://install/291210` and hit Enter
* Confirm the installation location in Steam and wait until the download is finished

## Using Steam's built-in console

* Open the console
    * The console can be activated by opening `steam://open/console` in a browser window or "Run" dialog, or by launching Steam with the `-console` parameter
* Execute the following command: `download_depot 291210 291212 8147831209968313515`
    * The downloaded files will be located in `<path to Steam>/steamapps/common/content/app_291210/app_291212`
    * For macOS, type `download_depot 291210 291211 7198107249301931430`
    * If there is an error that you do not own this game (or something along those lines), type `app_license_request 291210` to add UberStrike to your owned games
    * Up-to-date depot manifest IDs can be found on SteamDB: [Windows](https://steamdb.info/depot/291212/manifests/), [macOS](https://steamdb.info/depot/291211/manifests/)
* Wait for the console to print `Depot download complete`. There is no visible download progress.

## Using [DepotDownloader](https://github.com/SteamRE/DepotDownloader)

* Open a console window and change to the directory containing `DepotDownloader.exe`
* Run the following command:
    * Windows: `.\DepotDownloader.exe -app 291210 -depot 291212 -username <user>`
    * macOS: `.\DepotDownloader.exe -app 291210 -depot 291211 -username <user>`
* The downloaded files are located in `<path to DepotDownloader>\depots\29221x\696170`