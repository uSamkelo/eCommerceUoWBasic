using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Common
{
    public class PagedResponse<T>
    {
        public IReadOnlyList<T> Data { get; init; } = [];
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }
        public int TotalRecords { get; init; }
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }
}