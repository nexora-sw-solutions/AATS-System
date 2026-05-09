const fs = require('fs');
const pdf = require('pdf-parse');

let dataBuffer1 = fs.readFileSync('d:\\\\Nexora\\\\AATS\\\\Windows\\\\AATS\\\\Updated Prompt.pdf');
pdf(dataBuffer1).then(function(data) {
    fs.writeFileSync('d:\\\\Nexora\\\\AATS\\\\Windows\\\\AATS\\\\Updated_Prompt.txt', data.text);
    let dataBuffer2 = fs.readFileSync('d:\\\\Nexora\\\\AATS\\\\Windows\\\\AATS\\\\Tech Stack Proposed.pdf');
    return pdf(dataBuffer2);
}).then(function(data) {
    fs.writeFileSync('d:\\\\Nexora\\\\AATS\\\\Windows\\\\AATS\\\\Tech_Stack_Proposed.txt', data.text);
    console.log("PDF extraction successful.");
}).catch(function(err) {
    console.error(err);
});
