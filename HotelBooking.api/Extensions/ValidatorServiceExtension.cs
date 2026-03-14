using FluentValidation;
using HotelBooking.application.DTOs.User.Login;
using HotelBooking.application.DTOs.User.Register;
using HotelBooking.application.DTOs.Request.UpgradeRequest;
using HotelBooking.application.Validators.AdminManagement.Amenities;
using HotelBooking.application.Validators.AdminManagement.Policies;
using HotelBooking.application.Validators.AdminManagement.Services;
using HotelBooking.application.Validators.AdminManagement.RoomAttributes;
using HotelBooking.application.Validators.Common;
using HotelBooking.application.Validators.UserManagement.Login;
using HotelBooking.application.Validators.UserManagement.Register;
using HotelBooking.application.Validators.UserManagement;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Validators.RoomManagement;

public static class ValidatorServiceExtension
{
    public static IServiceCollection AddAppValidators(this IServiceCollection services)
    {
        services.AddUserValidators();
        services.AddAdminManagementValidators();
        services.AddRoomManagementValidators();
        services.AddCommonValidators();

        return services;
    }

    private static void AddUserValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<RegisterCustomerDTO>, RegisterCustomerValidator>();
        services.AddScoped<IValidator<RegisterAdminDTO>, RegisterAdminValidator>();
        services.AddScoped<IValidator<LoginUserDTO>, LoginValidator>();
        services.AddScoped<IValidator<CreateUpgradeRequestDTO>, CreateUpgradeRequestValidator>();
    }

    private static void AddAdminManagementValidators(this IServiceCollection services)
    {
        // Amenity
        services.AddScoped<IValidator<AmenityCreateDTO>, AmenityCreateValidator>();
        services.AddScoped<IValidator<AmenityUpdateDTO>, AmenityUpdateValidator>();

        // Policy
        services.AddScoped<IValidator<PolicyCreateDTO>, PolicyCreateValidator>();
        services.AddScoped<IValidator<PolicyUpdateDTO>, PolicyUpdateValidator>();

        // Service
        services.AddScoped<IValidator<ServiceCreateDTO>, ServiceCreateValidator>();
        services.AddScoped<IValidator<ServiceUpdateDTO>, ServiceUpdateValidator>();

        // Room Attributes (Quality, View, Bed, Unit)
        services.AddScoped<IValidator<RoomQualityCreateDTO>, RoomQualityCreateValidator>();
        services.AddScoped<IValidator<RoomQualityUpdateDTO>, RoomQualityUpdateValidator>();
        services.AddScoped<IValidator<RoomViewCreateDTO>, RoomViewCreateValidator>();
        services.AddScoped<IValidator<RoomViewUpdateDTO>, RoomViewUpdateValidator>();
        services.AddScoped<IValidator<BedTypeCreateDTO>, BedTypeCreateValidator>();
        services.AddScoped<IValidator<BedTypeUpdateDTO>, BedTypeUpdateValidator>();
        services.AddScoped<IValidator<UnitTypeCreateDTO>, UnitTypeCreateValidator>();
        services.AddScoped<IValidator<UnitTypeUpdateDTO>, UnitTypeUpdateValidator>();
    }

    public static void AddRoomManagementValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<RoomTypeCreateDTO>, RoomTypeValidator>();
        // Add other room management validators here (e.g., for updating room types, managing rooms, etc.)
    }

    private static void AddCommonValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<ManageMenuRequest>, ManageMenuRequestValidator>();
        services.AddScoped<IValidator<PagingRequest>, PagingRequestValidator>();
        services.AddScoped<IValidator<GetRoomAttributeRequest>, GetRoomAttributeRequestValidator>();
    }
}