# Run in build/server/Paradise.WebServices/wwwroot/updates

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
platforms = [ "universal", "win", "darwin" ]

platform_removed_files = {
  "universal": [
    {
      "filename": "uberbeat.dll",
      "localPath": "Plugins"
    }
  ]
}

for channel in channels:
  if (os.path.exists(os.path.join("./v2", channel))):
    updates = {
      "platforms": []
    }
    
    for platform in platforms:
      _path = os.path.join("./v2", channel, platform)
      _start = os.path.join("./v2", channel, platform, "UberStrike_Data")
      
      if (os.path.exists(_path)):
        platform_updates = {
          "platform": platform if platform == "universal" else platform + "32",
          "version": version,
          "build": build,
          "files": [],
          "removedFiles": []
        }
        
        files = list_files(_path)
        
        for file in files:
          localPath = normalize_path(os.path.dirname(os.path.relpath(file, _start)))
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
          
          platform_updates["files"].append(file_def)
        
        if (platform in platform_removed_files):   
          platform_updates["removedFiles"] = platform_removed_files[platform]
      
      updates["platforms"].append((platform_updates))
    
    with open(f"./updates-{channel}.yaml", 'w') as outfile:
      yaml.dump(updates, outfile, sort_keys=False)
    