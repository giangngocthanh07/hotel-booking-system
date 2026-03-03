using FluentValidation;
using HotelBooking.application.DTOs.User.Login;
using HotelBooking.application.DTOs.User.Register;
using HotelBooking.application.Services;
using HotelBooking.application.Services.Domains.UserManagement;
using HotelBooking.application.Validators.UserManagement.Login;
using HotelBooking.application.Validators.UserManagement.Register;
using HotelBooking.infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HotelBooking.Tests.Integration.Base;

public static class TestServiceCollections
{
    public static IServiceCollection AddTestBase(
        this IServiceCollection services,
        HotelBookingDBContext dbContext,
        IConfiguration config)
    {
        services.AddSingleton(dbContext);
        services.AddSingleton(config);
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<JwtAuthService>();
        return services;
    }

    public static IServiceCollection AddUserServiceDependencies(
        this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IUpgradeRequestRepository, UpgradeRequestRepository>();

        services.AddScoped<IValidator<RegisterCustomerDTO>, RegisterCustomerValidator>();
        services.AddScoped<IValidator<RegisterAdminDTO>, RegisterAdminValidator>();
        services.AddScoped<IValidator<LoginUserDTO>, LoginValidator>();

        services.AddScoped<IUserService, UserService>();
        return services;
    }

    // Sau này thêm domain mới thì thêm vào đây
    // public static IServiceCollection AddHotelServiceDependencies(
    //     this IServiceCollection services)
    // {
    //     services.AddScoped<IHotelRepository, HotelRepository>();
    //     services.AddScoped<IHotelService, HotelService>();
    //     return services;
    // }
}