import os
filepath = r'd:\Nexora\AATS\Windows\AATS-4.0\AATS Frontend\AATS.Desktop\Views\SecretarialAdvisory\AddBOIView.axaml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()
content = content.replace('FileCommand}', 'FilesCommand}')
style_xml = '''
        <Style Selector=\"Button.action-btn-premium\">
            <Setter Property=\"Background\" Value=\"Transparent\"/>
            <Setter Property=\"BorderBrush\" Value=\"{DynamicResource BorderDefault}\"/>
            <Setter Property=\"BorderThickness\" Value=\"1\"/>
            <Setter Property=\"Padding\" Value=\"12,8\"/>
            <Setter Property=\"CornerRadius\" Value=\"6\"/>
            <Setter Property=\"Cursor\" Value=\"Hand\"/>
            <Setter Property=\"Transitions\">
                <Transitions>
                    <BrushTransition Property=\"Background\" Duration=\"0:0:0.2\"/>
                </Transitions>
            </Setter>
        </Style>
        <Style Selector=\"Button.action-btn-premium:pointerover /template/ ContentPresenter#PART_ContentPresenter\">
            <Setter Property=\"Background\" Value=\"{DynamicResource SelectionIndicatorBg}\"/>
        </Style>
'''
if 'action-btn-premium' not in content:
    content = content.replace('</UserControl.Styles>', style_xml + '    </UserControl.Styles>')
with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print('Done!')
