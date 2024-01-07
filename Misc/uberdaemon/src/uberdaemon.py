import hashlib
import sys
from os import path

DEBUG = True


if DEBUG:
    debug = print
else:
    debug = lambda *args: None


def to_sha(s):
    hash_obj = hashlib.sha256(s.encode() if isinstance(s, str) else s)
    hash_str = hash_obj.hexdigest()
    return hash_str


def read(file_path):
    if not path.exists(file_path):
        debug('File not found:', file_path)
        return 'nofile'
    debug('Found:', file_path)
    return open(file_path, 'rb').read()


def hash_files(files, assembly_path):
    d = []
    for file_path in files:
        full_path = assembly_path + '/' + file_path
        data = read(full_path)
        if isinstance(data, str):
            d.append([file_path, data])
        else:
            d.append([file_path, to_sha(data)])
    return d


args = sys.argv
executable = args[0]

working_path = ''
tmp = executable
while tmp[0].startswith('/') or tmp[0].startswith('.'):
    working_path += tmp[0]
    tmp = tmp[1:]

if len(working_path) < 2:
    working_path = '.'
else:
    working_path = working_path[:-1]

if executable.startswith(working_path):
    executable = executable[len(working_path)+1:]

debug('Executable:', executable)
debug('Working Path:', working_path)

os = (executable == 'uberdaemon.exe' and 1) or (
    executable == 'uberdaemon' and 2) or 0
if os == 1:
    if not path.exists(f'{working_path}/uberdaemon.exe'):
        print(to_sha('wrongexewin'))
        exit()

    managed_path = f'{working_path}/UberStrike_Data/Managed/'
    mono_path = f'{working_path}/UberStrike_Data/Mono/'
elif os == 2:
    if not path.exists(f'{working_path}/uberdaemon'):
        print(to_sha('wrongexeosx'))
        exit()

    managed_path = f'{working_path}/UberStrike.app/Contents/Data/Managed/'
    mono_path = f'{working_path}/UberStrike.app/Contents/Frameworks/MonoEmbedRuntime/osx/'
else:
    if executable.endswith('exe'):
        print(to_sha('wrongexewin'))
    else:
        print(to_sha('wrongexeosx'))
    exit()

# Individual hashes
managed = hash_files([
    "Assembly-CSharp-firstpass.dll",  # local_a0
    "Assembly-CSharp.dll",  # pcStack_98
    "Assembly-UnityScript-firstpass.dll",  # pcStack_90
    "Assembly-CSharp-firstpass.dll",  # pcStack_88
    "Assembly-CSharp.dll",  # pcStack_80
    "Assembly-UnityScript-firstpass.dll",  # pcStack_78
    "Boo.Lang.dll",  # pcStack_70
    "GraphLogger.dll",  # pcStack_68
    "Mono.Posix.dll",  # pcStack_60
    "Mono.Security.dll",  # pcStack_58
    "mscorlib.dll",  # pcStack_50
    "Photon3Unity3D.dll",  # pcStack_48
    "System.Configuration.dll",  # pcStack_40
    "System.Core.dll",  # pcStack_38
    "System.dll",  # pcStack_30
    "System.Security.dll",  # pcStack_28
    "System.Xml.dll",  # pcStack_20
    "UnityEngine.dll",  # pcStack_18
    "UnityEngine.UI.dll",  # pcStack_10
    "UnityScript.Lang.dll"  # pcStack_8
], managed_path)

if os == 1:
    mono = hash_files([
        "mono.dll"
    ], mono_path)
else:
    mono = hash_files([
        "libMonoPosixHelper.dylib",
        "libmono.0.dylib"
    ], mono_path)


# Common hash
full = ''.join([x[1] for x in managed + mono])
common = to_sha(full)
debug('Common Hash:', common)

# Salt hash
salt = ''.join([
    "b8700db7",  # local_40
    "3e6b30af",  # pcStack_38
    "4f180e0e",  # pcStack_30
    "3c5afb94",  # pcStack_28
    "e52de85f8",  # pcStack_20
    "9297cee7",  # pcStack_18
    "1067ee13",  # pcStack_10
    "25bc2fa"  # pcStack_8
])

salt = to_sha(salt)
debug('SALT Hash:', salt)

# Composite hash
composite = to_sha(common + salt)
debug('Composite Hash:', composite)

# Final hash
if len(sys.argv) > 1:
    final = to_sha(composite + sys.argv[1])
    debug('Magic Hash:', final)
else:
    final = to_sha('nousertoken')
    debug('No user token - Magic Hash:', final)


print(final)