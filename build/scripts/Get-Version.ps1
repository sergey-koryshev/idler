[CmdletBinding()]
param(
    [string]
    $AssemblyInfoPath,
    
    [string]
    $VersionType,
    
    [switch]
    $OriginalLine
)

Process {
    $target = if ($OriginalLine.IsPresent) { 'line' } else { 'version' }
    Write-Host "Getting $target '$VersionType' from assembly info file '$AssemblyInfoPath'"
    
    if (!(Test-Path $AssemblyInfoPath)) {
        throw "Assembly info file '$AssemblyInfoPath' doesn't exist"
    }
    
    $versionRegex = "^\[assembly:\s$VersionType\(""(?<version>.*)""\)\]"
    
    try {
        $version = Select-String -Path $AssemblyInfoPath -Pattern $versionRegex | 
            Foreach-Object { $_.Matches } | 
            Foreach-Object { @{
                Line = $_.Groups[0].Value
                Version = $_.Groups["version"].Value
            } }
    }
    catch {
        Write-Error "Error has occurred while seraching version in assembly info file '$AssemblyInfoPath'"
        throw $_;
    }
    
    if (!$version) {
        throw "Version type '$VersionType' doesn't exist in assembly info file '$AssemblyInfoPath'"
    }
    
    if ($OriginalLine.IsPresent) {
        Write-Output $version.Line
    } else {
        Write-Output $version.Version
    }
}