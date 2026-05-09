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

            // Ensure missing columns exist in child tables
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE company_officers ADD COLUMN IF NOT EXISTS created_by UUID;");
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE epf_etf_staff ADD COLUMN IF NOT EXISTS created_by UUID;");
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE branches ADD COLUMN IF NOT EXISTS created_by UUID;");
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE users ADD COLUMN IF NOT EXISTS created_by UUID;");
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE clients ADD COLUMN IF NOT EXISTS created_by UUID;");
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE nexora_service_requests ADD COLUMN IF NOT EXISTS created_by UUID;");
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE payments ADD COLUMN IF NOT EXISTS created_by UUID;");
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE cheque_details ADD COLUMN IF NOT EXISTS created_by UUID;");
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE documents ADD COLUMN IF NOT EXISTS created_by UUID;");

            // Company Registration columns
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE company_registrations ADD COLUMN IF NOT EXISTS description TEXT;");
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE company_registrations ADD COLUMN IF NOT EXISTS director_names TEXT;");
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE company_registrations ADD COLUMN IF NOT EXISTS secretary_names TEXT;");
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE company_registrations ADD COLUMN IF NOT EXISTS shareholder_names TEXT;");
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE company_registrations ADD COLUMN IF NOT EXISTS other_names TEXT;");

            // Ensure current_step exists in all RecordBase tables
            var recordTables = new[] {
                "audit_assurance_records", "forensic_audit_records", "internal_audit_records",
                "internal_control_records", "management_account_records", "other_audit_records",
                "tax_account_records", "tax_filings", "company_registrations", "epf_etf_records",
                "trade_marks", "trade_licenses", "import_export_clearances", "boi_registrations",
                "business_plan_valuations", "hr_management_consulting", "other_secretarial_records"
            };

            foreach (var table in recordTables)
            {
                await context.Database.ExecuteSqlRawAsync($"ALTER TABLE {table} ADD COLUMN IF NOT EXISTS current_step INTEGER DEFAULT 0;");
            }

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
