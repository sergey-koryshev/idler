[CmdletBinding(DefaultParameterSetName = 'Incrementation')]
param(
    [string]
    $AssemblyInfoPath,
    
    [string]
    $VersionType,
    
    [Parameter(ParameterSetName = 'SetValue')]
    [string]
    $Version,
    
    [Parameter(ParameterSetName = 'SetValues')]
    [int]
    $Major,
    
    [Parameter(ParameterSetName = 'SetValues')]
    [int]
    $Minor,
    
    [Parameter(ParameterSetName = 'SetValues')]
    [int]
    $Build,
    
    [Parameter(ParameterSetName = 'SetValues')]
    [int]
    $Revision,
    
    [Parameter(ParameterSetName = 'Incrementation')]
    [switch]
    $IncrementMajor,
    
    [Parameter(ParameterSetName = 'Incrementation')]
    [switch]
    $IncrementMinor,
    
    [Parameter(ParameterSetName = 'Incrementation')]
    [switch]
    $IncrementBuildNumber,
    
    [Parameter(ParameterSetName = 'Incrementation')]
    [switch]
    $IncrementRevision,
    
    [Parameter(ParameterSetName = 'SetValues')]
    [Parameter(ParameterSetName = 'Incrementation')]
    [string]
    $Suffix
)

Process {
    $currentVersion = & (Join-Path $PSScriptRoot 'Get-Version.ps1') -AssemblyInfoPath $AssemblyInfoPath -VersionType $VersionType
        
    $parsedVersionRegex = "(?<major>\d+)\.(?<minor>\d+)(\.(?<build>\d+))?(\.(?<revision>\d+))?(?<suffix>.*)"

    $newVersion = [string]::Empty

    if ($PsCmdlet.ParameterSetName -eq "SetValue") {
        if ($Version -notmatch $parsedVersionRegex) {
            throw "Version '$Version' has incorrect format"
        }
        if ($Version -eq $currentVersion) {
            Write-Host "Version '$VersionType' already equals to '$Version'"
            return $Version
        }
        $newVersion = $Version
    } else {
        Write-Host "Version '$VersionType' currently contains value '$currentVersion'"

        $parsedVersionMatch = $currentVersion -match $parsedVersionRegex
    
        if (!$parsedVersionMatch) {
            throw "Value '$currentVersion' cannot be parsed"
        }
    
        $newMajor = [int]$Matches["major"]
        $newMinor = [int]$Matches["minor"]
        $newBuild = $(if($Matches["build"]) { [int]$Matches["build"] })
        $newRevision = $(if($Matches["revision"]) { [int]$Matches["revision"] })
        $newSuffix = $Matches["suffix"]
        
        if ($PsCmdlet.ParameterSetName -eq "SetValues") {        
            if ($null -ne $Major -and $null -ne $newMajor) {
                $newMajor = $Major
            }
            
            if ($null -ne $Minor -and $null -ne $newMinor) {
                $newMinor = $Minor
            }
            
            if ($null -ne $Build -and $null -ne $newBuild) {
                $newBuild = $Build
            }
            
            if ($null -ne $Revision -and $null -ne $newRevision) {
                $newRevision = $Revision
            }
        }
        
        if ($PsCmdlet.ParameterSetName -eq "Incrementation") {
            if ($IncrementMajor.IsPresent -and $null -ne $newMajor) {
                $newMajor++
            }
            
            if ($IncrementMinor.IsPresent -and $null -ne $newMinor) {
                $newMinor++
            }
            
            if ($IncrementBuildNumber.IsPresent -and $null -ne $newBuild) {
                $newBuild++
            }
            
            if ($IncrementRevision.IsPresent -and $null -ne $newRevision) {
                $newRevision++
            }
        }
    
        if ($null -ne $Suffix) {
            $newSuffix = $Suffix
        }
    
        $newVersion = "{0}{1}{2}{3}{4}" -f $newMajor,
            ".$newMinor",
            $(if ($null -ne $newBuild) { ".$newBuild" } else { [string]::Empty }),
            $(if ($null -ne $newRevision) { ".$newRevision" } else { [string]::Empty }),
            $(if ($null -ne $newSuffix) { $newSuffix } else { [string]::Empty })
    }
        
    Write-Host "New version is '$newVersion'"
    
    $currentVersionLine = & (Join-Path $PSScriptRoot 'Get-Version.ps1') -AssemblyInfoPath $AssemblyInfoPath -VersionType $VersionType -OriginalLine
    Write-Host "Current line is '$currentVersionLine'"
    
    try {
        Write-Host "Saving new version in file '$AssemblyInfoPath'"
        
        (Get-Content -Path $AssemblyInfoPath | 
            Foreach-Object { 
                if ($_ -match [Regex]::Escape($currentVersionLine)) {
                    $newLine = $_ -replace ([Regex]::Escape($currentVersion)), $newVersion
                    Write-Host "Updated line is '$newLine'"
                    Write-Output $newLine
                } else {
                    Write-Output $_
                }
            }) | Set-Content $AssemblyInfoPath -Force
    }
    catch {
        Write-Error "Error has occurred while saving new version in assembly info file '$AssemblyInfoPath'"
        throw $_;
    }
    
    Write-Output $newVersion
}