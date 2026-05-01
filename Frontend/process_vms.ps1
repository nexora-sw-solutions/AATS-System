$files = @(
    "AuditAndAccounts\TaxAccountAddRecordViewModel.cs",
    "AuditAndAccounts\ManagementAccountAddRecordViewModel.cs",
    "AuditAndAccounts\InternalControlAddRecordViewModel.cs",
    "AuditAndAccounts\InternalAuditAddRecordViewModel.cs",
    "AuditAndAccounts\ForensicAuditAddRecordViewModel.cs",
    "AuditAndAccounts\AuditAssuranceAddRecordViewModel.cs",
    "AuditAndAccounts\AuditOthersAddRecordViewModel.cs",
    "SecretarialAdvisory\AddTradeMarkViewModel.cs",
    "SecretarialAdvisory\AddTradeLicenseViewModel.cs",
    "SecretarialAdvisory\AddImportExportViewModel.cs",
    "SecretarialAdvisory\AddHRConsultingViewModel.cs",
    "SecretarialAdvisory\AddEPFETFViewModel.cs",
    "SecretarialAdvisory\AddCompanyRegistrationViewModel.cs",
    "SecretarialAdvisory\AddBusinessPlanViewModel.cs",
    "SecretarialAdvisory\AddBOIViewModel.cs",
    "SecretarialAdvisory\AddSecretarialOthersViewModel.cs",
    "SecretarialAdvisory\AddEPFETFStaffViewModel.cs"
)

$basePath = "c:\Users\sagar\Desktop\AATS\Frontend\AATS.Desktop\ViewModels\"

foreach ($f in $files) {
    $path = Join-Path $basePath $f
    if (-Not (Test-Path $path)) { continue }
    
    $content = Get-Content $path -Raw
    
    # 1. Rename _id to _clientId if it exists
    if ($content -match "private string _id = string.Empty;") {
        $content = $content -replace "private string _id = string.Empty;", "private string _clientId = string.Empty;"
        $content = $content -replace "\.ID = Id", ".ID = ClientId"
        $content = $content -replace "ID = Id", "ID = ClientId"
        $content = $content -replace "Id =", "ClientId ="
        $content = $content -replace " Id,", " ClientId,"
    }
    
    # 2. Add LoadClientCodesAsync() to constructors if not already there
    # Find constructors
    $className = [System.IO.Path]::GetFileNameWithoutExtension($f)
    $constructorPattern = "public $className\(\)\s*\{"
    if ($content -match $constructorPattern) {
        if ($content -notmatch "LoadClientCodesAsync\(\)") {
             $content = $content -replace $constructorPattern, "public $className() { _ = LoadClientCodesAsync();"
        }
    }
    
    $recordConstructorPattern = "public $className\(AuditRecord record\)\s*\{"
    if ($content -match $recordConstructorPattern) {
        if ($content -notmatch "LoadClientCodesAsync\(\)") {
             $content = $content -replace $recordConstructorPattern, "public $className(AuditRecord record) { _ = LoadClientCodesAsync();"
        }
    }

    # 3. Add partial void OnClientIdChanged and override SelectClientCode at the end of the class
    # But first remove any existing ones to avoid duplicates
    $content = $content -replace "partial void OnClientIdChanged\(string value\)\s*\{[\s\S]*?\}", ""
    $content = $content -replace "public override void SelectClientCode\(string code\)\s*\{[\s\S]*?\}", ""
    $content = $content -replace "protected override void SelectClientCode\(string code\)\s*\{[\s\S]*?\}", ""
    
    $newMethods = @"
    partial void OnClientIdChanged(string value)
    {
        FilterClientCodes(value);
    }

    public override void SelectClientCode(string code)
    {
        ClientId = code;
        ClientName = GetClientName(code);
        IsClientCodeDropdownOpen = false;
    }
}
"@
    # Replace the last } with the new methods and the closing brace
    $content = $content.TrimEnd()
    if ($content.EndsWith("}")) {
        $content = $content.Substring(0, $content.Length - 1) + $newMethods
    }

    Set-Content $path $content -NoNewline
    Write-Host "Processed $f"
}
