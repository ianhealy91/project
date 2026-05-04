namespace Logbook.Models;

/// Generic wrapper for a paginated subset of query results.
/// Returned by GetPagedAsync to carry both the current page items
/// and the metadata needed to render pagination controls.
public class PagedResult<T>
{
    /// The items for the current page.
    public IEnumerable<T> Items { get; init; } = [];
    /// The current page number.        
    public int Page { get; init; }
    /// The maximum number of items per page.
    public int PageSize { get; init; }
    /// The total number of records across all pages.
    public int TotalCount { get; init; }
    /// The total number of pages, derived from TotalCount and PageSize.
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    /// True when the current page is not the first page.
    public bool HasPreviousPage => Page > 1;

    /// True when the current page is not the last page.
    public bool HasNextPage => Page < TotalPages;
}