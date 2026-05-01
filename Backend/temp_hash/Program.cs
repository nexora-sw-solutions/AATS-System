using System;
using BCrypt.Net;

namespace TempHash
{
    class Program
    {
        static void Main(string[] args)
        {
            string password = "admin123";
            string hash = BCrypt.Net.BCrypt.HashPassword(password);
            Console.WriteLine(hash);
        }
    }
}
