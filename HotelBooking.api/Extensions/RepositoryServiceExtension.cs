public static class RepositoryServiceExtension
{
    public static IServiceCollection AddAppRepositories(this IServiceCollection services)
    {
        services.AddUserRepositories();
        services.AddHotelRepositories();
        services.AddAdminRepositories();

        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<IServiceTypeRepository, ServiceTypeRepository>();
        services.AddScoped<IPolicyTypeRepository, PolicyTypeRepository>();
        services.AddScoped<IRoomQualityGroupRepository, RoomQualityGroupRepository>();

        // Ensure related repos are present
        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

    public static IServiceCollection AddUserRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IUpgradeRequestRepository, UpgradeRequestRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        return services;
    }

    public static IServiceCollection AddHotelRepositories(this IServiceCollection services)
    {
        services.AddScoped<IHotelRepository, HotelRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
        services.AddScoped<IHotelPolicyRepository, HotelPolicyRepository>();
        services.AddScoped<IHotelAmenityRepository, HotelAmenityRepository>();
        services.AddScoped<IHotelImageRepository, HotelImageRepository>();
        // ... Add related hotel repos here
        return services;
    }

    public static IServiceCollection AddAdminRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAmenityTypeRepository, AmenityTypeRepository>();
        services.AddScoped<IAmenityRepository, AmenityRepository>();
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        // ... Add related admin repos here
        return services;
    }
}