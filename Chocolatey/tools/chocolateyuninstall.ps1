$packageName = "AlpehNote"
$installerType = "exe"

$folderName = "AlpehNote"
$uninstallFile = "Uninstaller.exe"

if (Test-Path "$env:ProgramFiles\$folderName") {
    Uninstall-ChocolateyPackage $packageName $installerType "/VERYSILENT /NORESTART" "$env:ProgramFiles\$folderName\$uninstallFile"
}

if (Test-Path "${env:ProgramFiles(x86)}\$folderName") {
    Uninstall-ChocolateyPackage $packageName $installerType "/VERYSILENT /NORESTART" "${env:ProgramFiles(x86)}\$folderName\$uninstallFile"
}