import fitz

doc = fitz.open('D:\\Nexora\\AATS\\Windows\\AATS\\Updated Prompt.pdf')
for i, page in enumerate(doc):
    pix = page.get_pixmap(dpi=150)
    pix.save(f'D:\\Nexora\\AATS\\Windows\\AATS\\Updated_Prompt_Page_{i}.png')
print(f"Extracted {len(doc)} pages as images.")
