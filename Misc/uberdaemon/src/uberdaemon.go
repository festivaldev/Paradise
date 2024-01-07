/// uberdaemon
/// Reverse engineed as Python code by daniel_crime
/// Modified and translated to Go by Sniper_GER


package main

import (
    "crypto/sha256"
	"encoding/hex"
	"flag"
    "fmt"
    "io/ioutil"
    "os"
    "path/filepath"
	"runtime"
	"strings"
)

var DEBUG = false

func debug(args ...interface{}) {
    if DEBUG {
        fmt.Println(args...)
    }
}

func to_sha(s interface{}) string {
	var data []byte
	switch v := s.(type) {
	case string:
		data = []byte(v)
	case []byte:
		data = v
	}
	hash_obj := sha256.Sum256(data)
	hash_str := hex.EncodeToString(hash_obj[:])
	return hash_str
}

func read(file_path string) []byte {
    if _, err := os.Stat(file_path); os.IsNotExist(err) {
        debug("File not found:", file_path)
        return []byte("nofile")
    }
    debug("Found:", file_path)
    data, err := ioutil.ReadFile(file_path)
    if err != nil {
        debug("Error reading file:", file_path)
        return []byte("nofile")
    }
    return data
}

func hash_files(files []string, assembly_path string) [][]string {
    d := [][]string{}
    for _, file_path := range files {
        full_path := filepath.Join(assembly_path, file_path)
        data := read(full_path)
        if string(data) == "nofile" {
            d = append(d, []string{file_path, "nofile"})
        } else {
            d = append(d, []string{file_path, to_sha(data)})
        }
    }
    return d
}

func main() {
	debugFlag := flag.Bool("d", false, "Enable debugging")
    flag.Parse()

    if *debugFlag {
        DEBUG = true
    }

    args := os.Args
    executable := args[0]

	working_path, _ := os.Getwd()

	debug(runtime.GOOS)

    if strings.HasPrefix(executable, working_path) {
        executable = executable[len(working_path)+1:]
    }

    debug("Executable:", executable)
    debug("Working Path:", working_path)

    var _os int
    if runtime.GOOS == "windows" {
        _os = 1
    } else if runtime.GOOS == "darwin" {
        _os = 2
    }

    var managed_path, mono_path string
    if _os == 1 {
        managed_path = filepath.Join(working_path, "UberStrike_Data", "Managed")
        mono_path = filepath.Join(working_path, "UberStrike_Data", "Mono")
    } else if _os == 2 {
        if _, err := os.Stat(filepath.Join(working_path, "uberdaemon")); err != nil {
            fmt.Println(to_sha("wrongexeosx"))
            os.Exit(1)
        }

        managed_path = filepath.Join(working_path, "UberStrike.app", "Contents", "Data", "Managed")
        mono_path = filepath.Join(working_path, "UberStrike.app", "Contents", "Frameworks", "MonoEmbedRuntime", "osx")
    } else {
		fmt.Println(to_sha("wrongplatform"))
        os.Exit(1)
    }

	debug("Managed Path:", managed_path)
	debug("Mono Path:", mono_path)

	// Individual hashes
	managed := hash_files([]string{
		"0Harmony.dll",
		"Assembly-CSharp-firstpass.dll",
		"Assembly-CSharp.dll",
		"Assembly-UnityScript-firstpass.dll",
		"Assembly-CSharp-firstpass.dll",
		"Assembly-CSharp.dll",
		"Assembly-UnityScript-firstpass.dll",
		"Boo.Lang.dll",
		"GraphLogger.dll",
		"Mono.Posix.dll",
		"Mono.Security.dll",
		"mscorlib.dll",
		"Paradise.Client.Bootstrap.dll",
		"Paradise.Client.dll",
		"Photon3Unity3D.dll",
		"System.Configuration.dll",
		"System.Core.dll",
		"System.dll",
		"System.Security.dll",
		"System.Xml.dll",
		"UnityEngine.dll",
		"UnityEngine.UI.dll",
		"UnityScript.Lang.dll",
		"YamlDotNet.dll",
    }, managed_path)

    var mono [][]string
    if _os == 1 {
        mono = hash_files([]string{
            "mono.dll",
        }, mono_path)
    } else {
        mono = hash_files([]string{
            "libMonoPosixHelper.dylib",
            "libmono.0.dylib",
        }, mono_path)
    }

    full := ""
    for _, x := range append(managed, mono...) {
        full += x[1]
    }

    common := to_sha(full)
    debug("Common Hash:", common)

	// Salt hash
	salt := strings.Join([]string {
		"b8700db7",
		"3e6b30af",
		"4f180e0e",
		"3c5afb94",
		"e52de85f8",
		"9297cee7",
		"1067ee13",
		"25bc2fa",
	}, "")

    salt = to_sha(salt)
    debug("SALT Hash:", salt)

	// Composite hash
    composite := to_sha(common + salt)
    debug("Composite Hash:", composite)

	// Final hash
    var final string
    if len(os.Args) > 1 {
        final = to_sha(composite + os.Args[1])
        debug("Magic Hash:", final)
    } else {
        final = to_sha("nousertoken")
        debug("No user token - Magic Hash:", final)
    }

    fmt.Println(final)
}