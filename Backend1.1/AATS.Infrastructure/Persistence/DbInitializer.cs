using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AATS.Domain.Entities;
using AATS.Infrastructure.Persistence;

namespace AATS.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // One-time schema sync should now be handled via SQL script in Supabase
            // This reduces startup latency significantly.

            if (!await context.Branches.AnyAsync())
            {
                var mainBranch = new Branch
                {
                    Name = "Colombo Head Office",
                    Code = "COL-001",
                    Address = "123 Galle Road, Colombo",
                    Phone = "0112345678"
                };

                context.Branches.Add(mainBranch);
                await context.SaveChangesAsync();
            }

            // Seed an admin user if it doesn't exist specifically
            if (!await context.Users.AnyAsync(u => u.Email == "admin@aats.com"))
            {
                var branch = await context.Branches.FirstOrDefaultAsync();
                if (branch != null)
                {
                    var admin = new User
                    {
                        Username = "admin",
                        Email = "admin@aats.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                        Role = UserRole.Admin,
                        BranchId = branch.Id
                    };
                    context.Users.Add(admin);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
