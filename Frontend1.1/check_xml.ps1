
$file = "d:\Nexora\AATS\Windows\AATS\AATS.Desktop\Views\Shared\SharedSecretarialTableView.axaml"
$content = Get-Content $file -Raw

$openTags = [regex]::Matches($content, "<([a-zA-Z0-9:]+)") | ForEach-Object { $_.Groups[1].Value }
$closeTags = [regex]::Matches($content, "</([a-zA-Z0-9:]+)>") | ForEach-Object { $_.Groups[1].Value }

Write-Host "Open Tags Count: $($openTags.Count)"
Write-Host "Close Tags Count: $($closeTags.Count)"

$stats = @{}
foreach ($tag in $openTags) {
    if (-not $stats.ContainsKey($tag)) { $stats[$tag] = @{Open=0; Close=0} }
    $stats[$tag].Open += 1
}
foreach ($tag in $closeTags) {
    if (-not $stats.ContainsKey($tag)) { $stats[$tag] = @{Open=0; Close=0} }
    $stats[$tag].Close += 1
}

$stats.GetEnumerator() | Sort-Object Name | ForEach-Object {
    if ($_.Value.Open -ne $_.Value.Close) {
        Write-Host "MISMATCH: $($_.Name) (Open: $($_.Value.Open), Close: $($_.Value.Close))" -ForegroundColor Red
    } else {
        # Write-Host "$($_.Name) OK ($($_.Value.Open))"
    }
}
