Clear

$FileName = "updates"
$FileSuffix = "yaml"
$Version = "2.0"

$BuildDate = Get-Date -format "yyMMdd.HHmm"
$Build = "4.7.5.$BuildDate"

$Platforms = @("universal", "win32", "darwin32")
$Channels = @("stable", "beta")
$UpdateDirectories = @("Managed", "Maps")

echo "=== Paradise Update Definition Generator ==="
echo "Generating update definitions for version $Version ($Build)..."

ForEach ($Channel in $Channels) {
    if ((Test-Path -Path $Channel) -eq $False -And (Test-Path "$FileName-$Channel.$FileSuffix") -eq $True) {
        Remove-Item "$FileName-$Channel.$FileSuffix"
        continue
    }

	if ((Test-Path -Path "$Channel") -eq $False) {
		continue
	}

    if (Test-Path "$FileName-$Channel.$FileSuffix") {
	    Set-Content "$FileName-$Channel.$FileSuffix" ""
    }

    $Content = "platforms:"
    ForEach ($Platform in $Platforms) {
        $Content += "`n  -"
        $Content += "`n    platform: $Platform"
        $Content += "`n    version: $Version"
        $Content += "`n    build: $Build"
        $Content += "`n    files:"

        ForEach ($dir in $UpdateDirectories) {
            if (Test-Path -Path "$Channel\$Platform\$dir") {
                Get-ChildItem -Path .\$Channel\$Platform\$dir -Filter *.dll | ForEach-Object {
                    echo "Processing `"$dir\$([System.IO.Path]::GetFileName($_))`"..."
                    $Content += "`n      -"
                    $Content += "`n        filename: `"$([System.IO.Path]::GetFileName($_))`""
                    $Content += "`n        description: `"`""
                    $Content += "`n        localPath: `"$dir/`""
                    $Content += "`n        remotePath: `"$Channel/$Platform/$dir`""
                    $Content += "`n        filesize: $($_.Length)"
                    $Content += "`n        md5sum: $($(CertUtil -HashFile $_.FullName MD5)[1])"
                    $Content += "`n        sha256: $($(CertUtil -HashFile $_.FullName SHA256)[1])"
                    $Content += "`n        sha512: $($(CertUtil -HashFile $_.FullName SHA512)[1])"
                }

                Get-ChildItem -Path .\$Channel\$Platform\$dir -Filter *.unity3d | ForEach-Object {
                    echo "Processing `"$dir\$([System.IO.Path]::GetFileName($_))`"..."
                    $Content += "`n      -"
                    $Content += "`n        filename: `"$([System.IO.Path]::GetFileName($_))`""
                    $Content += "`n        description: `"`""
                    $Content += "`n        localPath: `"$dir/`""
                    $Content += "`n        remotePath: `"$Channel/$Platform/$dir`""
                    $Content += "`n        filesize: $($_.Length)"
                    $Content += "`n        md5sum: $($(CertUtil -HashFile $_.FullName MD5)[1])"
                    $Content += "`n        sha256: $($(CertUtil -HashFile $_.FullName SHA256)[1])"
                    $Content += "`n        sha512: $($(CertUtil -HashFile $_.FullName SHA512)[1])"
                }
            }
        }
    }

    Set-Content "$FileName-$Channel.$FileSuffix" $Content;
}

echo "Done."