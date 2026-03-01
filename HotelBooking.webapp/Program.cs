using MudBlazor.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using HotelBooking.Client;
using MudBlazor;
using HotelBooking.webapp.Services;
using HotelBooking.webapp.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIG SERVICES ---

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// MudBlazor
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

// Authentication & Authorization
builder.Services.AddAuthentication(); // Cơ chế đăng nhập
builder.Services.AddAuthorization();  // Cơ chế phân quyền
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddAuthorizationCore(); // Quan trọng cho Blazor

builder.Services.AddHttpContextAccessor();
builder.Services.AddBlazoredLocalStorage();

// 3. Đăng ký HttpClient có gắn Interceptor
builder.Services.AddHttpClient("HotelBookingAPI", client =>
{
    client.BaseAddress = new Uri("http://localhost:5083/api/");
});

// DI Services
builder.Services.AddScoped<HotelFormState>();
builder.Services.AddScoped<IManagementService, ManagementService>();
builder.Services.AddScoped<IRequestService, RequestService>();


var app = builder.Build();

// --- 2. CONFIG MIDDLEWARE (PIPELINE) ---

// app.UseHttpsRedirection(); // kích hoạt https
// 2.1. Chuyển hướng HTTPS (Nên để đầu tiên)
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();

// 2.2. File tĩnh (CSS, JS, Ảnh) - Cho phép truy cập KHÔNG CẦN Auth
app.UseStaticFiles();

// 2.3. Routing (Định tuyến)
app.UseRouting();

// 2.4. Authentication & Authorization (Phải nằm SAU Routing và TRƯỚC Endpoints)
app.UseAuthentication();
app.UseAuthorization();

// 2.5. Endpoints
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
