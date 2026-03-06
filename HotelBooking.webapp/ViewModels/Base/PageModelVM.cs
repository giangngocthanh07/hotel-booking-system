// 1. DTO for receiving pagination parameters from the Client
public class PagingRequest
{
    /// <summary>
    /// Current page number (1-based index). Defaults to 1.
    /// </summary>
    public int? PageIndex { get; set; } = 1;

    /// <summary>
    /// Number of items per page. Defaults to 10.
    /// </summary>
    public int? PageSize { get; set; } = 10;
}

// 2. Generic wrapper for returning paginated results with metadata
public class PagedResult<T>
{
    /// <summary>
    /// List of data items for the current page.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Total number of records across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The current page index.
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of available pages (calculated by the backend).
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Indicates if there is a page before the current one.
    /// </summary>
    public bool HasPreviousPage => PageIndex > 1;

    /// <summary>
    /// Indicates if there is another page after the current one.
    /// </summary>
    public bool HasNextPage => PageIndex < TotalPages;
}