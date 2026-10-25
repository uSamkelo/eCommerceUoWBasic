using eCommerceUoWBasic.Models;

namespace server.Interfaces;

public interface IIdempotencyRecordRepository
{
    Task<IdempotencyRecord?> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default);
    void Add(IdempotencyRecord record);
}
