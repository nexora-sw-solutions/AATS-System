import os
filepath = r'd:\Nexora\AATS\Windows\AATS-4.0\AATS Frontend\AATS.Desktop\Views\SecretarialAdvisory\AddBOIView.axaml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()
start_marker = '<!-- Bank Letter Tab -->'
end_marker = '<!-- Company Registration Tab -->'
start = content.find(start_marker)
end = content.find(end_marker)
if start != -1 and end != -1:
    section = content[start:end]
    new_section = section.replace('PickBankLetterFilesCommand', 'OpenBankLetterPopupCommand')
    content = content[:start] + new_section + content[end:]
    popup_xml = '''
        <!-- Custom Bank Letter Upload Popup -->
        <Border ZIndex=\"103\" IsVisible=\"{Binding IsBankLetterPopupVisible}\" Classes=\"premium-modal-overlay\">
            <Border Classes=\"premium-modal-content\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Width=\"480\">
                <StackPanel Spacing=\"20\">
                    <TextBlock Text=\"BANK LETTER\" Foreground=\"{DynamicResource TextPrimary}\" FontSize=\"18\" FontWeight=\"Bold\" HorizontalAlignment=\"Center\"/>
                    <StackPanel Spacing=\"8\">
                        <TextBlock Text=\"Currency\" Classes=\"input-label\"/>
                        <ComboBox ItemsSource=\"{Binding Currencies}\" SelectedItem=\"{Binding SelectedCurrency}\" HorizontalAlignment=\"Stretch\" Classes=\"form-input\" PlaceholderText=\"Select Currency\"/>
                    </StackPanel>
                    <Button Classes=\"action-btn-premium\" HorizontalAlignment=\"Left\" Command=\"{Binding PickBankLetterFilesCommand}\">
                        <StackPanel Orientation=\"Horizontal\" Spacing=\"8\">
                            <i:Icon Value=\"fa-solid fa-plus\" Foreground=\"{DynamicResource AccentPrimary}\"/>
                            <TextBlock Text=\"Add Document\" FontWeight=\"SemiBold\" Foreground=\"{DynamicResource TextPrimary}\"/>
                        </StackPanel>
                    </Button>
                    <ScrollViewer MaxHeight=\"250\" VerticalScrollBarVisibility=\"Auto\">
                        <ItemsControl ItemsSource=\"{Binding BankLetterFiles}\">
                            <ItemsControl.ItemTemplate>
                                <DataTemplate>
                                    <Border Background=\"{DynamicResource DrawerHeaderBg}\" CornerRadius=\"6\" Padding=\"12\" Margin=\"0,0,0,8\">
                                        <Grid ColumnDefinitions=\"Auto,*,Auto\">
                                            <i:Icon Value=\"fa-solid fa-file\" Foreground=\"{DynamicResource AccentPrimary}\" FontSize=\"16\" VerticalAlignment=\"Center\"/>
                                            <TextBlock Grid.Column=\"1\" Text=\"{Binding FileName}\" Margin=\"12,0,0,0\" VerticalAlignment=\"Center\" FontSize=\"13\" Foreground=\"{DynamicResource TextInput}\"/>
                                            <StackPanel Grid.Column=\"2\" Orientation=\"Horizontal\" Spacing=\"8\">
                                                <Button Command=\"{Binding $parent[ItemsControl].DataContext.PreviewDocumentCommand}\" CommandParameter=\"{Binding Url}\" Classes=\"action-btn-premium\">
                                                    <i:Icon Value=\"fa-solid fa-eye\" Foreground=\"{DynamicResource AccentPrimary}\" FontSize=\"14\"/>
                                                </Button>
                                                <Button Command=\"{Binding $parent[ItemsControl].DataContext.ShowRemoveBankLetterFileConfirmCommand}\" CommandParameter=\"{Binding}\" Classes=\"action-btn-premium\">
                                                    <i:Icon Value=\"fa-solid fa-trash-can\" Foreground=\"{DynamicResource StatusDanger}\" FontSize=\"14\"/>
                                                </Button>
                                            </StackPanel>
                                        </Grid>
                                    </Border>
                                </DataTemplate>
                            </ItemsControl.ItemTemplate>
                        </ItemsControl>
                    </ScrollViewer>
                    <Grid ColumnDefinitions=\"*, 12, *\" Margin=\"0,10,0,0\">
                        <Button Grid.Column=\"0\" Content=\"Cancel\" Command=\"{Binding CancelBankLetterPopupCommand}\" Classes=\"btn-secondary\" HorizontalAlignment=\"Stretch\" HorizontalContentAlignment=\"Center\"/>
                        <Button Grid.Column=\"2\" Content=\"Save\" Command=\"{Binding SaveBankLetterPopupCommand}\" Classes=\"btn-save\" HorizontalAlignment=\"Stretch\" HorizontalContentAlignment=\"Center\"/>
                    </Grid>
                </StackPanel>
            </Border>
        </Border>
'''
    panel_end = content.rfind('</Panel>')
    content = content[:panel_end] + popup_xml + content[panel_end:]
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    print('Successfully updated AddBOIView.axaml')
else:
    print('Could not find markers')
