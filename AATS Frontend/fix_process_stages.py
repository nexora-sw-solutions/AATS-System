import os
filepath = r'd:\Nexora\AATS\Windows\AATS-4.0\AATS Frontend\AATS.Desktop\Views\SecretarialAdvisory\BOIDetailView.axaml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

start_marker = '<StackPanel Grid.Row=\"2\" Spacing=\"24\" Margin=\"0,0,0,32\">'
end_marker = '<!-- 4. Detailed Info Grid -->'

start_idx = content.find('<!-- 3. Registration Process -->')
if start_idx != -1:
    end_idx = content.find(end_marker, start_idx)
    if end_idx != -1:
        old_block = content[start_idx:end_idx]
        new_block = old_block.replace('<StackPanel Grid.Row=\"2\" Spacing=\"24\" Margin=\"0,0,0,32\">', '<StackPanel Grid.Row=\"2\" Margin=\"0,0,0,32\">\n                    <Border Classes=\"detail-card\">\n                        <StackPanel Spacing=\"20\">')
        new_block = new_block.replace('<TextBlock Text=\"Registration Process\" FontSize=\"18\" FontWeight=\"Bold\" Foreground=\"{DynamicResource TextPrimary}\"/>', '<StackPanel Orientation=\"Horizontal\" Spacing=\"12\">\n                                <i:Icon Value=\"fa-solid fa-list-check\" Foreground=\"{DynamicResource AccentPrimary}\" FontSize=\"18\"/>\n                                <TextBlock Text=\"REGISTRATION PROCESS\" FontSize=\"14\" FontWeight=\"Bold\" Foreground=\"{DynamicResource TextPrimary}\" VerticalAlignment=\"Center\"/>\n                            </StackPanel>')
        new_block = new_block.replace('                    </Grid>\n                </StackPanel>\n', '                    </Grid>\n                        </StackPanel>\n                    </Border>\n                </StackPanel>\n')
        content = content[:start_idx] + new_block + content[end_idx:]
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        print('Updated Process Stages block.')
    else:
        print('End marker not found.')
else:
    print('Start marker not found.')
