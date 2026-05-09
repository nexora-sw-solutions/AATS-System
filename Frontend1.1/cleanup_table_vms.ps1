$dir = "c:\Users\sagar\Desktop\AATS\Frontend\AATS.Desktop\ViewModels\SecretarialAdvisory\"
$files = Get-ChildItem -Path $dir -Filter "*ViewModel.cs"

foreach ($f in $files) {
    if ($f.Name -like "*DetailViewModel.cs" -or $f.Name -like "Add*") {
        continue
    }
    
    $content = Get-Content $f.FullName -Raw
    # Remove the misplaced partial void OnIdChanged and SelectClientRecord blocks
    $newContent = $content -replace '(?s)\s*partial void OnIdChanged.*?}\s*public override void SelectClientRecord.*?}\s*(?=})', ''
    
    if ($content -ne $newContent) {
        Set-Content $f.FullName $newContent -NoNewline
        Write-Host "Cleaned up $($f.Name)"
    }
}
