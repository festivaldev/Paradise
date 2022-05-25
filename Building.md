# Building

* Clone this repository
* Make sure required assemblies have been copied to `AssemblyReferences` (as per [[Building Requirements]])
* Open `Paradise.sln` using Visual Studio 2019 or later

## Client
Some projects are linked through build dependencies. Therefore, for the client part, (re-)building `Paradise.Client.Bootstrap` is sufficient.  
Not only will the build output be copied to `build\client\win32` (only applies to Release configuration), it's also copied to your UberStrike folder for easier deployment.

As everybody's PC is different, you (probably) will have UberStrike installed in a different location. Just change the path in the post-build scripts of both `Paradise.Client` and `Paradise.Client.Bootstrap`. Or, to disable copying the build output, remove the post-build scripts entirely.

## Realtime
The Release configuration copies the build output to `build\server\Paradise.Realtime\bin`. For testing, you can install Photon to `build\server\photon`, as seen in [[Server Installation|Installation-Server]].

## Web Services
To build the Web Services, you need to build both `Paradise.WebServices.ServiceHost` for the Windows service and `Paradise.WebServices.GUI` for the tray application. When using the Release configuration, the build output is conveniently placed inside `build\server\Paradise.WebServices`.