using HotelBooking.application.Services.Domains.Auth;
using HotelBooking.application.Services.Domains.Media;
using HotelBooking.application.Services.Domains.AdminManagement;
using HotelBooking.application.Services.Domains.HotelManagement;
using HotelBooking.application.Services.Domains.RequestManagement;
using HotelBooking.application.Services.Domains.UserManagement;

public static class ApplicationServiceExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Core Services
        services.AddScoped<JwtAuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        
        // Hotel & Business Services
        services.AddScoped<IHotelService, HotelService>();
        services.AddScoped<IUpgradeRequestService, UpgradeRequestService>();
        services.AddScoped<IPhotoService, PhotoService>();

        // Admin Management
        services.AddScoped<IManagementAdminService, ManagementAdminService>();
        services.AddScoped<IAmenityService, AmenityService>();
        services.AddScoped<IPolicyService, PolicyService>();
        services.AddScoped<IServiceService, ServiceService>();
        
        // Helpers
        services.AddSingleton<IImageHelper, ImageHelper>();
        services.AddScoped<IFileHelper, FileHelper>();

        return services;
    }
}