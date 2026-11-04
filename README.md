# Project Paradise
UberStrike Server Emulation. Reimagined.

[![GitHub Release](https://img.shields.io/github/v/release/festivaldev/Paradise)](https://github.com/festivaldev/Paradise/releases/latest)
[![GitHub Issues](https://img.shields.io/github/issues/festivaldev/Paradise)](https://github.com/festivaldev/Paradise/issues)
[![GitHub Build Status](https://img.shields.io/github/actions/workflow/status/festivaldev/Paradise/build.yml)](https://github.com/festivaldev/Paradise/actions/workflows/build.yml)
[![GitHub Repo stars](https://img.shields.io/github/stars/festivaldev/Paradise)](https://github.com/festivaldev/Paradise/stargazers)
[![GitHub watchers](https://img.shields.io/github/watchers/festivaldev/Paradise)](https://github.com/festivaldev/Paradise/watchers)
[![GitHub forks](https://img.shields.io/github/forks/festivaldev/Paradise)](https://github.com/festivaldev/Paradise/forks)

[![Paradise UberStrike Discord Server](https://discordapp.com/api/guilds/1071142989579178116/widget.png?style=banner2)](https://discord.gg/5PSspzWTCJ)

## Background
Back in 2017, three students of Applied Computer Sciences had the idea to bring back their favorite first-person shooter called [UberStrike](https://steamdb.info/app/291210), which was shut down by Cmune on June 13, 2016.  
First, the idea was to just emulate the web service responses using Windows' built-in Internet Information Services (IIS), and resulted in the first iteration of [Project Dropshot](https://github.com/festivaldev/dropshot).

When the time came to implement everything realtime related (ingame chat and actual gameplay), these three students were overwhelmed at first by the Photon implementation, when @FICTURE7 stepped in and had their own go with [UberStrok](https://github.com/FICTURE7/UberStrok) ([forked](https://github.com/ashley-dv/UberStrok) by @ashley-dv). Unfortunately, Project Dropshot was never finished, and UberStrok was never fully completed.

Then came Februrary 2022 when the same three students, since then grouped as _Team FESTIVAL_, took on this project once more, with the goal of implementing every feature that made UberStrike as popular as it was as truthfully as possible, sprinkled with some quality-of-life fixes and improvements. This is Project Paradise ⁠⁠– now featuring fully implemented web services for the latest Steam build (and partial support for v4.3.10.1) as well as completed realtime server implementations.

> :information_source: The wiki is currently being rebuilt. Some information may be outdated. Please proceed with caution, and if something is unclear, feel free to ask on the Paradise Discord server.

## Installation
### Client
Installing the Paradise client requires a working build of UberStrike. Don't have UberStrike installed yet? See [Downloading UberStrike](https://github.com/festivaldev/Paradise/wiki/ClientDownloadingUberStrike) first.  
Client binaries are available for the following platforms:
- Windows (x86): [.exe](https://github.com/festivaldev/Paradise/releases/latest/download/Paradise-client-win-x86.exe) or [.zip](https://github.com/festivaldev/Paradise/releases/latest/download/Paradise-client-win-x86.zip)
- macOS (Intel): [.zip](https://github.com/festivaldev/Paradise/releases/latest/download/Paradise-client-darwin-x64.zip)

ZIP installation requires a one-time [manual patch](https://github.com/festivaldev/Paradise/wiki/ClientInstallingParadise#manual-instructions) of the game. Using the installer does not require manual patching.

### Server
Server binaries are available for the following platforms:
- Windows (x64): [.exe](https://github.com/festivaldev/Paradise/releases/latest/download/Paradise-server-win-x86_64.exe) or [.zip](https://github.com/festivaldev/Paradise/releases/latest/download/Paradise-server-win-x86_64.zip)
- Windows (ARM64): [.exe](https://github.com/festivaldev/Paradise/releases/latest/download/Paradise-server-win-aarch64.exe) or [.zip](https://github.com/festivaldev/Paradise/releases/latest/download/Paradise-server-win-aarch64.zip)
- macOS (Intel): [.zip](https://github.com/festivaldev/Paradise/releases/download/latest/Paradise-server-darwin-x86_64.zip)
- macOS (Apple Silicon): [.zip](https://github.com/festivaldev/Paradise/releases/download/latest/Paradise-server-darwin-aarch64.zip)
- Linux (x64): [.zip](https://github.com/festivaldev/Paradise/releases/download/latest/Paradise-server-linux-x86_64.zip)
- Linux (ARM64): [.zip](https://github.com/festivaldev/Paradise/releases/download/latest/Paradise-server-linux-aarch64.zip)

For server setup instructions, please see [Server Setup](https://github.com/festivaldev/Paradise/wiki/ServerSetup).

## Building
Find instructions for building each of Paradise's components below:

- [Client](#), [Bootstrap](#) and [Discord Rich Presence](#)
- [Web Services](#)
- Realtime Servers ([Comm](#)/[Game](#))
- [Docker images](#)
- Web UI (coming soon)

## Moving on from Photon
Paradise builds before v2.2.0 relied on Photon, a proprietary realtime networking solution developed by Exit Games GmbH, using a paid (or free, but severely limited) licensing model. The [NekoNexus fork](https://github.com/NekoSune-GameServers-MP-Mods/NekoNexus), maintained by [@NekoSuneVR](https://github.com/NekoSuneVR), implements shims to replace Photon entirely and translates the game's realtime events to a customized protocol using [LiteNetLib](https://github.com/RevenantX/LiteNetLib/tree/v0.9.5.2). With permission from @NekoSuneVR, Paradise now also implements this solution, enabling developers, server owners and of course players to not worry about any kind of limitation Photon brings with their licensing model. Using this solution, Paradise can now be run completely independent of any platform without relying on Windows to run the realtime servers.

## Credits
_Developers_
* [@SniperGER](https://github.com/SniperGER)
* [@vainamov](https://github.com/vainamov)
* [@jonaszadach](https://github.com/jonaszadach)

_Contributors_
* [@dcrime](https://github.com/dcrime)
* [@NekoSuneVR](https://github.com/NekoSuneVR)
* [@AzisPT](https://github.com/AzisPT)

_Used libraries_  
Please see [Acknowledgements.txt](./Acknowledgements.txt) for information about the libraries used in Paradise.