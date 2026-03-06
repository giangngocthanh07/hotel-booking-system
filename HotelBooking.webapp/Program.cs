using MudBlazor.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using HotelBooking.Client;
using MudBlazor;
using HotelBooking.webapp.Services;
using HotelBooking.webapp.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. SERVICE CONFIGURATION (DI Container)
// ==========================================

// Add framework services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// MudBlazor Component Library configuration
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

// Authentication & Authorization Setup
builder.Services.AddAuthentication(); // Login mechanics
builder.Services.AddAuthorization();  // Permission mechanics
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddAuthorizationCore(); // Essential for Blazor authorization components

builder.Services.AddHttpContextAccessor();
builder.Services.AddBlazoredLocalStorage();

// Register Named HttpClient for API communication
builder.Services.AddHttpClient("HotelBookingAPI", client =>
{
    // Ensure the backend API is running on this port
    client.BaseAddress = new Uri("http://localhost:5083/api/");
});

// Custom Application Services (Dependency Injection)
builder.Services.AddScoped<HotelFormState>();
builder.Services.AddScoped<IManagementService, ManagementService>();
builder.Services.AddScoped<IRequestService, RequestService>();


var app = builder.Build();

// ==========================================
// 2. MIDDLEWARE CONFIGURATION (Pipeline)
// ==========================================

// 2.1. HSTS & HTTPS Redirection (Security)
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();

// 2.2. Static Files (CSS, JS, Images) - Accessible without authentication
app.UseStaticFiles();

// 2.3. Routing
app.UseRouting();

// 2.4. Auth Middleware (Order is critical: MUST be after Routing and before Endpoints)
app.UseAuthentication();
app.UseAuthorization();

// 2.5. Endpoints Mapping
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();