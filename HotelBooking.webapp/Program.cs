

using HotelBooking.webapp.Components;
using MudBlazor.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using HotelBooking.Client;
using Blazored.Toast;
using MudBlazor;
using HotelBooking.webapp.Services;

var builder = WebApplication.CreateBuilder(args);

// add service blazor

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// add service mudblazor
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});


// add service http client
builder.Services.AddHttpClient("HotelBookingAPI", client =>
{
    client.BaseAddress = new Uri("http://localhost:5083/api/");
});

// add service blazor local storage
builder.Services.AddBlazoredLocalStorage();

// Add Authentication & Authorization services
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddAuthorizationCore();

builder.Services.AddHttpContextAccessor();

// DI service
builder.Services.AddScoped<HotelFormState>();


var app = builder.Build();

// app.UseHttpsRedirection(); // kích hoạt https
app.UseRouting(); // chia các components thành page
app.UseStaticFiles(); // wwwroot thư mục tài nguyên

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
