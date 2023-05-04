[CmdletBinding()]
param(
    [string]
    $SHA,
    
    [string]
    $Owner,
    
    [string]
    $Repository
)

begin {
  $getLabelsUrl = "https://api.github.com/repos/{0}/{1}/commits/{2}/pulls"
}

Process {
  Write-Host "Getting all relatd PRs for SHA '$SHA'"
  $prs = Invoke-RestMethod -Method Get -Uri ($getLabelsUrl -f $Owner, $Repository, $SHA)

  Write-Host "Found '$($prs.Length)' PRs"

  Write-Output ($prs | ForEach-Object { $_.number }) -NoEnumerate
}