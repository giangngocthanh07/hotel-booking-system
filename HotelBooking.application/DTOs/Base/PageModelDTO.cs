// 1. Class nhận tham số phân trang từ Client gửi lên
public class PagingRequest
{
    public int? PageIndex { get; set; } = 1; // Mặc định trang 1
    public int? PageSize { get; set; } = 10; // Mặc định 10 dòng
}

// 2. Class trả về kết quả kèm tổng số trang
public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }      // Tổng số bản ghi (để tính số trang)
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    // Tính toán an toàn hơn (tránh chia cho 0)
    public int TotalPages
    {
        get
        {
            if (TotalCount == 0 || PageSize == 0) return 0;
            return (int)Math.Ceiling((double)TotalCount / PageSize);
        }
    }

    public PagedResult(List<T> items, int count, int? pageIndex, int? pageSize)
    {
        Items = items;
        TotalCount = count;
        PageIndex = pageIndex ?? 1;
        PageSize = pageSize ?? 10;
    }
}