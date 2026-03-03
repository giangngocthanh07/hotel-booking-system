using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;

public static class SwaggerServiceExtension
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "HotelBooking API", Version = "v1" });

            // Cấu hình JWT Bearer cho Swagger
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Nhập token: Bearer {token}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    new string[] {}
                }
            });

            // Hỗ trợ Kế thừa & Đa hình
            options.UseAllOfForInheritance();
            options.UseOneOfForPolymorphism();

            // Filter cho Enum
            options.SchemaFilter<EnumSchemaFilter>();
        });

        return services;
    }
}