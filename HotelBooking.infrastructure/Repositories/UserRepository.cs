using System.Linq.Expressions;
using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IUserRepository : IRepository<User>
{
    Task<User> GetUserWithRoles(Expression<Func<User, bool>> predicate);

    // MÔ PHỎNG 3 TRƯỜNG HỢP LỌC DỮ LIỆU KHÁC NHAU
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

    #region MÔ PHỎNG 3 TRƯỜNG HỢP LỌC DỮ LIỆU KHÁC NHAU

    /// <summary>
    /// Trường hợp 1: Lọc tại SQL (Cách chuẩn - Code hiện tại của bạn)
    /// Đây là cách tối ưu nhất. EF Core sẽ dịch predicate thành WHERE trong SQL.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public async Task<User> GetUserWithRoles_SQL_Filter(Expression<Func<User, bool>> predicate)
    {
        // --- DIỄN BIẾN ---
        // 1. _dbSet: Bắt đầu IQueryable (Chưa chạy)
        // 2. .Include(): Vẫn là IQueryable (Chưa chạy, chỉ đánh dấu là sẽ JOIN bảng)
        // 3. .FirstOrDefaultAsync(predicate): CHỐT ĐƠN!

        // -> EF Core dịch: SELECT ... FROM Users JOIN Roles ... WHERE (Điều kiện của predicate)
        // -> SQL Server lọc xong, trả về ĐÚNG 1 dòng (hoặc null).
        // -> RAM Server nhận 1 dòng đó.

        var result = await _dbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(predicate); // [OK] Predicate được đưa xuống DB

        return result;
    }

    /// <summary>
    /// Trường hợp 2: Lọc tại RAM (Cách tệ - Dùng .ToListAsync() trước)
    /// Trường hợp này bạn cố tình tải hết về rồi mới lọc. 
    /// Thường dùng khi logic lọc quá phức tạp mà SQL không làm được (như mình đã ví dụ ở các câu trả lời trước), 
    /// nhưng nếu logic đơn giản mà làm thế này là sai lầm.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>

    public async Task<User> GetUserWithRoles_RAM_Explicit(Expression<Func<User, bool>> predicate)
    {
        // --- DIỄN BIẾN ---
        // 1. .ToListAsync(): CHỐT ĐƠN NGAY TẠI ĐÂY!
        // -> EF Core dịch: SELECT ... FROM Users JOIN Roles ... (KHÔNG CÓ WHERE)
        // -> SQL Server trả về TOÀN BỘ 1 triệu user.
        // -> RAM Server nhận 1 triệu user, nhét vào List<User>.
        var allUsersInRam = await _dbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ToListAsync(); // [X] Lấy hết về RAM

        // 2. Lọc thủ công trên RAM
        // Lưu ý: Vì predicate là Expression, muốn dùng cho List (RAM) phải Compile nó thành Func
        var func = predicate.Compile();

        // CPU server chạy vòng lặp duyệt 1 triệu dòng để tìm
        return allUsersInRam.FirstOrDefault(func);
    }

    /// <summary>
    /// Trường hợp 3: Lọc tại RAM (Cách bẫy - Dùng .AsEnumerable() trước)
    /// Trường hợp này rất dễ bị mắc bẫy nếu bạn không hiểu rõ về IQueryable và IEnumerable.
    /// Đây là trường hợp "không có ToList" mà bạn hỏi, 
    /// nhưng nó vẫn lọc tại RAM. Nó nguy hiểm vì nhìn code rất giống trường hợp 1, dễ bị nhầm.
    /// </summary>
    /// <param name="predicateFunc"></param>
    /// <returns></returns>
    public User GetUserWithRoles_RAM_Trap(Func<User, bool> predicateFunc) // Lưu ý: Tham số là Func, ko phải Expression
    {
        // --- DIỄN BIẾN ---
        // 1. .AsEnumerable(): Chuyển đổi vai trò từ "Giám đốc" (IQueryable) sang "Nhân viên kho" (IEnumerable)
        // -> Nó CẮT ĐỨT khả năng dịch SQL của đoạn sau.
        IEnumerable<User> query = _dbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsEnumerable(); // [!] Ngắt kết nối logic SQL tại đây

        // 2. .FirstOrDefault(predicateFunc):
        // -> Vì biến 'query' là IEnumerable, lệnh này là lệnh chạy trên RAM.
        // -> Để chạy được lệnh này, nó âm thầm gọi DB: "Lấy hết data về đây cho tao duyệt!"
        // -> SQL: SELECT ... (KHÔNG WHERE) -> Tải hết về RAM -> CPU lọc.

        return query.FirstOrDefault(predicateFunc);
    }

    #endregion
}
