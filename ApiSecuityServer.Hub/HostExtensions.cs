using System.Reflection.Metadata;
using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;

namespace ApiSecuityServer;

internal static class HostExtensions
{
    public static void AddJwtAuthentication(this IServiceCollection services)
    {
        // services.AddIdentity<IdentityUserEntity, IdentityRole>()
        //     .AddEntityFrameworkStores<DbContextFactory>()
        //     .AddJwtTokenProvider();
        //
        // services.AddAuthentication(options =>
        //     {
        //         options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //         options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //         options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        //     })
        //     .AddJwtBearer();
    }

    public static void AddMediatR(this IServiceCollection services)
    {
        services.AddMediatR(c => c.RegisterServicesFromAssembly(AssemblyReference.Assembly));
    }
    
    public static void AddValidators(this IServiceCollection services)
    {
        //services.AddScoped(typeof(IPipelineBehavior<,>), typeof(HubRequestValidationPipelineBehavior<,>));
        services.AddValidatorsFromAssembly(AssemblyReference.Assembly, includeInternalTypes: true);
    }

    public static void AddSwaggerGen(this IServiceCollection services)
    {
        // services.AddSwaggerGen(c =>
        // {
        //     c.SwaggerDoc("v1", new OpenApiInfo { Title = "OrderServer.Api", Version = "v1" });
        //
        //     // Add security definitions
        //     c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        //     {
        //         Description = "Please enter into field the word 'Bearer' followed by a space and the JWT value",
        //         Name = "Authorization",
        //         In = ParameterLocation.Header,
        //         Type = SecuritySchemeType.ApiKey,
        //     });
        //
        //     c.AddSecurityRequirement(new OpenApiSecurityRequirement
        //     {
        //         {
        //             new OpenApiSecurityScheme
        //             {
        //                 Reference = new OpenApiReference()
        //                 {
        //                     Id = "Bearer",
        //                     Type = ReferenceType.SecurityScheme
        //                 }
        //             },
        //             Array.Empty<string>()
        //         }
        //     });
        //
        //     // include document file
        //     c.IncludeXmlComments(
        //         Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml"),
        //         true);
        // });
    }

    public static void AddAndSetOptionsControllers(this IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
            options.JsonSerializerOptions.DefaultIgnoreCondition =
                System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
    }
}