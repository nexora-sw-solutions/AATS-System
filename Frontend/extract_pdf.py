import sys

try:
    import pypdf
    reader = pypdf.PdfReader('d:\\Nexora\\AATS\\Windows\\AATS\\Updated Prompt.pdf')
    with open('d:\\Nexora\\AATS\\Windows\\AATS\\Updated_Prompt.txt', 'w', encoding='utf-8') as f:
        for page in reader.pages:
            f.write(page.extract_text() + '\n')
            
    reader2 = pypdf.PdfReader('d:\\Nexora\\AATS\\Windows\\AATS\\Tech Stack Proposed.pdf')
    with open('d:\\Nexora\\AATS\\Windows\\AATS\\Tech_Stack_Proposed.txt', 'w', encoding='utf-8') as f:
        for page in reader2.pages:
            f.write(page.extract_text() + '\n')
    print("PyPDF success")
except Exception as e:
    print(f"PyPDF failed: {e}")
