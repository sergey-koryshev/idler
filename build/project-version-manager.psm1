<#
The module contains logic to retrive/set version in .NET WPF project
#>

$script:assemblyInfoPath = "src\Idler\Properties\AssemblyInfo.cs"
$script:versionRegex = "^\[assembly:\s{0}\(""(?<version>.*)""\)\]"

enum VersionType {
  AssemblyFileVersion
  AssemblyInformationalVersion
  AssemblyVersion
}

function Get-Version {
  [CmdletBinding()]
  [OutputType([string])]
  param ()

  process {
    $assemblyInfoFullPath = Join-Path (Get-Location) $script:assemblyInfoPath

    if (-not(Test-Path $assemblyInfoFullPath)) {
      throw "AssemblyInfo file '$assemblyInfoFullPath' doesn't exist"
    }

    $versionMatches = Select-String -Path $assemblyInfoFullPath -Pattern ($script:versionRegex -f [VersionType]::AssemblyFileVersion)
    
    if (($null -eq $versionMatches) -or ($null -eq $versionMatches.Matches[0].Groups["version"])) {
      throw "Version type 'AssemblyFileVersion' doesn't exist"
    }

    if ([string]::IsNullOrWhiteSpace($versionMatches.Matches[0].Groups["version"].Value)) {
      throw "Version of type 'AssemblyFileVersion' empty"
    }

    Write-Output $versionMatches.Matches[0].Groups["version"].Value
  }
}

function Set-Version {
  [CmdletBinding()]
  param (
    [string]
    $OldVersion,

    [string]
    $NewVersion
  )
  
  process {
    $assemblyInfoFullPath = Join-Path (Get-Location) $script:assemblyInfoPath

    if (-not(Test-Path $assemblyInfoFullPath)) {
      throw "AssemblyInfo file '$assemblyInfoFullPath' doesn't exist"
    }
    
    $currentBranch = . git rev-parse --abbrev-ref HEAD

    if ($null -eq $currentBranch) {
      throw "Cannot retrieve current branch name"
    }

    Write-Host "Current branch: $currentBranch"

    # dafault values for all versions
    $assemblyFileVersion = $NewVersion
    $assemblyInformationalVersion = $NewVersion
    $assemblyVersion = $NewVersion

    # by default for information version we take only three parts from version
    if ($NewVersion -match "^\d+\.\d+\.\d+") {
      $assemblyInformationalVersion = $Matches[0]
    }

    # for release branches for information version we take only two parts from version
    # if it's not the first version for the release branch then we add suffix "HFx"
    # where x - incrementing number for each hotfix which starts with '1'
    if ($currentBranch -match "release/*") {
      if ($NewVersion -match "^\d+\.\d+") {
        $suffix = [string]::Empty
        $adjustedInformationVersion = $Matches[0]
        $currentInformationVersionMatches = Select-String -Path $assemblyInfoFullPath -Pattern ($script:versionRegex -f [VersionType]::AssemblyInformationalVersion)

        if (($null -ne $currentInformationVersionMatches) -and ($null -ne $currentInformationVersionMatches.Matches[0].Groups["version"])) {
          $currentInformationVersion = $currentInformationVersionMatches.Matches[0].Groups["version"].Value

          if ($currentInformationVersion -eq $adjustedInformationVersion) {
            $suffix = " HF1"
          } elseif ($currentInformationVersion -match "HF(?<HotFixNumber>\d+)") {
            $suffix = " HF$(([int]$Matches["HotFixNumber"] + 1))"
          }
        }

        $assemblyInformationalVersion = "$adjustedInformationVersion$suffix"
      }
    }

    if ($NewVersion -match "^\d+") {
      $assemblyVersion = "$($Matches[0]).0.0.0"
    }

    Write-Host "The following versons will be written to AssemblyInfo file:"
    Write-Host "- $([VersionType]::AssemblyFileVersion): $assemblyFileVersion"
    Write-Host "- $([VersionType]::AssemblyInformationalVersion): $assemblyInformationalVersion"
    Write-Host "- $([VersionType]::AssemblyVersion): $assemblyVersion"

    Write-Host "Updating AssemblyInfo file with new versions"

    (Get-Content -Path $AssemblyInfoPath |
      Foreach-Object {
          if ($_ -match ($script:versionRegex -f [VersionType]::AssemblyFileVersion)) {
            $newLine = $_ -replace ([Regex]::Escape($Matches["version"])), $assemblyFileVersion
            Write-Output $newLine
          } elseif ($_ -match ($script:versionRegex -f [VersionType]::AssemblyInformationalVersion)) {
            $newLine = $_ -replace ([Regex]::Escape($Matches["version"])), $assemblyInformationalVersion
            Write-Output $newLine
          } elseif ($_ -match ($script:versionRegex -f [VersionType]::AssemblyVersion)) {
            $newLine = $_ -replace ([Regex]::Escape($Matches["version"])), $assemblyVersion
            Write-Output $newLine
          } else {
            Write-Output $_
          }
      }) | Set-Content $AssemblyInfoPath -Force

    Write-Host "Finished updating the file"
  }
}

function Test-BuildRunAfterRelease {
  [CmdletBinding()]
  param (
  )
  
  process {
    $currentVersion = Get-Version

    if ($currentVersion -match "^\d+\.\d+") {
      $expectedReleaseBranch = "origin/release/v$($Matches[0])"

      $existingReleaseBranch = & git branch -r --list $expectedReleaseBranch

      if ($LASTEXITCODE -ne 0) {
        throw "Error occured while looking for branch '$expectedReleaseBranch'"
      }

      $existingReleaseBranch = 

      if ([string]::IsNullOrWhiteSpace($existingReleaseBranch)) {
        Write-Host "There is no release branch '$expectedReleaseBranch'"
        Write-Output $false
      } elseif ($existingReleaseBranch.Trim() -ne $expectedReleaseBranch) {
        Write-Host "Found branch is not the same as expected one. Expected: '$expectedReleaseBranch'. Found: '$($existingReleaseBranch.Trim())'"
        Write-Output $false
      } else {
        Write-Host "Found release branch for current version: '$($existingReleaseBranch.Trim())'"
        Write-Output $true
      }
    } else {
      Write-Host "Couldn't parse current version: '$currentVersion'"
      Write-Output $false
    }
  }
}

Export-ModuleMember -Function @('Get-Version', 'Set-Version', 'Test-BuildRunAfterRelease')