$ErrorActionPreference = 'Stop';


$toolsDir     = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$file         = Join-Path $toolsDir "AutoUpdater.exe"

$version = '1.7.0'

$packageArgs = @{
  packageName   = $env:ChocolateyPackageName
  softwareName  = 'alpehnote*'
  fileType      = 'zip'
  silentArgs    = "/S"

  validExitCodes= @(0)
  url           = "https://github.com/Mikescher/AlephNote/releases/download/v$version/AlephNote.zip"
  checksum      = '331F3E5AFDCB6B8860B6C4C507BF99CB7DA67757C7FA71112EC3832A751F3A68'
  checksumType  = 'sha256'
  destination   = $toolsDir
}

$installArgs = @{
  packageName    = $packageName
  file           = $file

  silentArgs     = '--silent --pkgmanager=choco'
  validExitCodes = @(0)
}



Install-ChocolateyZipPackage @packageArgs

Install-ChocolateyInstallPackage @installArgs

