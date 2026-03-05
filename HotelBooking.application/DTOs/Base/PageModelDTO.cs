// 1. Class for receiving pagination parameters from Client
public class PagingRequest
{
    public int? PageIndex { get; set; } = 1; // Default page 1
    public int? PageSize { get; set; } = 10; // Default 10 items per page
}

// 2. Class for returning results with total pages
public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }      // Total number of records (to calculate pages)
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    // Safer calculation (avoid division by 0)
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