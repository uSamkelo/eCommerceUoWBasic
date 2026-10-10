using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using eCommerceUoWBasic.Data;
using eCommerceUoWBasic.Models;
using Microsoft.EntityFrameworkCore;

namespace eCommerceUoWBasic.Services;

public sealed class PurchaseService(DataContext context)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<PurchaseOutcome> TryPurchaseAsync(
        IReadOnlyCollection<PurchaseRequest> requests,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var items = NormalizeRequests(requests);
        var requestHash = CreateRequestHash(items);
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable,
                cancellationToken);

            var existing = await context.IdempotencyRecords
                .SingleOrDefaultAsync(record => record.Key == idempotencyKey, cancellationToken);

            if (existing is not null)
            {
                await transaction.CommitAsync(cancellationToken);

                return existing.RequestHash == requestHash
                    ? new PurchaseOutcome(
                        existing.StatusCode,
                        JsonSerializer.Deserialize<PurchaseResponse>(
                            existing.ResponseBody,
                            JsonOptions)!)
                    : PurchaseOutcome.Conflict("The idempotency key was used for a different request.");
            }

            await transaction.CreateSavepointAsync("BeforeStock", cancellationToken);
            var failedProductId = await DecrementStockAsync(items, cancellationToken);

            if (failedProductId is not null)
            {
                await transaction.RollbackToSavepointAsync("BeforeStock", cancellationToken);
            }

            var response = failedProductId is null
                ? PurchaseResponse.Success()
                : PurchaseResponse.InsufficientStock(failedProductId.Value);
            var statusCode = failedProductId is null ? 200 : 409;

            context.IdempotencyRecords.Add(new IdempotencyRecord
            {
                Key = idempotencyKey,
                RequestHash = requestHash,
                StatusCode = statusCode,
                ResponseBody = JsonSerializer.Serialize(response, JsonOptions),
                CreatedAtUtc = DateTime.UtcNow
            });

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new PurchaseOutcome(statusCode, response);
        });
    }

    private async Task<int?> DecrementStockAsync(
        IReadOnlyList<PurchaseRequest> items,
        CancellationToken cancellationToken)
    {
        foreach (var item in items)
        {
            var affectedRows = await context.Products
                .Where(product =>
                    product.Id == item.ProductId &&
                    product.StockQuantity >= item.Quantity)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        product => product.StockQuantity,
                        product => product.StockQuantity - item.Quantity),
                    cancellationToken);

            if (affectedRows != 1)
            {
                return item.ProductId;
            }
        }

        return null;
    }

    private static List<PurchaseRequest> NormalizeRequests(
        IReadOnlyCollection<PurchaseRequest> requests)
    {
        if (requests.Count == 0)
        {
            throw new ArgumentException("At least one purchase is required.", nameof(requests));
        }

        if (requests.Any(request => request.ProductId <= 0 || request.Quantity <= 0))
        {
            throw new ArgumentException(
                "Product IDs and quantities must be greater than zero.",
                nameof(requests));
        }

        return requests
            .GroupBy(request => request.ProductId)
            .Select(group => new PurchaseRequest
            {
                ProductId = group.Key,
                Quantity = checked(group.Sum(request => request.Quantity))
            })
            .OrderBy(request => request.ProductId)
            .ToList();
    }

    private static string CreateRequestHash(IReadOnlyList<PurchaseRequest> requests)
    {
        var payload = JsonSerializer.Serialize(requests, JsonOptions);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash);
    }

    public sealed record PurchaseOutcome(int StatusCode, PurchaseResponse Response)
    {
        public static PurchaseOutcome Conflict(string message) =>
            new(409, new PurchaseResponse(message));
    }

    public sealed record PurchaseResponse(string Message, int? ProductId = null)
    {
        public static PurchaseResponse Success() =>
            new("Purchase completed.");

        public static PurchaseResponse InsufficientStock(int productId) =>
            new("One or more products no longer have enough stock.", productId);
    }
}
