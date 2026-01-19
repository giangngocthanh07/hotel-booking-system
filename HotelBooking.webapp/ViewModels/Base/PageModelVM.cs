// 1. Class nhận tham số phân trang từ Client gửi lên
public class PagingRequest
{
    public int? PageIndex { get; set; } = 1; // Mặc định trang 1
    public int? PageSize { get; set; } = 10; // Mặc định 10 dòng
}

// 2. Class trả về kết quả kèm tổng số trang
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }      // Tổng số bản ghi (để tính số trang)
    public int PageIndex { get; set; }
    public int PageSize { get; set; }

    // Backend đã tính toán và gửi số này về trong JSON,
    // Frontend chỉ việc hứng lấy (get; set;)
    public int TotalPages { get; set; }

}