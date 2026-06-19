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

using HotelBooking.application.Services.Domains.BookingManagement;
using HotelBooking.application.Interfaces;

public static class ApplicationServiceExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // ... (rest of core services)

        // Hotel & Business Services
        services.AddScoped<IHotelService, HotelService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IRoomTypeService, RoomTypeService>();


        // Admin Management
        services.AddScoped<IManagementAdminService, ManagementAdminService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IOwnerDashboardService, OwnerDashboardService>();
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
        services.AddScoped<IJwtAuthService, JwtAuthService>();
        services.AddScoped<IUserService, UserService>();


        // Helpers
        services.AddSingleton<IImageHelper, ImageHelper>();
        services.AddScoped<IFileHelper, FileHelper>();

        return services;
    }
}