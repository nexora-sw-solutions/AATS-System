$files = @(
    "ForensicAuditAddRecordViewModel.cs",
    "InternalAuditAddRecordViewModel.cs",
    "InternalControlAddRecordViewModel.cs",
    "ManagementAccountAddRecordViewModel.cs",
    "TaxAccountAddRecordViewModel.cs"
)

$basePath = "c:\Users\sagar\Desktop\AATS\Frontend\AATS.Desktop\ViewModels\AuditAndAccounts\"

foreach ($f in $files) {
    $path = Join-Path $basePath $f
    $content = Get-Content $path -Raw
    
    # Standardize _clientId
    if ($content -match "\[ObservableProperty\] private string _id = string\.Empty;") {
        $content = $content -replace "\[ObservableProperty\] private string _id = string\.Empty;", "[ObservableProperty] private string _clientId = string.Empty;"
    }
    
    # Ensure constructor calls LoadClientCodesAsync()
    $className = [System.IO.Path]::GetFileNameWithoutExtension($f)
    $constructorPattern = "public $className\(\)\s*\{"
    if ($content -match $constructorPattern -and $content -notmatch "LoadClientCodesAsync\(\)") {
        $content = $content -replace $constructorPattern, "public $className() { _ = LoadClientCodesAsync();"
    }
    
    $recordConstructorPattern = "public $className\(AuditRecord record\)\s*\{"
    if ($content -match $recordConstructorPattern -and $content -notmatch "LoadClientCodesAsync\(\)") {
        $content = $content -replace $recordConstructorPattern, "public $className(AuditRecord record) { _ = LoadClientCodesAsync();"
    }

    # Fix usages of Id to ClientId
    $content = $content -replace "Id = record\.ID", "ClientId = record.ID"
    $content = $content -replace "record\.ID = Id", "record.ID = ClientId"
    $content = $content -replace "ID = Id", "ID = ClientId"

    # Add/Update methods at the end
    $content = $content -replace "partial void OnIdChanged\(string value\)\s*\{[\s\S]*?\}", ""
    $content = $content -replace "partial void OnClientIdChanged\(string value\)\s*\{[\s\S]*?\}", ""
    $content = $content -replace "public override void SelectClientCode\(ClientRecord client\)\s*\{[\s\S]*?\}", ""
    $content = $content -replace "public override void SelectClientCode\(string code\)\s*\{[\s\S]*?\}", ""
    
    $newMethods = @"
        partial void OnClientIdChanged(string value)
        {
            FilterClientCodes(value);
        }

        public override void SelectClientCode(ClientRecord client)
        {
            ClientId = client.ClientCode ?? string.Empty;
            ClientName = client.Name ?? string.Empty;
            IsClientCodeDropdownOpen = false;
        }
    }
}
"@
    # Replace the last } } (closing class and namespace)
    $content = $content.TrimEnd()
    if ($content.EndsWith("}")) {
        # Namespace brace
        $content = $content.Substring(0, $content.LastIndexOf("}"))
        $content = $content.TrimEnd()
        if ($content.EndsWith("}")) {
            # Class brace
            $content = $content.Substring(0, $content.LastIndexOf("}"))
            $content = $content + $newMethods
        }
    }

    Set-Content $path $content -NoNewline
    Write-Host "Processed $f"
}
