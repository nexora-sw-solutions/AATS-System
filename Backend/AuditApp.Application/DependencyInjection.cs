using AuditApp.Application.Interfaces;
using AuditApp.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AuditApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<ITaxService, TaxService>();
        services.AddScoped<ISecretarialService, SecretarialService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<INexoraService, NexoraService>();
        services.AddScoped<IDocumentService, DocumentService>();

        return services;
    }
}
