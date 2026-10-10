namespace server.Common
{
    public class ProductQueryFilter
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public string? Search { get; set; }
    }
}