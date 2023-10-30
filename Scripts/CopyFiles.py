# Run in project root

import os
from os import path
import shutil
import sys

root = os.getcwd()
scriptroot = path.dirname(sys.argv[0])
wwwroot = path.normpath(path.join(root, "build/server/Paradise.WebServices/wwwroot/"))
update_dir = "updates/v2"
channel = "beta"

files = {
  "darwin": {
    "uberdaemon/build/uberdaemon_paradise": "",
    "uberdaemon/uberdaemon_paradise.sh": ""
  },
  "universal": {
    "build/client/Managed/0Harmony.dll": "UberStrike_Data/Managed",
    "build/client/Managed/log4net.dll": "UberStrike_Data/Managed",
    "build/client/Managed/Paradise.Client.Bootstrap.dll": "UberStrike_Data/Managed",
    "build/client/Managed/Paradise.Client.dll": "UberStrike_Data/Managed",
    "build/client/Managed/YamlDotNet.dll": "UberStrike_Data/Managed",
    "build/client/Maps/SpaceCity.unity3d": "UberStrike_Data/Maps",
    "build/client/Maps/SpacePortAlpha.unity3d": "UberStrike_Data/Maps",
    "build/client/Maps/UberZone.unity3d": "UberStrike_Data/Maps",
  },
  "win": {
    "uberdaemon/build/uberdaemon_paradise.exe": "",
    "build/client/Plugins/Paradise.Client.DiscordRPC.exe": "UberStrike_Data/Plugins",
  }
}

for platform in files:
  for file in files[platform]:
    src = path.normpath(path.join(root, file))
    dest = path.normpath(path.join(wwwroot, update_dir, channel, platform, files[platform][file], path.basename(file)))
    
    if (path.isfile(src)):
      print(src, " -> ", dest)

      os.makedirs(path.dirname(dest), exist_ok=True)
      shutil.copy(src, dest)  

print("Hashing files for v2 updates...")
os.chdir(path.normpath(path.join(wwwroot, update_dir)))
os.system(f'python "{path.join(scriptroot, "HashFiles.py")}"')

print("Hashing files for update fallback...")
os.chdir("../")
os.system(f'python "{path.join(scriptroot, "HashFiles_Fallback.py")}"')

print("Compressing updates...")
shutil.make_archive(path.join(wwwroot, "updates"), 'zip', path.join(wwwroot, "updates"))