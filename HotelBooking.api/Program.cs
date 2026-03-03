using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Database - Service EF
var connectionString = builder.Configuration.GetConnectionString("connectionStringHotelBooking");
builder.Services.AddDbContext<HotelBookingDBContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Business Extensions
builder.Services.AddAppRepositories();
builder.Services.AddApplicationServices();
builder.Services.AddAppValidators();

// 3. Infrastructure Extensions
builder.Services.AddSwaggerConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);

// 4. Common Services
builder.Services.AddControllers();
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

// Configure Middleware Pipeline
app.UseCors("AllowAll");

app.UseMiddleware<PerformanceMiddleware>();

//use middleware controller
app.MapControllers();

if (app.Environment.IsDevelopment()) // Chỉ hiện Swagger ở môi trường Dev cho bảo mật
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//use middleware authentication
app.UseAuthentication();
app.UseAuthorization();
app.Run();
