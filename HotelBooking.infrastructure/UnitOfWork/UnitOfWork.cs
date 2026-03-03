using HotelBooking.infrastructure.Models;
public interface IUnitOfWork : IAsyncDisposable
{

    Task BeginTransactionAsync();
    Task CommitTransactionAsync();

    Task<int> SaveChangesAsync();
    Task RollBackTransactionAsync();
}

public class UnitOfWork : IUnitOfWork
{

    private readonly HotelBookingDBContext _context;

    public UnitOfWork(HotelBookingDBContext context)
    {
        _context = context;

    }
    // Methods used for LINQ-based operations
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }


    // Methods used for raw SQL transactions
    public async Task BeginTransactionAsync()
    {
        await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await _context.Database.CommitTransactionAsync();

    }

    public async Task RollBackTransactionAsync()
    {
        await _context.Database.RollbackTransactionAsync();
    }
}