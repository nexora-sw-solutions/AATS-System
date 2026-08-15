import os, re

files = [
    r'd:\Nexora\AATS\Windows\Version 2\Version2.5\AATS-version2.5\AATS-Version2.3\AATS.Desktop\Views\Shared\SharedTaxTableView.axaml',
    r'd:\Nexora\AATS\Windows\Version 2\Version2.5\AATS-version2.5\AATS-Version2.3\AATS.Desktop\Views\Shared\SharedSecretarialTableView.axaml',
    r'd:\Nexora\AATS\Windows\Version 2\Version2.5\AATS-version2.5\AATS-Version2.3\AATS.Desktop\Views\Shared\SharedAuditTableView.axaml',
    r'd:\Nexora\AATS\Windows\Version 2\Version2.5\AATS-version2.5\AATS-Version2.3\AATS.Desktop\Views\Clients\ClientsView.axaml',
    r'd:\Nexora\AATS\Windows\Version 2\Version2.5\AATS-version2.5\AATS-Version2.3\AATS.Desktop\Views\AuditAndAccounts\ForensicAuditDetailView.axaml',
    r'd:\Nexora\AATS\Windows\Version 2\Version2.5\AATS-version2.5\AATS-Version2.3\AATS.Desktop\Views\AuditAndAccounts\InternalControlDetailView.axaml',
    r'd:\Nexora\AATS\Windows\Version 2\Version2.5\AATS-version2.5\AATS-Version2.3\AATS.Desktop\Views\AuditAndAccounts\TaxAccountDetailView.axaml',
    r'd:\Nexora\AATS\Windows\Version 2\Version2.5\AATS-version2.5\AATS-Version2.3\AATS.Desktop\Views\AuditAndAccounts\ManagementAccountDetailView.axaml',
    r'd:\Nexora\AATS\Windows\Version 2\Version2.5\AATS-version2.5\AATS-Version2.3\AATS.Desktop\Views\AuditAndAccounts\InternalAuditDetailView.axaml',
    r'd:\Nexora\AATS\Windows\Version 2\Version2.5\AATS-version2.5\AATS-Version2.3\AATS.Desktop\Views\SecretarialAdvisory\BOIDetailView.axaml',
    r'd:\Nexora\AATS\Windows\Version 2\Version2.5\AATS-version2.5\AATS-Version2.3\AATS.Desktop\Views\SecretarialAdvisory\BusinessPlanDetailView.axaml',
    r'd:\Nexora\AATS\Windows\Version 2\Version2.5\AATS-version2.5\AATS-Version2.3\AATS.Desktop\Views\SecretarialAdvisory\CompanyRegistrationDetailView.axaml',
    r'd:\Nexora\AATS\Windows\Version 2\Version2.5\AATS-version2.5\AATS-Version2.3\AATS.Desktop\Views\SecretarialAdvisory\EPFETFDetailView.axaml',
    r'd:\Nexora\AATS\Windows\Version 2\Version2.5\AATS-version2.5\AATS-Version2.3\AATS.Desktop\Views\SecretarialAdvisory\HRConsultingDetailView.axaml',
    r'd:\Nexora\AATS\Windows\Version 2\Version2.5\AATS-version2.5\AATS-Version2.3\AATS.Desktop\Views\SecretarialAdvisory\ImportExportDetailView.axaml',
    r'd:\Nexora\AATS\Windows\Version 2\Version2.5\AATS-version2.5\AATS-Version2.3\AATS.Desktop\Views\SecretarialAdvisory\StaffDetailView.axaml',
    r'd:\Nexora\AATS\Windows\Version 2\Version2.5\AATS-version2.5\AATS-Version2.3\AATS.Desktop\Views\SecretarialAdvisory\TradeLicenseDetailView.axaml',
    r'd:\Nexora\AATS\Windows\Version 2\Version2.5\AATS-version2.5\AATS-Version2.3\AATS.Desktop\Views\SecretarialAdvisory\TradeMarkDetailView.axaml',
    r'd:\Nexora\AATS\Windows\Version 2\Version2.5\AATS-version2.5\AATS-Version2.3\AATS.Desktop\Views\AuditAndAccounts\AuditAssuranceDetailView.axaml'
]

for file in files:
    if not os.path.exists(file):
        print(f"Skipping {file}, not found")
        continue

    with open(file, 'r', encoding='utf-8') as f:
        content = f.read()

    # We replace Margin="0,0,0,40" with Margin="0,0,0,0" globally in these files.
    # We saw it was used mostly on StackPanels or Grids inside the ScrollViewer.
    content = content.replace('Margin="0,0,0,40"', 'Margin="0,0,0,0"')

    # The detail views in AuditAndAccounts/SecretarialAdvisory have:
    # <Grid RowDefinitions="..., Auto, Auto, Auto" Margin="40,20,40,40">
    # We should replace that Margin to not clip as well.
    content = content.replace('Margin="40,20,40,40"', 'Margin="40,20,40,0"')

    # Inject the spacer before </StackPanel> directly preceding </ScrollViewer>
    # or before </Grid> directly preceding </ScrollViewer>
    content = re.sub(
        r'(</StackPanel>)\s*(?=</ScrollViewer>)',
        r'<!-- Bottom spacer to ensure full scrollability -->\n                                <Border Height="40" Background="Transparent"/>\n                            \1',
        content
    )
    content = re.sub(
        r'(</Grid>)\s*(?=</ScrollViewer>)',
        r'<!-- Bottom spacer to ensure full scrollability -->\n                                <Border Height="40" Background="Transparent"/>\n                            \1',
        content
    )

    with open(file, 'w', encoding='utf-8') as f:
        f.write(content)
    print(f"Updated {os.path.basename(file)}")
