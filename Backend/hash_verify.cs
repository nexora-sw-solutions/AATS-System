using BCrypt.Net;

var password = "Admin@123";
var hash = "$2a$12$R9h/lIPzHZclJLVKGaFqcO96vTq9yv7uMv8PmqfQ5D9N8jB9vLh.G";
bool match = BCrypt.Net.BCrypt.Verify(password, hash);
Console.WriteLine($"Match: {match}");
