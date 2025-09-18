using Blazored.LocalStorage;
using HotelBooking.webapp.Data;

var builder = WebApplication.CreateBuilder(args);

// add service blazor

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// add service http client
builder.Services.AddHttpClient("HotelBookingAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:5083/api/");
});

// add service blazor local storage
builder.Services.AddBlazoredLocalStorage();

var app = builder.Build();

app.UseHttpsRedirection(); // kích hoạt https
app.UseRouting(); // chia các components thành page
app.UseStaticFiles(); // wwwroot thư mục tài nguyên


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
