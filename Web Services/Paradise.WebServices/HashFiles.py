# Run in build/server/Paradise.WebServices/wwwroot/updates/v2

from datetime import datetime
import hashlib
import os
import yaml

def get_directories(path = "./"):
  return next(os.walk(path))[1]

def list_files(directory, extensions = ['.exe', '.dll', '.unity3d', '']):
    if isinstance(extensions, str):
        extensions = [extensions]
    files = []
    for dirpath, dirnames, filenames in os.walk(directory):
        for filename in filenames:
            if any(filename.endswith(ext) for ext in extensions):
                files.append(normalize_path(os.path.join(dirpath, filename)))
    return files

def md5(path):
  with open(path, 'rb', buffering=0) as f:
    return hashlib.file_digest(f, 'md5').hexdigest()

def sha256(path):
  with open(path, 'rb', buffering=0) as f:
    return hashlib.file_digest(f, 'sha256').hexdigest()

def sha512(path):
  with open(path, 'rb', buffering=0) as f:
    return hashlib.file_digest(f, 'sha512').hexdigest()
    
def normalize_path(path):
  return os.path.normpath(path).replace(os.sep, "/")


file_name = "updates"
file_suffix = "yml"
version = "2.1"

build_date = datetime.now().strftime("%y%m%d-%H%M")
build = f"4.7.1.{build_date}"

channels = [ "stable", "beta" ]
platforms = [ "win", "darwin", "universal" ]

platform_optional_files = {
  "win": [
    {
      "filename": "Paradise.Client.DiscordRPC.exe",
      "localPath": "UberStrike_Data/Plugins"
    }
  ]
}

platform_removed_files = {
  "universal": [
    {
      "filename": "uberbeat.dll",
      "localPath": "UberStrike_Data/Plugins"
    }
  ]
}

for channel in channels:
  if (os.path.exists(os.path.join("./", channel))):
    updates = {
      "version": version,
      "build": build,
      "channel": channel,
      "platforms": {}
    }
    
    for platform in platforms:
      _path = os.path.join("./", channel, platform)
      
      if (os.path.exists(_path)):
        platform_updates = {
          "platform": platform,
          "files": [],
          "removedFiles": []
        }
        
        files = list_files(_path)
        
        for file in files:
          localPath = normalize_path(os.path.dirname(os.path.relpath(file, _path)))
          remotePath =  normalize_path(os.path.dirname(file))

          file_def = {
            "filename": os.path.basename(file),
            "description": "",
            "localPath": localPath,
            "remotePath": remotePath,
            "filesize": os.stat(file).st_size,
            "md5sum": md5(file),
            "sha256": sha256(file),
            "sha512": sha512(file)
          }

          if (platform in platform_optional_files):
            if (next((x for x in platform_optional_files[platform] if x["filename"] == os.path.basename(file)), None) != None):
                file_def["optional"] = True

          platform_updates["files"].append(file_def)
        
        if (platform in platform_removed_files):   
          platform_updates["removedFiles"] = platform_removed_files[platform]
        
        updates["platforms"][platform] = (platform_updates)
    with open(f"{channel}/updates.yml", 'w') as outfile:
      yaml.dump(updates, outfile, sort_keys=False)