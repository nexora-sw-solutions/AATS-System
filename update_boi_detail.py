import os
filepath = r'd:\Nexora\AATS\Windows\AATS-4.0\AATS Frontend\AATS.Desktop\Views\SecretarialAdvisory\BOIDetailView.axaml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()
start_marker = '<!-- 4. Detailed Info Grid -->'
end_marker = '<!-- 5. Status Overview Cards -->'
start_idx = content.find(start_marker)
end_idx = content.find(end_marker)
if start_idx != -1 and end_idx != -1:
    new_xml = '''<!-- 4. Detailed Info Grid -->
                <StackPanel Grid.Row=\"3\" Margin=\"0,0,0,32\">
                    <!-- 1. Client Information Card -->
                    <Border Classes=\"detail-card\">
                        <StackPanel Spacing=\"20\">
                            <StackPanel Orientation=\"Horizontal\" Spacing=\"12\">
                                <i:Icon Value=\"fa-solid fa-address-card\" Foreground=\"{DynamicResource AccentPrimary}\" FontSize=\"18\"/>
                                <TextBlock Text=\"GENERAL INFORMATION\" FontSize=\"14\" FontWeight=\"Bold\" Foreground=\"{DynamicResource TextPrimary}\" VerticalAlignment=\"Center\"/>
                            </StackPanel>

                            <Grid RowDefinitions=\"Auto, 32, Auto, 32, Auto, 32, Auto\" ColumnDefinitions=\"*, *\">
                                <!-- Row 1 -->
                                <StackPanel Grid.Row=\"0\" Grid.Column=\"0\">
                                    <TextBlock Text=\"CLIENT NAME\" Classes=\"card-label\"/>
                                    <StackPanel Orientation=\"Horizontal\" Spacing=\"8\">
                                        <TextBlock Text=\"{Binding Record.ClientName}\" Classes=\"card-value\"/>
                                        <Border Background=\"{Binding ClientCategoryColor}\" CornerRadius=\"12\" Padding=\"8,2\" VerticalAlignment=\"Center\" IsVisible=\"{Binding HasClientCategory}\">
                                            <TextBlock Text=\"{Binding ClientCategory}\" Foreground=\"Black\" FontSize=\"10\" FontWeight=\"Bold\"/>
                                        </Border>
                                    </StackPanel>
                                </StackPanel>
                                <StackPanel Grid.Row=\"0\" Grid.Column=\"1\">
                                    <TextBlock Text=\"COMPANY NAME\" Classes=\"card-label\"/>
                                    <TextBlock Text=\"{Binding CompanyName}\" Classes=\"card-value\"/>
                                </StackPanel>

                                <!-- Row 2 -->
                                <StackPanel Grid.Row=\"2\" Grid.Column=\"0\">
                                    <TextBlock Text=\"CODE\" Classes=\"card-label\"/>
                                    <TextBlock Text=\"{Binding Record.Code}\" Classes=\"card-value\"/>
                                </StackPanel>
                                <StackPanel Grid.Row=\"2\" Grid.Column=\"1\">
                                    <TextBlock Text=\"INVESTMENT VALUE (USD)\" Classes=\"card-label\"/>
                                    <TextBlock Text=\"{Binding Record.InvestmentValue}\" Classes=\"card-value\"/>
                                </StackPanel>

                                <!-- Row 3 -->
                                <StackPanel Grid.Row=\"4\" Grid.Column=\"0\">
                                    <TextBlock Text=\"COUNTRY\" Classes=\"card-label\"/>
                                    <TextBlock Text=\"{Binding Record.Country}\" Classes=\"card-value\"/>
                                </StackPanel>
                                <StackPanel Grid.Row=\"4\" Grid.Column=\"1\">
                                    <TextBlock Text=\"ASSIGNMENT\" Classes=\"card-label\"/>
                                    <TextBlock Text=\"{Binding AssignmentDisplay}\" Classes=\"card-value\"/>
                                </StackPanel>

                                <!-- Row 4 -->
                                <StackPanel Grid.Row=\"6\" Grid.ColumnSpan=\"2\">
                                    <TextBlock Text=\"COUNTRY ADDRESS\" Classes=\"card-label\"/>
                                    <TextBlock Text=\"{Binding Record.CountryAddress}\" Classes=\"card-value\"/>
                                </StackPanel>
                            </Grid>
                        </StackPanel>
                    </Border>
                </StackPanel>

                '''
    content = content[:start_idx] + new_xml + content[end_idx:]
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    print('Updated BOIDetailView.axaml successfully.')
else:
    print('Could not find markers.')
