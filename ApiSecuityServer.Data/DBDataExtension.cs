using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiSecuityServer.Data;

public static class DBDataExtension
{
    public static void AddSqliteEfCore(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContextPool<ApplicationDbContext>(optionsAction: options =>
        {
            options.UseSqlite(connectionString);
        }).AddScoped<DbContext, ApplicationDbContext>();
    }
}