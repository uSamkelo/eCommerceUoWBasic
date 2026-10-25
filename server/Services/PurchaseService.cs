using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using eCommerceUoWBasic.Models;
using server.Interfaces;
using server.Services;

namespace eCommerceUoWBasic.Services;

public sealed class PurchaseService(IUnitOfWork unitOfWork, ProductCacheVersion productCacheVersion)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<PurchaseOutcome> TryPurchaseAsync(
        IReadOnlyCollection<PurchaseRequest> requests,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var items = NormalizeRequests(requests);
        var requestHash = CreateRequestHash(items);
        var outcome = await unitOfWork.ExecuteInTransactionAsync(
            System.Data.IsolationLevel.Serializable,
            async (transaction, token) =>
        {
            var existing = await unitOfWork.IdempotencyRecords.GetByKeyAsync(idempotencyKey, token);

            if (existing is not null)
            {
                return existing.RequestHash == requestHash
                    ? new PurchaseOutcome(
                        existing.StatusCode,
                        JsonSerializer.Deserialize<PurchaseResponse>(
                            existing.ResponseBody,
                            JsonOptions)!)
                    : PurchaseOutcome.Conflict("The idempotency key was used for a different request.");
            }

            await transaction.CreateSavepointAsync("BeforeStock", token);
            var failedProductId = await DecrementStockAsync(items, token);

            if (failedProductId is not null)
            {
                await transaction.RollbackToSavepointAsync("BeforeStock", token);
            }

            var response = failedProductId is null
                ? PurchaseResponse.Success()
                : PurchaseResponse.InsufficientStock(failedProductId.Value);
            var statusCode = failedProductId is null ? 200 : 409;

            unitOfWork.IdempotencyRecords.Add(new IdempotencyRecord
            {
                Key = idempotencyKey,
                RequestHash = requestHash,
                StatusCode = statusCode,
                ResponseBody = JsonSerializer.Serialize(response, JsonOptions),
                CreatedAtUtc = DateTime.UtcNow
            });

            await unitOfWork.SaveChangesAsync(token);

            return new PurchaseOutcome(statusCode, response);
        },
        cancellationToken);

        if (outcome.StatusCode is 200 or 409)
        {
            productCacheVersion.Invalidate();
        }

        return outcome;
    }

    private async Task<int?> DecrementStockAsync(
        IReadOnlyList<PurchaseRequest> items,
        CancellationToken cancellationToken)
    {
        foreach (var item in items)
        {
            var decremented = await unitOfWork.Products.TryDecrementStockAsync(
                item.ProductId,
                item.Quantity,
                cancellationToken);
            if (!decremented)
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
