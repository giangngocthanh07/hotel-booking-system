using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;
public interface IUnitOfWork : IAsyncDisposable
{

    Task BeginTransactionAsync();
    Task CommitTransactionAsync();

    Task<int> SaveChangesAsync();
    Task<SaveChangesResult> TrySaveChangesAsync(CancellationToken cancellationToken = default);
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

    public async Task<SaveChangesResult> TrySaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return new SaveChangesResult(await _context.SaveChangesAsync(cancellationToken), false);
        }
        catch (DbUpdateConcurrencyException)
        {
            // FR-ONB-019: discard stale tracked changes, including an unsaved Hotel.
            // The caller must roll back any explicit transaction before returning a conflict.
            _context.ChangeTracker.Clear();
            return new SaveChangesResult(0, true);
        }
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
