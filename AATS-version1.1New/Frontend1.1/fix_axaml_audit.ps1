$oldSnippet = @'
                                            <TextBox Classes="form-input" Watermark="Enter Client ID (e.g. CL-00001)" Text="{Binding ClientId}" Name="ClientIdTextBox"/>
                                            <!-- Client Code Suggestions Dropdown (absolute overlay) -->
                                            <Popup IsOpen="{Binding IsClientCodeDropdownOpen}" 
                                                   PlacementTarget="{Binding #ClientIdTextBox}"
                                                   Placement="Bottom"
                                                   PlacementGravity="Bottom"
                                                   PlacementAnchor="Bottom"
                                                   HorizontalOffset="0"
                                                   VerticalOffset="2"
                                                   IsLightDismissEnabled="True"
                                                   MaxHeight="220"
                                                   Width="{Binding #ClientIdTextBox.Bounds.Width}">
                                                <Border Background="{DynamicResource CardBg}" 
                                                        BorderBrush="{DynamicResource BorderDefault}" 
                                                        BorderThickness="1" 
                                                        CornerRadius="0,0,6,6" 
                                                        BoxShadow="0 4 16 0 #40000000"
                                                        ClipToBounds="True">
                                                    <ScrollViewer MaxHeight="200" VerticalScrollBarVisibility="Auto">
                                                        <ItemsControl ItemsSource="{Binding ClientCodeSuggestions}">
                                                            <ItemsControl.ItemTemplate>
                                                                <DataTemplate>
                                                                    <Button Background="Transparent" 
                                                                            BorderThickness="0" 
                                                                            Padding="12,8" 
                                                                            HorizontalAlignment="Stretch" 
                                                                            HorizontalContentAlignment="Left"
                                                                            Cursor="Hand"
                                                                            Command="{Binding $parent[UserControl].DataContext.SelectClientCodeCommand}" 
                                                                            CommandParameter="{Binding}">
                                                                        <TextBlock Text="{Binding}" 
                                                                                   Foreground="{DynamicResource TextInput}" 
                                                                                   FontSize="13"/>
                                                                    </Button>
                                                                </DataTemplate>
                                                            </ItemsControl.ItemTemplate>
                                                        </ItemsControl>
                                                    </ScrollViewer>
                                                </Border>
                                            </Popup>
'@

$newSnippet = @'
                                            <TextBox Classes="form-input" Watermark="Enter Client ID (e.g. CL-00001)" Text="{Binding ClientId}" Name="ClientIdTextBox"/>
                                            <Popup IsOpen="{Binding IsClientCodeDropdownOpen}" 
                                                   PlacementTarget="{Binding #ClientIdTextBox}"
                                                   Placement="Bottom"
                                                   PlacementGravity="Bottom"
                                                   PlacementAnchor="Bottom"
                                                   HorizontalOffset="0"
                                                   VerticalOffset="2"
                                                   IsLightDismissEnabled="True"
                                                   MaxHeight="220"
                                                   Width="{Binding #ClientIdTextBox.Bounds.Width}">
                                                <Border Background="{DynamicResource CardBg}" 
                                                        BorderBrush="{DynamicResource BorderDefault}" 
                                                        BorderThickness="1" 
                                                        CornerRadius="0,0,6,6" 
                                                        BoxShadow="0 4 16 0 #40000000"
                                                        ClipToBounds="True">
                                                    <ScrollViewer MaxHeight="200" VerticalScrollBarVisibility="Auto">
                                                        <ItemsControl ItemsSource="{Binding ClientCodeSuggestions}">
                                                            <ItemsControl.ItemTemplate>
                                                                <DataTemplate>
                                                                    <Button Background="Transparent" 
                                                                            BorderThickness="0" 
                                                                            Padding="12,8" 
                                                                            HorizontalAlignment="Stretch" 
                                                                            HorizontalContentAlignment="Left"
                                                                            Cursor="Hand"
                                                                            Command="{Binding $parent[UserControl].DataContext.SelectClientCodeCommand}" 
                                                                            CommandParameter="{Binding}">
                                                                        <StackPanel Orientation="Horizontal" Spacing="10">
                                                                            <TextBlock Text="{Binding ClientCode}" FontWeight="Bold" Foreground="{DynamicResource TextInput}" FontSize="13"/>
                                                                            <TextBlock Text="-" Foreground="{DynamicResource TextSecondary}" FontSize="13"/>
                                                                            <TextBlock Text="{Binding Name}" Foreground="{DynamicResource TextSecondary}" FontSize="12" VerticalAlignment="Center"/>
                                                                        </StackPanel>
                                                                    </Button>
                                                                </DataTemplate>
                                                            </ItemsControl.ItemTemplate>
                                                        </ItemsControl>
                                                    </ScrollViewer>
                                                </Border>
                                            </Popup>
'@

$directories = @(
    "c:\Users\sagar\Desktop\AATS\Frontend\AATS.Desktop\Views\SecretarialAdvisory\",
    "c:\Users\sagar\Desktop\AATS\Frontend\AATS.Desktop\Views\AuditAndAccounts\"
)

foreach ($dir in $directories) {
    $files = Get-ChildItem -Path $dir -Filter "*View.axaml"
    foreach ($f in $files) {
        $content = Get-Content $f.FullName -Raw
        
        if ($content.Contains($oldSnippet)) {
            $content = $content.Replace($oldSnippet, $newSnippet)
            Set-Content $f.FullName $content -NoNewline
            Write-Host "Updated $($f.Name)"
        }
    }
}
