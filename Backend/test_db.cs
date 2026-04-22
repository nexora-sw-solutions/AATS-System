using AuditApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("AuditApp.API/appsettings.Development.json")
    .Build();

var services = new ServiceCollection();
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("SupabaseConnection")));

using var serviceProvider = services.BuildServiceProvider();
using var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

try 
{
    Console.WriteLine("Connecting to Supabase...");
    var user = await context.Users.FirstOrDefaultAsync();
    if (user != null)
    {
        Console.WriteLine($"SUCCESS: Connected to database. Found user: {user.Username} (Role: {user.Role})");
    }
    else
    {
        Console.WriteLine("SUCCESS: Connected to database, but no users found.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: {ex.Message}");
    if (ex.InnerException != null) Console.WriteLine($"Inner: {ex.InnerException.Message}");
}
