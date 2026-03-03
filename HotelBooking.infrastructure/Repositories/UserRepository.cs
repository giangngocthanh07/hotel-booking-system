using System.Linq.Expressions;
using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IUserRepository : IRepository<User>
{
    Task<User> GetUserWithRoles(Expression<Func<User, bool>> predicate);

    // DEMONSTRATES 3 DIFFERENT DATA FILTERING APPROACHES
    Task<User> GetUserWithRoles_SQL_Filter(Expression<Func<User, bool>> predicate);
    Task<User> GetUserWithRoles_RAM_Explicit(Expression<Func<User, bool>> predicate);
    User GetUserWithRoles_RAM_Trap(Func<User, bool> predicateFunc);
}
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(HotelBookingDBContext context) : base(context) { }

    public async Task<User> GetUserWithRoles(Expression<Func<User, bool>> predicate)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(predicate);
    }

    #region DEMONSTRATES 3 DIFFERENT DATA FILTERING APPROACHES

    /// <summary>
    /// Case 1: Filter at SQL level (Best practice — current approach)
    /// Most efficient option. EF Core translates the predicate into a SQL WHERE clause.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public async Task<User> GetUserWithRoles_SQL_Filter(Expression<Func<User, bool>> predicate)
    {
        // --- EXECUTION FLOW ---
        // 1. _dbSet: Starts an IQueryable (not yet executed)
        // 2. .Include(): Still IQueryable (not executed, just marks JOIN tables)
        // 3. .FirstOrDefaultAsync(predicate): FIRES THE QUERY!

        // -> EF Core generates: SELECT ... FROM Users JOIN Roles ... WHERE (predicate condition)
        // -> SQL Server filters and returns exactly 1 row (or null).
        // -> Server RAM receives only that 1 row.

        var result = await _dbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(predicate); // [OK] Predicate is pushed down to DB

        return result;
    }

    /// <summary>
    /// Case 2: Filter in RAM (Bad practice — uses .ToListAsync() first)
    /// Intentionally loads all records into memory before filtering.
    /// Acceptable only when filtering logic is too complex for SQL, but wasteful for simple conditions.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>

    public async Task<User> GetUserWithRoles_RAM_Explicit(Expression<Func<User, bool>> predicate)
    {
        // --- EXECUTION FLOW ---
        // 1. .ToListAsync(): FIRES THE QUERY HERE!
        // -> EF Core generates: SELECT ... FROM Users JOIN Roles ... (NO WHERE clause)
        // -> SQL Server returns ALL 1 million users.
        // -> Server RAM receives 1 million users into a List<User>.
        var allUsersInRam = await _dbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ToListAsync(); // [X] Loads everything into RAM

        // 2. Filter manually in RAM
        // Note: Since predicate is an Expression, it must be compiled into a Func before use on a List
        var func = predicate.Compile();

        // Server CPU iterates over 1 million rows to find the match
        return allUsersInRam.FirstOrDefault(func);
    }

    /// <summary>
    /// Case 3: Filter in RAM (Trap — uses .AsEnumerable() first)
    /// Easy to fall into if you don't fully understand IQueryable vs IEnumerable.
    /// This case has no .ToList(), but still filters in RAM.
    /// Dangerous because the code looks almost identical to Case 1, making it easy to confuse.
    /// </summary>
    /// <param name="predicateFunc"></param>
    /// <returns></returns>
    public User GetUserWithRoles_RAM_Trap(Func<User, bool> predicateFunc) // Note: Parameter is Func, not Expression
    {
        // --- EXECUTION FLOW ---
        // 1. .AsEnumerable(): Switches the role from "Director" (IQueryable) to "Warehouse worker" (IEnumerable)
        // -> This BREAKS the SQL translation capability for everything that follows.
        IEnumerable<User> query = _dbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsEnumerable(); // [!] Disconnects from SQL translation here

        // 2. .FirstOrDefault(predicateFunc):
        // -> Since 'query' is IEnumerable, this runs in RAM.
        // -> To execute, it silently fetches all data from DB: "Give me everything so I can iterate!"
        // -> SQL: SELECT ... (NO WHERE) -> Loads all into RAM -> CPU filters.

        return query.FirstOrDefault(predicateFunc);
    }

    #endregion
}
