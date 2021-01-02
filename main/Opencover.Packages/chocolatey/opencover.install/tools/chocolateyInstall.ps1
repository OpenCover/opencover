$packageName = 'opencover.install'
$installerType = 'msi'
$silentArgs= '/quiet'
$url = 'https://github.com/OpenCover/opencover/releases/download/4.7.0/opencover.4.7.0.msi'
$checkSum = "812AE75B079368D74AA09282B72D5894BBA666808B3D3C3A822381B3D504BFD3"
Install-ChocolateyPackage "$packageName" "$installerType" "$silentArgs" "$url" -validExitCodes @(0) -ChecksumType "sha256" -Checksum $checkSum