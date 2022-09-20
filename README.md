# Project Paradise
UberStrike Server Emulation. Reimagined.

[Announcement tweet](https://twitter.com/Sniper_GER/status/1532027810792996864)

## Background
Back in 2017, three students of Applied Computer Sciences had the idea to bring back their favorite first-person shooter called [UberStrike](https://steamdb.info/app/291210), which was shut down on June 13, 2016.  
The idea was at first to just emulate the web service responses using Windows' built-in Internet Information Services (IIS), and resulted in the first iteration of [Project Dropshot](https://github.com/festivaldev/dropshot).

When it came to realtime communication however, the three students were overwhelmed at first and stopped working on the project for a while, when @FICTURE7 took it on their own with [UberStrok](https://github.com/FICTURE7/UberStrok). Dropshot was never finished, and UberStrok, even with the fork by @SirAnuse, was never fully completed.

It was until February 2022 when the three students, since then grouped as Team FESTIVAL, took on this project again, with the goal of implementing every feature that made UberStrike this popular as truthfully as possible. This is Project Paradise.

## Installation
See the wiki for installing the [client runtime](https://github.com/festivaldev/Paradise/wiki/Installation-Client) or how to [set up](https://github.com/festivaldev/Paradise/wiki/Installation-Server) your own server.

## Building
Instructions on how to build the Paradise components yourself can be found [here](https://github.com/festivaldev/Paradise/wiki/Building).

## Configuration
A list of available [client](https://github.com/festivaldev/Paradise/wiki/Configuration-Client) and [server](https://github.com/festivaldev/Paradise/wiki/Configuration-Server) configuration options can be found in the wiki.

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
* _ProfanityDetector_ by stephenhaunts

_Honorable mentions_
* [@FICTURE7](https://github.com/FICTURE7) - for initially forking Dropshot and investing a lot of time to get things working. Also, you've got us to work on UberStrok with you after we gave up on the realtime stuff, thank you very much!
* [@SirAnuse](https://github.com/SirAnuse) - for a more complete fork of UberStrok that helped us reimplement and complete the realtime part of Paradise
* Cmune (especially Shaun LeLacheur Sales) - the developers of UberStrike, and being supportive in the initial attempts of Dropshot. Also, thank you for not obfuscating the UberStrike code, otherwise this wouldn't have been possible in the first place.