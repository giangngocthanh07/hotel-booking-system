using System.Security.Claims;
using System.Text;
using HotelBooking.application.Services;
using HotelBooking.application.Services.Domains.AdminManagement;
using HotelBooking.application.Services.Domains.HotelManagement;
using HotelBooking.application.Services.Domains.UserManagement;
using HotelBooking.application.Services.Domains.RequestManagement;
using HotelBooking.infrastructure.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using HotelBooking.application.Interfaces;
using Microsoft.OpenApi.Models;
using FluentValidation;
using HotelBooking.application.Validators.AdminManagement.Amenity;


var builder = WebApplication.CreateBuilder(args);

// Service EF
var connectionString = builder.Configuration.GetConnectionString("connectionStringHotelBooking");
builder.Services.AddDbContext<HotelBookingDBContext>(options =>
    options.UseSqlServer(connectionString));

// DI for Repository
builder.Services.AddScoped<IAmenityTypeRepository, AmenityTypeRepository>();
builder.Services.AddScoped<IAmenityRepository, AmenityRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IRoomImageRepository, RoomImageRepository>();
builder.Services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUpgradeRequestRepository, UpgradeRequestRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IRoomServiceRepository, RoomServiceRepository>();
builder.Services.AddScoped<IRoomAmenityRepository, RoomAmenityRepository>();
builder.Services.AddScoped<IHotelPolicyRepository, HotelPolicyRepository>();
builder.Services.AddScoped<IHotelAmenityRepository, HotelAmenityRepository>();
builder.Services.AddScoped<IHotelImageRepository, HotelImageRepository>();
builder.Services.AddScoped<IBookingRoomRepository, BookingRoomRepository>();
builder.Services.AddScoped<IPolicyTypeRepository, PolicyTypeRepository>();
builder.Services.AddScoped<IServiceTypeRepository, ServiceTypeRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IUnitTypeRepository, UnitTypeRepository>();
builder.Services.AddScoped<IBedTypeRepository, BedTypeRepository>();
builder.Services.AddScoped<IRoomViewRepository, RoomViewRepository>();
builder.Services.AddScoped<IRoomQualityRepository, RoomQualityRepository>();
builder.Services.AddScoped<IRoomQualityGroupRepository, RoomQualityGroupRepository>();


// DI for UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<JwtAuthService>();

// DI for Service
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<IUpgradeRequestService, UpgradeRequestService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();

// DI for Admin Management Services
builder.Services.AddScoped<IManagementAdminService, ManagementAdminService>();
builder.Services.AddScoped<IAmenityService, AmenityService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IServiceService, ServiceService>();

builder.Services.AddScoped<IAmenityService, AmenityService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IRoomQualityService, RoomQualityService>();

builder.Services.AddScoped<IRoomViewService, RoomViewService>();
builder.Services.AddScoped<IBedTypeService, BedTypeService>();
builder.Services.AddScoped<IUnitTypeService, UnitTypeService>();

builder.Services.AddScoped<IRoomAttributeFacade, RoomAttributeFacade>();
builder.Services.AddScoped<IManagementAdminService, ManagementAdminService>();

// DI for Helpers
builder.Services.AddSingleton<IImageHelper, ImageHelper>();
builder.Services.AddScoped<IFileHelper, FileHelper>();

// Quét toàn bộ Assembly chứa class Validator này và đăng ký hết
builder.Services.AddValidatorsFromAssemblyContaining<AmenityCreateValidator>();

//Use map controller
builder.Services.AddControllers();

// Cài đặt auto-mapper
// builder.Services.AddAutoMapper(typeof(Program));

// ============== Swagger =============== //
//Swagger cấu hình có điền Authentication
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "HotelBooking API", Version = "v1" });

    // 🔥 Thêm hỗ trợ Authorization header tất cả api
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token vào ô bên dưới theo định dạng: Bearer {token}"
    });

    // 🔥 Định nghĩa yêu cầu sử dụng Authorization trên từng api
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    // BẬT 2 DÒNG NÀY:
    options.UseAllOfForInheritance(); // Hiển thị kế thừa
    options.UseOneOfForPolymorphism(); // Hiển thị đa hình (OneOf)

    // Thêm Enum Schema Filter để hiển thị mô tả Enum
    options.SchemaFilter<EnumSchemaFilter>();
});

// ============== JWT Authentication & Authorization =============== //
// Cài đặt Jwt Bearer Authentication
// Thêm middleware authentication
var privateKey = builder.Configuration["jwt:Serect-Key"];
var Issuer = builder.Configuration["jwt:Issuer"];
var Audience = builder.Configuration["jwt:Audience"];
// Thêm dịch vụ Authentication vào ứng dụng, sử dụng JWT Bearer làm phương thức xác thực
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    // Thiết lập các tham số xác thực token
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        // Kiểm tra và xác nhận Issuer (nguồn phát hành token)
        ValidateIssuer = true,
        ValidIssuer = Issuer, // Biến `Issuer` chứa giá trị của Issuer hợp lệ
                              // Kiểm tra và xác nhận Audience (đối tượng nhận token)
        ValidateAudience = true,
        ValidAudience = Audience, // Biến `Audience` chứa giá trị của Audience hợp lệ
                                  // Kiểm tra và xác nhận khóa bí mật được sử dụng để ký token
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey!)),
        // Sử dụng khóa bí mật (`privateKey`) để tạo SymmetricSecurityKey nhằm xác thực chữ ký của token
        // Giảm độ trễ (skew time) của token xuống 0, đảm bảo token hết hạn chính xác
        ClockSkew = TimeSpan.Zero,
        // Xác định claim chứa vai trò của user (để phân quyền)
        RoleClaimType = ClaimTypes.Role,
        // Xác định claim chứa tên của user
        NameClaimType = ClaimTypes.Name,
        // Kiểm tra thời gian hết hạn của token, không cho phép sử dụng token hết hạn
        ValidateLifetime = true
    };
});

// Thêm dịch vụ Authorization để hỗ trợ phân quyền người dùng
builder.Services.AddAuthorization();

// Khai báo JWT AUTH SERVICE
// builder.Services.AddScoped<JwtAuthService>();

// Thêm CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

/*  =============== BUILD APP =============== */
var app = builder.Build();

// app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseMiddleware<PerformanceMiddleware>();

//use middleware controller
app.MapControllers();

//use swagger
app.UseSwagger();
app.UseSwaggerUI();

//use middleware authentication
app.UseAuthentication();
app.UseAuthorization();
app.Run();
