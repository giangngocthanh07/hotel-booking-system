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

    private readonly HotelBookingContext _HBcontext;

    public UnitOfWork(HotelBookingContext HBcontext)
    {
        _HBcontext = HBcontext;

    }
    //2 phương thức sử dụng cho LinQ
    public async Task<int> SaveChangesAsync()
    {
        return await _HBcontext.SaveChangesAsync();
    }
    public async Task<int> SaveChanges()
    {
        return _HBcontext.SaveChanges();
    }
    public async ValueTask DisposeAsync()
    {
        await _HBcontext.DisposeAsync();
    }


    //3 phương thức bên dưới sử dụng cho SQLRaw
    public async Task BeginTransactionAsync()
    {
        await _HBcontext.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await _HBcontext.Database.CommitTransactionAsync();

    }

    public async Task RollBackTransactionAsync()
    {
        await _HBcontext.Database.RollbackTransactionAsync();
    }
}