using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AATS.Domain.Entities;

namespace AATS.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context)
        {
            try
            {
                // Seed branches if not exist
                var branchNames = new[] { "Central", "South", "West", "Northeast" };
                var codes = new[] { "CEN-001", "SOU-002", "WES-003", "NOR-004" };

                for (int i = 0; i < branchNames.Length; i++)
                {
                    var name = branchNames[i];
                    if (!await context.Branches.AnyAsync(b => b.Name == name))
                    {
                        context.Branches.Add(new Branch
                        {
                            Name = name,
                            Code = codes[i]
                        });
                    }
                }
                await context.SaveChangesAsync();

                // Seed / Reset admin user password hash to Admin@123
                var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@aats.com" || u.Username == "admin");
                if (admin == null)
                {
                    var branch = await context.Branches.FirstOrDefaultAsync(b => b.Name == "Central") ?? await context.Branches.FirstOrDefaultAsync();
                    if (branch != null)
                    {
                        admin = new User
                        {
                            Username = "admin",
                            Email = "admin@aats.com",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                            Role = UserRole.Admin,
                            BranchId = branch.Id
                        };
                        context.Users.Add(admin);
                    }
                }
                else
                {
                    // Always guarantee admin password is set to Admin@123
                    admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
                }
                await context.SaveChangesAsync();
                Console.WriteLine("[INFO] Admin user successfully initialized with username: admin / password: Admin@123");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] DbInitializer non-fatal notification: {ex.Message}");
            }
        }
    }
}
