using System.Linq.Expressions;

public interface INamedEntity
{
    int Id { get; set; }
    string Name { get; set; }
    bool IsDeleted { get; set; }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    // Thêm thuộc tính này để phân biệt lỗi 400 và 404
    public string StatusCode { get; set; }

    // Singleton cho trường hợp Success để đỡ tốn bộ nhớ new object liên tục
    public static readonly ValidationResult SuccessResult = new ValidationResult { IsValid = true, StatusCode = StatusCodeResponse.Success };

    // Helper tạo nhanh kết quả thành công
    public static ValidationResult Success()
        => SuccessResult;

    // Helper tạo nhanh kết quả thất bại
    public static ValidationResult Fail(string message, string statusCode)
        => new ValidationResult { IsValid = false, Message = message, StatusCode = statusCode };
}

// Static Simple Factory Method
public static class ValidateFactory
{
    // ---------------------------------------------------------
    // 1. CORE GENERIC METHOD
    // ---------------------------------------------------------

    /// <summary>
    /// Kiểm tra generic: Nếu predicate trả về False -> Trả về Lỗi
    /// </summary>
    public static ValidationResult Require<T>(T value, Func<T, bool> predicate, string errorMessage, string statusCode)
    {
        // Nếu điều kiện đúng -> Trả về null (Pass)
        if (predicate(value)) return ValidationResult.Success();

        // Nếu sai -> Trả về lỗi
        return ValidationResult.Fail(errorMessage, statusCode);
    }

    /// <summary>
    /// Helper: Yêu cầu danh sách không được rỗng
    /// </summary>
    public static ValidationResult RequireNotEmptyList<T>(IEnumerable<T>? list)
    {
        return Require(list, x =>
        {
            if (x == null || !x.Any()) return false;

            // Tối ưu hiệu năng: Nếu là ICollection thì dùng Count (O(1))
            if (x is ICollection<T> collection) return collection.Count > 0;

            // Nếu là IEnumerable thường thì dùng Any() (O(1) -> O(N))
            return x.Any();

        }
        , MessageResponse.EMPTY_LIST, StatusCodeResponse.BadRequest);
    }

    /// <summary>
    /// Helper: Yêu cầu tên không được rỗng
    /// </summary>
    public static ValidationResult RequireNotEmpty(string? value)
    {
        return Require(value, x => !string.IsNullOrWhiteSpace(value), MessageResponse.EMPTY_NAME, StatusCodeResponse.BadRequest);
    }

    /// <summary>
    /// Helper: Yêu cầu object không được null
    /// </summary>
    public static ValidationResult RequireNotNull<T>(T value)
    {
        // Logic: Nếu obj khác null -> Pass (null)
        // Nếu obj null -> Fail với statusCode (mặc định là 404)
        return Require(value, x => x is not null, MessageResponse.NOT_FOUND, StatusCodeResponse.NotFound);
    }


    // ---------------------------------------------------------
    // 3. AGGREGATOR (Thay thế cho toán tử ??)
    // ---------------------------------------------------------

    /// <summary>
    /// Chạy một loạt các rule, trả về lỗi ĐẦU TIÊN gặp phải.
    /// Nếu tất cả đều qua -> Trả về Success.
    /// </summary>

    public static ValidationResult BasicCheck(params ValidationResult[] results)
    {
        foreach (var result in results)
        {
            if (!result.IsValid)
            {
                return result; // Trả về lỗi đầu tiên gặp phải
            }
        }
        return ValidationResult.Success(); // Tất cả đều hợp lệ
    }

    // ---------------------------------------------------------
    // 4. DYNAMIC VALIDATION (Dùng Expression Tree để build điều kiện động)
    // ---------------------------------------------------------

    /// <summary>
    /// Kiểm tra trùng tên động cho các Entity có cấu trúc giống nhau (Id, Name, IsDeleted).
    /// </summary>
    /// <typeparam name="TEntity">Loại Entity cần kiểm tra (phải có Id, Name, IsDeleted)</typeparam>
    /// <param name="repo">Repository của Entity cần kiểm tra</param>
    /// <param name="nameValue">Giá trị Name cần kiểm tra trùng</param>
    /// <param name="idValue">Giá trị Id (nếu có) để loại trừ khi cập nhật</param>
    /// <returns>True nếu trùng, False nếu không trùng</returns>
    /// 

    public static async Task<ValidationResult> ValidateFullAsync<TEntity>(
        IRepository<TEntity> repo,
        string name,
        int? id = null,
        int? typeId = null,
        // [Thay thế Reflection]: Delegate lấy TypeId (tránh Boxing)
        Func<TEntity, int>? getEntityTypeIdFunc = null,

        // [Thay thế Reflection]: Delegate lấy IsDeleted (tránh Boxing)
        Func<TEntity, bool?>? getEntityIsDeletedFunc = null,

        // [Thay thế Magic String "Name"]: Expression để chọn cột Name (tránh sai tên)
        Expression<Func<TEntity, string>>? nameSelector = null,
        Expression<Func<TEntity, bool?>>? isDeletedSelector = null
    ) where TEntity : class
    {

        // 1. Check rỗng (Basic check)
        var basicCheck = RequireNotEmpty(name);
        if (!basicCheck.IsValid) return basicCheck;

        // 2. CHECK VALIDITY CỦA TYPEID
        // Nếu Entity có cột TypeId -> Thì bắt buộc giá trị truyền vào phải hợp lệ (>0)
        // Thay vì check type.GetProperty("TypeId"), ta check xem caller có truyền hàm lấy Id không
        if (getEntityTypeIdFunc != null)
        {
            // Nếu user không truyền typeId hoặc truyền số <= 0 -> Lỗi ngay
            if (!typeId.HasValue || typeId.Value <= 0)
            {
                return ValidationResult.Fail(MessageResponse.BAD_REQUEST, StatusCodeResponse.BadRequest);
            }
        }

        // 3. Check ID tồn tại (Dùng Reflection để lấy Id và IsDeleted)
        // Lưu ý: Phần này chạy trên Memory sau khi get DB nên dùng Reflection ok
        if (id.HasValue)
        {
            // Phải query lấy entity cũ lên để so sánh
            // Lưu ý: EF Core có cơ chế Cache, nên việc query ở đây và query lại ở hàm UpdateAsync 
            // thường không ảnh hưởng đáng kể hiệu năng (nó lấy từ bộ nhớ đệm).
            var existingEntity = await repo.GetByIdAsync(id.Value);
            var foundCheck = RequireNotNull(existingEntity);
            if (!foundCheck.IsValid) return foundCheck;

            // A. Check IsDeleted (Dùng Delegate - SIÊU NHANH & KHÔNG BOXING)
            if (getEntityIsDeletedFunc != null)
            {
                // Code cũ: (bool?)prop.GetValue(entity) -> Boxing, Unboxing
                // Code mới: Chạy trực tiếp
                if (getEntityIsDeletedFunc(existingEntity) == true)
                {
                    return ValidationResult.Fail(MessageResponse.NOT_FOUND, StatusCodeResponse.NotFound);
                }
            }

            // B. [QUAN TRỌNG] Check Nhất quán TypeId (Không cho phép đổi Type khi Update)
            // Nếu bảng có cột TypeId -> So sánh giá trị cũ (DB) và mới (Input)
            // B. Check Nhất quán TypeId
            if (getEntityTypeIdFunc != null && typeId.HasValue)
            {
                // Code cũ: (int)prop.GetValue(entity) -> Boxing, Unboxing
                // Code mới: Chạy trực tiếp
                var dbTypeId = getEntityTypeIdFunc(existingEntity);

                if (dbTypeId != typeId.Value)
                {
                    return ValidationResult.Fail(MessageResponse.BAD_REQUEST, StatusCodeResponse.BadRequest);
                }
            }
        }

        // 4. Check trùng tên (QUAN TRỌNG: PHẢI DỰNG EXPRESSION TREE ĐỂ EF HIỂU)
        // Mục tiêu: Tạo ra câu lệnh lambda: x => x.Name == name && x.Id != id && x.IsDeleted == false

        // 4. Check trùng tên (Truyền Selector vào để dựng Expression an toàn)
        // Nếu không truyền selector thì mặc định bỏ qua check trùng (hoặc handle lỗi tùy logic)
        if (nameSelector != null)
        {


            var isDuplicate = await CheckDuplicateDynamicAsync(repo, name, id, nameSelector, isDeletedSelector);

            if (isDuplicate)
            {
                return ValidationResult.Fail(MessageResponse.NAME_ALREADY_EXISTS, StatusCodeResponse.Conflict);
            }
        }

        return ValidationResult.Success();
    }


    private static async Task<bool> CheckDuplicateDynamicAsync<TEntity>(
        IRepository<TEntity> repo,
        string nameValue,
        int? idValue,
        Expression<Func<TEntity, string>> nameSelector,  // Input: x => x.Name
        Expression<Func<TEntity, bool?>>? isDeletedSelector = null
    ) where TEntity : class
    {
        // Lấy tham số 'x' từ nameSelector để dùng chung cho tất cả
        var parameter = nameSelector.Parameters[0];

        // 1. Điều kiện x.Name == nameValue
        var nameConst = Expression.Constant(nameValue);
        var nameEqual = Expression.Equal(nameSelector.Body, nameConst);

        Expression finalBody = nameEqual;

        // 2. Điều kiện x.IsDeleted == false (Hỗ trợ cả bool và bool?)

        if (isDeletedSelector != null)
        {
            // Kỹ thuật: Invocation (Gọi Expression B bằng tham số của Expression A)
            // Ý nghĩa: "Chạy logic lấy IsDeleted trên chính cái biến 'x' này"
            // Code sinh ra dạng: Invoke(y => y.IsDeleted, x)
            var invokedIsDeleted = Expression.Invoke(isDeletedSelector, parameter);
            var falseConstant = Expression.Constant(false);

            Expression isNotDeletedExpression;

            // Kỹ thuật Coalesce: (x.IsDeleted ?? false) == false
            // Ý nghĩa: Nếu IsDeleted là NULL thì coi như là FALSE, sau đó so sánh với FALSE
            // Cách này an toàn nhất cho cả bool và bool?
            // Kiểm tra Type của kết quả Invoke để áp dụng Coalesce
            if (invokedIsDeleted.Type == typeof(bool?))    // Nếu là nullable
            {
                // Logic: (x.IsDeleted ?? false) == false
                var coalesce = Expression.Coalesce(invokedIsDeleted, falseConstant);
                isNotDeletedExpression = Expression.Equal(coalesce, falseConstant);
            }
            else    // Nếu là bool thường
            {
                isNotDeletedExpression = Expression.Equal(invokedIsDeleted, falseConstant);
            }

            // Nối vào bằng AND
            finalBody = Expression.AndAlso(finalBody, isNotDeletedExpression);
        }

        // 3. Nếu là Update (idValue != null), => Thêm điều kiện x.Id != idValue - Logic "Trừ tôi ra" (Exclude Self)
        if (idValue.HasValue)
        {
            var idProp = Expression.Property(parameter, "Id");
            var idConst = Expression.Constant(idValue.Value);
            var idNotEqual = Expression.NotEqual(idProp, idConst);

            // Kết hợp với biểu thức hiện tại
            finalBody = Expression.AndAlso(finalBody, idNotEqual);
        }

        // Tạo Lambda Expression hoàn chỉnh
        var lambda = Expression.Lambda<Func<TEntity, bool>>(finalBody, parameter);

        // Sử dụng AnyAsync với biểu thức Lambda đã tạo
        return await repo.AnyAsync(lambda);
    }
}

