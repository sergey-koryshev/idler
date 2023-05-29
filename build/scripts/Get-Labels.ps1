[CmdletBinding()]
param(
    [int]
    $PullRequestId,
    
    [string]
    $Owner,
    
    [string]
    $Repository
)

begin {
  $labels = @(
    "bug"
    "enhancement"
    "misc"
    "breaking changes"
    "minor enhancement"
  )
  $getLabelsUrl = "https://api.github.com/repos/{0}/{1}/issues/{2}/labels"
}

Process {
  Write-Host "Getting labels for PR '$PullRequestId'"
  $prLabels = Invoke-RestMethod -Method Get -Uri ($getLabelsUrl -f $Owner, $Repository, $PullRequestId)

  if ($null -eq $prLabels -or $prLabels.Length -eq 0) {
    throw "The PR doesn't contains any label"
  }

  Write-Host "The PR contains follow labels: $(($prLabels | ForEach-Object { $_.name }) -join ", ")"

  $foundLabels = 0;

  foreach($prLabel in $prLabels) {
    if ($labels -contains $prLabel.name) {
      $foundLabels++
    }
  }

  if ($foundLabels -gt 1) {
    throw "The PR must contains one of these labels: $($labels -join ", ")"
  }

  Write-Output ($prLabels | ForEach-Object { $_.name }) -NoEnumerate
}