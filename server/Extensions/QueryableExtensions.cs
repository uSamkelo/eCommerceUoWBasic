using System.Reflection;
using System.Linq.Dynamic.Core;
using eCommerceUoWBasic.Models;
using Microsoft.EntityFrameworkCore;

namespace server.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            var skip = (int)Math.Min((long)(pageNumber - 1) * pageSize, int.MaxValue);
            return query
                .Skip(skip)
                .Take(pageSize);
        }

        public static IQueryable<T> ApplySort<T>(this IQueryable<T> query, string? sortBy) where T : class
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query;

            var allowedProperties = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            var sortExpressions = new List<string>();

            foreach (var part in sortBy.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var tokens = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 0 ||
                    !allowedProperties.TryGetValue(tokens[0], out var property))
                    continue;

                if (tokens.Length > 1 &&
                    !tokens[1].Equals("asc", StringComparison.OrdinalIgnoreCase) &&
                    !tokens[1].Equals("desc", StringComparison.OrdinalIgnoreCase))
                    continue;

                var direction = tokens.Length > 1 && tokens[1].Equals("desc", StringComparison.OrdinalIgnoreCase)
                    ? "descending"
                    : "ascending";

                sortExpressions.Add($"{property.Name} {direction}");
            }

            return sortExpressions.Count > 0
                ? query.OrderBy(string.Join(", ", sortExpressions))
                : query;
        }

        public static IQueryable<Product> ApplySearch(this IQueryable<Product> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            return query.Where(m =>
                EF.Functions.ILike(m.Name, $"%{search}%") ||
                EF.Functions.ILike(m.Category, $"%{search}%"));
        }
    }
}