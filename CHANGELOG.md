# Changelog for 2.1

It's been six months already since the last release of Paradise, and while some of you may have thought this project was dead again, we've been working hard on the next big update to Paradise!

This update, dubbed &lt;insert creative name here&gt;, bumps the version to 2.1 and brings a huge amount of features and quite a lot of security fixes. Please give a warm welcome to @dcrime, who helped a lot with identifying potential exploits, game-breaking bugs, testing new features and, perhaps the biggest of them all, single-handedly reverse engineering `uberdaemon` and `uberheartbeat` (more on that topic later on). Also, big shoutout to the members of our Discord server who contributed to this update with their suggestions and bug reports! This release is also the first to feature user-friendly installers for both the Client Runtime and the Server, based on Nullsoft's NSIS. No more manual file copying (unless, of course, you want to), just install and you're ready to go! Uninstalling the Paradise Client Runtime even restores the patches made to the UberStrike client, so you can easily switch between different server projects if you like (though verifying the game files through Steam is always recommended).

Because there are quite a lot of breaking changes, we're publishing this update as a beta release for everyone to test before it's going live. If you're interested in participating in the beta, please consider joining our [Discord server](https://discord.gg/5PSspzWTCJ) and checking out the `#beta-discussions` forums, in which you'll find everything you need to get started.

But for now, let's see what has changed in this update:

### Client
<details>
    <summary>Click to expand</summary>

- Added Discord Rich Presence
	- Rich Presence allows you to share your current ingame activity with other players via Discord
    - Players can also join your game with the click of a single button
    - Rich Presence is optional and only available if `Paradise.Client.DiscordRPC.exe` is located in `UberStrike_Data\Plugins`
	- See [wiki link] for more details
- Improved update process
	- Moved update process to before the authentication flow to always ensure the latest version when launching the game
	- File hashes are now always checked for each hash type (MD5, SHA256, SHA512)
	- Files can now be marked as optional, which will only get updated if they exist beforehand
	- Updated/removed files are moved to a central temporary directory before deleted on the next game start
- Starting with Update 2.1, coomunication with the Web Services is fully encrypted. Clients running an older build of Paradise (which use unencrypted Web Service communication) can still connect until this fallback is disabled.
- Reenabled sending Magic Hashes to the Realtime servers to always ensure the validity and integrity of the game files
- Fixed application working directory if UberStrike is launched via the `uberstrike://` protocol (defaulted to current user directory) which prevented `uberdaemon` from being run. Which is why it was disabled in the first place.
- Added `-console` launch parameter to open the debug console right when the game is launched. For regular players, the debug console can be closed by pressing Ctrl-Alt-D. Players with a higher rank can still toggle the console at will.
- Removed the "Have you played UberStrike before?" prompt when first starting the game, since it was used to migrate player data from Standalone to Steam (which is _long_ gone)
- Removed 2 seconds of artificial delay from client login procedure
- Fixed main menu background music not playing
- Decreased minimum mouse sensitivity to 0.1 (spruple)
- Fixed XML serialization not ignoring comments
- Expanded the `uberstrike://` protocol to open Main Menu pages (eg. `uberstrike://open/shop` to open the shop, see [wiki link] for more details)
- Reduced the initial wait time to connect to the lobby chat after the main menu has finished loading
- Game modifiers (Quick Switch, Low Gravity, etc.) are no longer reset on GUI draw calls
- Fixed server-side team assignment not being applied client-side in Team Elimination
- Re-enabled clan creation patches. If you are an admin, you can now create a clan without meeting the requirements (at least 1 friend, level 4 and a Clan License)
- Updated the Free Camera in Training mode (Ctrl-Alt-C) to be usable by every player. HUD messages and character movement will be disabled as long as Free Camera is active
- Weapon Quick Switch is now enabled by default in Training mode
- Added audible feedback when switching debug console tabs
- Updated Audio Debug to include volume and pitch controls, as well as a button to stop playing sounds and background music
- Updated to latest available NuGet packages
	- YamlDotNet: 12.0.0 -> 13.1.1
</details>

### Web Services
<details>
    <summary>Click to expand</summary>

- Keys renamed in Web Services settings
	- `WebServiceHostName` has been renamed to `Hostname`
- Keys added to Web Services settings
	- `TCPCommPort`: Port on which the Socket server listens on
    - `DatabasePath`: Allows server hosts to change the path where the database is stored
    - `PluginBlacklist`: A list of strings to prevent certain plugins from loading (without deleting or renaming them)
	- `ServerPassPhrases`: A list of passphrases for each server attempting to connect to the Socket server
```xml
<ServerPassPhrases>
	<ServerPassPhrase Id="00000000-0000-0000-0000-000000000000">0123456789abcdef</ServerPassPhrase>
</ServerPassPhrases>
```
- Keys removed from Web Services settings
	- `FileServerHostName`: Replaced by `Hostname`
- Updated CLI parameter parsing
    - Added `--silent` parameter for `--install`/`--uninstall` to suppress feedback messages
- Removed the deprecated `--gui` launch parameter (replaced by `--tray`)
- Tray application can now manually connect to the Web Services if the initial connection fails
- Added new commands (see details in the [Commands] wiki page)
	- `server`: Generates a Guid and a passphrase for new servers to be connected to a master server
    - `players`: Lists players currently online, all players or searches for a specific player
    - `rooms`: Lists or force-closes existing rooms,
    - `wallet`: Replaces the `credits` and `points` commands, and allows to view a player's current wallet
- Switched from Cmid-based command arguments to (partial) player names
    - Instead of entering a player's Cmid as a command argument, you can now use player names (partial names require at least 3 characters)
- Added Discord bot integration for lobby chat, commands and player/game notifications
    - In order to use the chat integration feature, players need to link their UberStrike profile with Discord. Please see [wiki link] for more details.
- Added Game Sessions to authenticate players
	- Sessions are valid for 12 hours, expiry time will be extended as long as the session is in use
	- Integrated garbage collector cleans unused game sessions every 5 minutes
	- Instead of encoded Steam IDs and session expiry dates, game session IDs are now used as authentication tokens
- Commands are now executed asynchronously
- Command output is now written into a buffer that can be retreived as a single string
- Commands now require a minimum rank if executed somewhere other than the Web Services console
    - Execution from outside the console is limited to admins by default, if not otherwise specified
- Added `help` description to the console help text
- The `service` command will now gracefully fail if no services are currently registered
- `ban` now removes a banned player from connected servers, regardless of where it was executed
- Split service host and services into separate plugins
    - Plugins can now include a Database client which handles table creation, querying, etc. for that service specifically
- Added encryption to Web Services
	- If incoming messages are not encrypted, service responses will also be unencrypted (fallback for older versions, will be removed in the future)
- Added TCP socket communication between Web Services and Realtime servers
- Fixed UTC dates stored in the database being converted to the local timezone
- Fixed a potential `NullReferenceException` when a HTTP router handler is called
- HTTP server is now only shut down when it was actually running
- Re-enabled Realtime server monitoring API accessible via HTTP (`/status/comm`, `/status/game`)
- Fixed various Web Service methods not returning a byte array, even if they're supposed to
- Services now use a `ServicePath` directory specific to their `ServiceVersion` (eg. `ServiceData\ApplicationWebService\2.0`
- Added service data hot reloading to Web Services
- `XpPointsUtil` now watches `ApplicationConfiguration.json` for changes, so XP calculation always reflects current level ranges
- Removed empty string `GroupTag` and `FacebookId` properties from player profile, which seemed to cause problems (daniel_crime)
- Fixed handling of player's `GroupTag` if clan membership changes
- When a player tries to register with a name that's already in use, the server will now actually suggest randomized alternatives
    - Member names are always limited to 18 characters, so generated names are truncated to fit the added randomness
- Added expiration date to items that are not purchased as "Permanent" items
- Replaced country flag icons with ones that more resemble the original UberStrike icons
	- Source: https://github.com/worlddb/world.db.flags
	- Added flags: AN (Netherlands Antilles), AQ (Antarctica), BV (Bouvet Island), EH (Western Sahara), GF (French Guiana), GS (South Georgia and the South Sandwich Islands), GY (Guyana), HM (Heard Island and McDonald Islands), MF (Saint Martin), NC (New Caledonia), PM (Saint Pierre and Miquelon), SH (Saint Helena, Ascension and Tristan da Cunha), SJ (Svalbard and Jan Mayen), SZ (Eswatini), TF (French Southern Territories), TV (Tuvalu), TZ (Tanzania), UM (United States Minor Outlying Islands), VA (Vatican), WF (Wallis and Futuna), YT (Mayotte)
	- Removed flags: BQ (Bonaire, Saint Eustatius and Saba), NATO, UN
	- Replaced flags: OK -> PK (Pakistan, fixed typo), TU -> TN (Tunisia, fixed incorrect country code)
- Replaced PowerShell script to generate update hashes with Python script
- Database error callback now includes the thrown error
- Updated profanity list (which is still English only for now)
- Improved player/clan name checks for profanity
- Fixed XML serialization not ignoring comments
- Fixed tray app window being potentially visible in the taskbar or Alt-Tab switcher
- Running `help` in the tray app console now shows every available command instead of just the default set
- Updated to latest available NuGet packages:
	- Newtonsoft.Json: 13.0.2 -> 13.0.3
	- LiteDB: 5.0.15.0 -> 5.0.17.0

#### Item Improvements
- Fixed "DIY Iron Helm of Viktor" being the wrong type of gear (`GearUpperBody` instead of `GearHead`) (dmx14)
- Adjusted Shotgun spread accuracy (selimK)

#### For Developers
- Service data is split into individual API levels
- Renamed `BanCommands` to `BanCommand`
- Removed `PublishCommMonitoringData` and `PublishGameMonitoringData` from `IApplicationWebServiceContract`
- Added `VerifyAuthToken` to `IAuthenticationWebService`
- Removed `OpPlayer` and `DeopPlayer` from `IModerationWebServiceContract`
	- Added (unused) `BanPermanently` method back instead
- Replaced `SteamMemberFromAuthToken` with `SteamMember.FromAuthToken`
- Removed `SteamMember.FromAuthToken` as SteamIDs are no longer used as auth tokens
</details>

### CommServer/GameServer
<details>
    <summary>Click to expand</summary>

- Keys renamed in Realtime settings
	- `WebServiceUrl`: Replaced by `MasterServerUrl`, `WebServicePort` and `WebServiceEndpoint`
- Keys added to Realtime settings
	- `MasterServerUrl`: The host where to connect to the master server
	- `WebServicePort`: Port to access the Web Services
	- `WebServiceEndpoint`: API version of the Web Services to connect to
	- `FileServerPort`: Port to access the file server integrated into Web Services (currently unused)
	- `TCPCommPort`: Port to connect to the socket server
	- `CommApplicationSettings`/`GameApplicationSettings`: Lets you specify the server ID (`ApplicationIdentifier`) and Rijndael encryption passphrase (`EncryptionPassPhrase`) for the Comm and Game realtime servers respectively
- Keys removed from Realtime settings
    - `CompositeHashes`/`JunkHashes`: Valid hashes are now stored in `CompositeHashes.txt` and `JunkHashes.txt` respectively, which are automatically reloaded on changes 
    - `DiscordChatIntegration`: Enables/disables sending chat events to the Discord plugin
    - `DiscordPlayerAnnouncements`: Enables/disables sending player join/leave events to the Discord plugin
    - `DiscordGameAnnouncements`: Enables/disables sending game room events to the Discord plugin
    - `DiscordErrorLog`: Enables/disables sending error logs to the Discord plugin
- Commands sent via lobby chat are now entirely handled by the Web Services
- Fixed XML serialization not ignoring comments
- Fixed a potential `InvalidOperationException` when game actor delta values are calculated
- Fixed a potential `NullReferenceException` in `PowerUpManager`
- Fixed a bunch of `KeyNotFoundException`s in `SpawnPointManager` when attempting to spawn with an invalid team (daniel_crime)
- Fixed spawnpoint randomness being exhausted after a player joins 7 times by comparing which players are actually using a spawnpoint (daniel_crime)
- Fixed a player flying exploit caused by triggering `DirectDamage` with zero bullets (daniel_crime)
- Added option to join a game as a spectator for Moderators (again)
- If all players (not connected peers in the `Overview` state) leave a room, used spawnpoints are reset
- Fixed players being able to instantly respawn (daniel_crime)
- Decreased team switch cooldown from 30 to 5 seconds
- Prevent team switch server-side if teams are unbalanced
- Fixed players not receiving damage from explosions or dying when their health reaches 0 (fixedlight)
- Fixed Armor Points not reducing below 100 if the player does not have any Armor capacity (Huntyy/ams9)
- Spawnpoints are now tracked by a peer's 'GameActor` object, which gets recreated for every game joined
- Added position tracking to powerups server-side, so only powerups actually picked up by a client are registered by the server
- Added additional client authentication when creating or joining a game room, or when kicking a player from a game
- Limited created/joined rooms per player to 1 by checking if the player is currently in any room (daniel_crime)
- Chat log is now written in to a separate log file
- Chat messages are now correctly censored and trimmed to 140 characters
- Implemented closing rooms via debug console (limited to admins)
- `XpPointsUtil` will now update it data every 15 minutes
- Implemented rate limiting to peer operation handlers to "prevent" package flooding
- Re-enabled validation of Magic Hashes sent by clients with every heartbeat
- Banned players can now no longer authenticate with Realtime servers
- Realtime monitoring data is now sent to the Web Services every 60 seconds (prev. 5 seconds) via the Socket connection instead of a SOAP endpoint
- Added encryption to Web Service clients
- Fixed incorrect return type for various Web Service client methods
- Updated to latest available NuGet packages 
	- Newtonsoft.Json: 13.0.2 -> 13.0.3

#### For Developers
- Removed unused `WebServiceBaseUrl` from `PeerConfiguration`
- Fixed accessing shared instances of Web Service clients
- Removed `PublishCommMonitoringData` and `PublishGameMonitoringData` from `ApplicationWebServiceClient`
- Added `VerifyAuthToken` to `AuthenticationWebServiceClient`
- Removed `OpPlayer` and `DeopPlayer` from `ModerationWebServiceClient`
	- Added (unused) `BanPermanently` method back instead
- Added missing `SetLoadout` method to `UserWebServiceClient`
</details>

### One more thing
Paradise is all about reverse engineering UberStrike to make it playable again. And thanks to the effort of daniel_crime, Paradise now includes a completely reverse-engineered build of `uberdaemon`, an executable in the game files responsible for calculating various file hashes and generating a so called "Magic Hash". This hash is used by the client to authenticate with the Realtime servers, which then use this hash to verify a client's file integrity. For more information on `uberdaemon`, please visit the [dedicated wiki page](https://github.com/festivaldev/Paradise/wiki/uberdaemon).  
This repo includes both the original Python code by daniel_crime, as well as the slightly modified Go code (which happens to be the langugage the original `uberdaemon` was written in) that allows the executable to be run from anywhere and print each individual hash to the console.

We've also included everything we know about [uberheartbeat](https://github.com/festivaldev/Paradise/wiki/uberheartbeat), a part of UberStrike's anticheat.