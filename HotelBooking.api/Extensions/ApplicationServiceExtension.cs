using HotelBooking.application.Services.Domains.Auth;
using HotelBooking.application.Services.Domains.Media;
using HotelBooking.application.Services.Domains.AdminManagement;
using HotelBooking.application.Services.Domains.HotelManagement;
using HotelBooking.application.Services.Domains.RequestManagement;
using HotelBooking.application.Services.Domains.RequestManagement.Admin;
using HotelBooking.application.Services.Domains.RequestManagement.Customer;
using HotelBooking.application.Services.Domains.UserManagement;
using HotelBooking.application.Services.Domains.RoomManagement;
using HotelBooking.application.Services.Domains.UserManagement.Register;
using HotelBooking.application.Services.Domains.UserManagement.Login;
using HotelBooking.application.Services.Domains.RequestManagement.Owner;
using HotelBooking.application.Services.Domains.Common;

public static class ApplicationServiceExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Core Services
        services.AddScoped<JwtAuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IJwtAuthService, JwtAuthService>();

        // Hotel & Business Services
        services.AddScoped<IHotelService, HotelService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IRoomTypeService, RoomTypeService>();


        // Admin Management
        services.AddScoped<IManagementAdminService, ManagementAdminService>();
        services.AddScoped<IAmenityService, AmenityService>();
        services.AddScoped<IPolicyService, PolicyService>();
        services.AddScoped<IServiceService, ServiceService>();
        services.AddScoped<IRoomAttributeFacade, RoomAttributeFacade>();
        services.AddScoped<IRoomQualityService, RoomQualityService>();
        services.AddScoped<IBedTypeService, BedTypeService>();
        services.AddScoped<IUnitTypeService, UnitTypeService>();
        services.AddScoped<IRoomViewService, RoomViewService>();

        // Room Management
        services.AddScoped<IRoomNameSuggestionService, RoomNameSuggestionService>();

        // Request Management
        services.AddScoped<IRequestOverviewService, RequestOverviewService>();
        services.AddScoped<ICustomerUpgradeRequestService, CustomerUpgradeRequestService>();
        services.AddScoped<IAdminUpgradeRequestService, AdminUpgradeRequestService>();
        services.AddScoped<IAdminHotelApprovalRequestService, AdminHotelApprovalRequestService>();
        services.AddScoped<IHotelRegistrationService, HotelRegistrationService>();

        // User Management
        services.AddScoped<IRegisterService, RegisterService>();
        services.AddScoped<ILoginService, LoginService>();



        // Helpers
        services.AddSingleton<IImageHelper, ImageHelper>();
        services.AddScoped<IFileHelper, FileHelper>();

        return services;
    }
}