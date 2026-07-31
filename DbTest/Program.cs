using System;
using Microsoft.EntityFrameworkCore;
using HotelBooking.infrastructure.Models;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var optionsBuilder = new DbContextOptionsBuilder<HotelBookingDBContext>();
        optionsBuilder.UseSqlServer("Server=127.0.0.1,1434; Database=HotelBooking;User Id=sa;Password=ONLY_FOR_DEMO;TrustServerCertificate=True");
        
        using var db = new HotelBookingDBContext(optionsBuilder.Options);
        
        var id = 5;
        Console.WriteLine("Querying UpgradeRequest Id = " + id);
        var req = await db.UpgradeRequests.AsNoTracking().Include(u => u.User).FirstOrDefaultAsync(u => u.Id == id);
        if (req == null) {
            Console.WriteLine("req is null");
            
            var reqNoInclude = await db.UpgradeRequests.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            Console.WriteLine("reqNoInclude is null? " + (reqNoInclude == null));
        } else {
            Console.WriteLine("req is NOT null, Status = " + req.Status);
            Console.WriteLine("User is null? " + (req.User == null));
        }
    }
}
