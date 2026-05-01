$dir = "c:\Users\sagar\Desktop\AATS\Frontend\AATS.Desktop\ViewModels\SecretarialAdvisory\"
$files = Get-ChildItem -Path $dir -Filter "*ViewModel.cs"

foreach ($f in $files) {
    if ($f.Name -like "*DetailViewModel.cs" -or $f.Name -like "Add*") {
        continue
    }
    
    $content = Get-Content $f.FullName -Raw
    
    # Very specific match to remove the corrupted methods and fix the braces
    # This matches the methods and the trailing braces they might have added
    $pattern = '(?s)\s*partial void OnIdChanged.*?public override void SelectClientRecord.*?}\s*}'
    if ($content -match $pattern) {
        $newContent = [regex]::Replace($content, $pattern, "`r`n}")
        Set-Content $f.FullName $newContent -NoNewline
        Write-Host "Fixed $($f.Name)"
    }
}
