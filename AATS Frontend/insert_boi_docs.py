import os

filepath = r'd:\Nexora\AATS\Windows\AATS-4.0\AATS Frontend\AATS.Desktop\Views\SecretarialAdvisory\BOIDetailView.axaml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

end_marker = '<!-- 5. Footer Actions -->'

part1_card = '''
                <StackPanel Margin=\"0,0,0,32\">
                    <Border Classes=\"detail-card\">
                        <StackPanel Spacing=\"20\">
                            <StackPanel Orientation=\"Horizontal\" Spacing=\"12\">
                                <i:Icon Value=\"fa-solid fa-folder-open\" Foreground=\"{DynamicResource AccentPrimary}\" FontSize=\"18\"/>
                                <TextBlock Text=\"REQUIRED DOCUMENTS - PART 1\" FontSize=\"14\" FontWeight=\"Bold\" Foreground=\"{DynamicResource TextPrimary}\" VerticalAlignment=\"Center\"/>
                            </StackPanel>

                            <Border BorderBrush=\"{DynamicResource BorderSubtle}\" BorderThickness=\"0,0,0,1\" Margin=\"0,0,0,10\">
                                <ScrollViewer HorizontalScrollBarVisibility=\"Auto\" VerticalScrollBarVisibility=\"Disabled\">
                                    <StackPanel Orientation=\"Horizontal\" Spacing=\"0\">
                                        <Button Classes=\"tab-btn\" Classes.selected=\"{Binding SelectedPart1Tab, Converter={StaticResource StringEqualityConverter}, ConverterParameter='Approval Letter'}\" Command=\"{Binding SelectPart1TabCommand}\" CommandParameter=\"Approval Letter\">
                                            <TextBlock Text=\"Approval Letter\"/>
                                        </Button>
                                        <Button Classes=\"tab-btn\" Classes.selected=\"{Binding SelectedPart1Tab, Converter={StaticResource StringEqualityConverter}, ConverterParameter='Passport'}\" Command=\"{Binding SelectPart1TabCommand}\" CommandParameter=\"Passport\">
                                            <TextBlock Text=\"Passport\"/>
                                        </Button>
                                        <Button Classes=\"tab-btn\" Classes.selected=\"{Binding SelectedPart1Tab, Converter={StaticResource StringEqualityConverter}, ConverterParameter='Investment'}\" Command=\"{Binding SelectPart1TabCommand}\" CommandParameter=\"Investment\">
                                            <TextBlock Text=\"Investment\"/>
                                        </Button>
                                        <Button Classes=\"tab-btn\" Classes.selected=\"{Binding SelectedPart1Tab, Converter={StaticResource StringEqualityConverter}, ConverterParameter='Residential Visa'}\" Command=\"{Binding SelectPart1TabCommand}\" CommandParameter=\"Residential Visa\">
                                            <TextBlock Text=\"Residential Visa\"/>
                                        </Button>
                                        <Button Classes=\"tab-btn\" Classes.selected=\"{Binding SelectedPart1Tab, Converter={StaticResource StringEqualityConverter}, ConverterParameter='IIA Account'}\" Command=\"{Binding SelectPart1TabCommand}\" CommandParameter=\"IIA Account\">
                                            <TextBlock Text=\"IIA Account\"/>
                                        </Button>
                                        <Button Classes=\"tab-btn\" Classes.selected=\"{Binding SelectedPart1Tab, Converter={StaticResource StringEqualityConverter}, ConverterParameter='Bank Letter'}\" Command=\"{Binding SelectPart1TabCommand}\" CommandParameter=\"Bank Letter\">
                                            <TextBlock Text=\"Bank Letter\"/>
                                        </Button>
                                        <Button Classes=\"tab-btn\" Classes.selected=\"{Binding SelectedPart1Tab, Converter={StaticResource StringEqualityConverter}, ConverterParameter='Company Registration'}\" Command=\"{Binding SelectPart1TabCommand}\" CommandParameter=\"Company Registration\">
                                            <TextBlock Text=\"Company Registration\"/>
                                        </Button>
                                    </StackPanel>
                                </ScrollViewer>
                            </Border>

                            <ScrollViewer MaxHeight=\"300\" VerticalScrollBarVisibility=\"Auto\">
                                <StackPanel>
                                    <TextBlock Text=\"No documents found.\" Foreground=\"{DynamicResource TextFaint}\" FontStyle=\"Italic\" HorizontalAlignment=\"Center\" Margin=\"0,20\" IsVisible=\"{Binding !FilteredPart1Documents.Count}\"/>
                                    <ItemsControl ItemsSource=\"{Binding FilteredPart1Documents}\">
                                        <ItemsControl.ItemTemplate>
                                            <DataTemplate x:DataType=\"models:SourceDocument\">
                                                <Border Classes=\"document-item\" Background=\"{DynamicResource DrawerHeaderBg}\" CornerRadius=\"10\" Padding=\"16,12\" Margin=\"0,0,0,8\" BorderBrush=\"{DynamicResource BorderDefault}\" BorderThickness=\"1\">
                                                    <Grid ColumnDefinitions=\"Auto, *, Auto\">
                                                        <i:Icon Value=\"fa-solid fa-file-pdf\" Foreground=\"{DynamicResource AccentPrimary}\" FontSize=\"20\" Margin=\"0,0,16,0\"/>
                                                        <StackPanel Grid.Column=\"1\" VerticalAlignment=\"Center\">
                                                            <TextBlock Text=\"{Binding FileName}\" Foreground=\"{DynamicResource TextPrimary}\" FontWeight=\"SemiBold\"/>
                                                            <TextBlock Text=\"{Binding Description}\" Foreground=\"{DynamicResource TextMuted}\" FontSize=\"11\" Margin=\"0,4,0,0\"/>
                                                        </StackPanel>
                                                        
                                                        <StackPanel Grid.Column=\"2\" Orientation=\"Horizontal\" Spacing=\"8\" VerticalAlignment=\"Center\">
                                                            <Button Classes=\"btn-detail-action\" Padding=\"8\" BorderThickness=\"0\" Height=\"32\"
                                                                    Command=\"{Binding [UserControl].DataContext.PreviewSourceDocumentCommand}\"
                                                                    CommandParameter=\"{Binding}\"
                                                                    ToolTip.Tip=\"Preview this document\">
                                                                <i:Icon Value=\"fa-solid fa-eye\" Foreground=\"{DynamicResource TextSecondary}\" FontSize=\"14\"/>
                                                            </Button>
                                                        </StackPanel>
                                                    </Grid>
                                                </Border>
                                            </DataTemplate>
                                        </ItemsControl.ItemTemplate>
                                    </ItemsControl>
                                </StackPanel>
                            </ScrollViewer>
                        </StackPanel>
                    </Border>
                </StackPanel>
'''

part2_card = '''
                <StackPanel Margin=\"0,0,0,32\">
                    <Border Classes=\"detail-card\">
                        <StackPanel Spacing=\"20\">
                            <StackPanel Orientation=\"Horizontal\" Spacing=\"12\">
                                <i:Icon Value=\"fa-solid fa-folder-open\" Foreground=\"{DynamicResource AccentPrimary}\" FontSize=\"18\"/>
                                <TextBlock Text=\"REQUIRED DOCUMENTS - PART 2\" FontSize=\"14\" FontWeight=\"Bold\" Foreground=\"{DynamicResource TextPrimary}\" VerticalAlignment=\"Center\"/>
                            </StackPanel>

                            <Border BorderBrush=\"{DynamicResource BorderSubtle}\" BorderThickness=\"0,0,0,1\" Margin=\"0,0,0,10\">
                                <ScrollViewer HorizontalScrollBarVisibility=\"Auto\" VerticalScrollBarVisibility=\"Disabled\">
                                    <StackPanel Orientation=\"Horizontal\" Spacing=\"0\">
                                        <Button Classes=\"tab-btn\" Classes.selected=\"{Binding SelectedPart2Tab, Converter={StaticResource StringEqualityConverter}, ConverterParameter='BOI Payment Slip'}\" Command=\"{Binding SelectPart2TabCommand}\" CommandParameter=\"BOI Payment Slip\">
                                            <TextBlock Text=\"BOI Payment Slip\"/>
                                        </Button>
                                        <Button Classes=\"tab-btn\" Classes.selected=\"{Binding SelectedPart2Tab, Converter={StaticResource StringEqualityConverter}, ConverterParameter='VAT Certificate'}\" Command=\"{Binding SelectPart2TabCommand}\" CommandParameter=\"VAT Certificate\">
                                            <TextBlock Text=\"VAT Certificate\"/>
                                        </Button>
                                        <Button Classes=\"tab-btn\" Classes.selected=\"{Binding SelectedPart2Tab, Converter={StaticResource StringEqualityConverter}, ConverterParameter='TDL Letter'}\" Command=\"{Binding SelectPart2TabCommand}\" CommandParameter=\"TDL Letter\">
                                            <TextBlock Text=\"TDL Letter\"/>
                                        </Button>
                                        <Button Classes=\"tab-btn\" Classes.selected=\"{Binding SelectedPart2Tab, Converter={StaticResource StringEqualityConverter}, ConverterParameter='Plan'}\" Command=\"{Binding SelectPart2TabCommand}\" CommandParameter=\"Plan\">
                                            <TextBlock Text=\"Plan\"/>
                                        </Button>
                                        <Button Classes=\"tab-btn\" Classes.selected=\"{Binding SelectedPart2Tab, Converter={StaticResource StringEqualityConverter}, ConverterParameter='Business Proposal'}\" Command=\"{Binding SelectPart2TabCommand}\" CommandParameter=\"Business Proposal\">
                                            <TextBlock Text=\"Business Proposal\"/>
                                        </Button>
                                        <Button Classes=\"tab-btn\" Classes.selected=\"{Binding SelectedPart2Tab, Converter={StaticResource StringEqualityConverter}, ConverterParameter='Cover Letter'}\" Command=\"{Binding SelectPart2TabCommand}\" CommandParameter=\"Cover Letter\">
                                            <TextBlock Text=\"Cover Letter\"/>
                                        </Button>
                                    </StackPanel>
                                </ScrollViewer>
                            </Border>

                            <ScrollViewer MaxHeight=\"300\" VerticalScrollBarVisibility=\"Auto\">
                                <StackPanel>
                                    <TextBlock Text=\"No documents found.\" Foreground=\"{DynamicResource TextFaint}\" FontStyle=\"Italic\" HorizontalAlignment=\"Center\" Margin=\"0,20\" IsVisible=\"{Binding !FilteredPart2Documents.Count}\"/>
                                    <ItemsControl ItemsSource=\"{Binding FilteredPart2Documents}\">
                                        <ItemsControl.ItemTemplate>
                                            <DataTemplate x:DataType=\"models:SourceDocument\">
                                                <Border Classes=\"document-item\" Background=\"{DynamicResource DrawerHeaderBg}\" CornerRadius=\"10\" Padding=\"16,12\" Margin=\"0,0,0,8\" BorderBrush=\"{DynamicResource BorderDefault}\" BorderThickness=\"1\">
                                                    <Grid ColumnDefinitions=\"Auto, *, Auto\">
                                                        <i:Icon Value=\"fa-solid fa-file-pdf\" Foreground=\"{DynamicResource AccentPrimary}\" FontSize=\"20\" Margin=\"0,0,16,0\"/>
                                                        <StackPanel Grid.Column=\"1\" VerticalAlignment=\"Center\">
                                                            <TextBlock Text=\"{Binding FileName}\" Foreground=\"{DynamicResource TextPrimary}\" FontWeight=\"SemiBold\"/>
                                                            <TextBlock Text=\"{Binding Description}\" Foreground=\"{DynamicResource TextMuted}\" FontSize=\"11\" Margin=\"0,4,0,0\"/>
                                                        </StackPanel>
                                                        
                                                        <StackPanel Grid.Column=\"2\" Orientation=\"Horizontal\" Spacing=\"8\" VerticalAlignment=\"Center\">
                                                            <Button Classes=\"btn-detail-action\" Padding=\"8\" BorderThickness=\"0\" Height=\"32\"
                                                                    Command=\"{Binding [UserControl].DataContext.PreviewSourceDocumentCommand}\"
                                                                    CommandParameter=\"{Binding}\"
                                                                    ToolTip.Tip=\"Preview this document\">
                                                                <i:Icon Value=\"fa-solid fa-eye\" Foreground=\"{DynamicResource TextSecondary}\" FontSize=\"14\"/>
                                                            </Button>
                                                        </StackPanel>
                                                    </Grid>
                                                </Border>
                                            </DataTemplate>
                                        </ItemsControl.ItemTemplate>
                                    </ItemsControl>
                                </StackPanel>
                            </ScrollViewer>
                        </StackPanel>
                    </Border>
                </StackPanel>
'''

if end_marker in content:
    content = content.replace(end_marker, part1_card + part2_card + end_marker)
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    print('Successfully inserted preview cards.')
else:
    print('Could not find end marker.')
