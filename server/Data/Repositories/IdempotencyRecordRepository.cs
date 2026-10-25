using eCommerceUoWBasic.Data;
using eCommerceUoWBasic.Models;
using Microsoft.EntityFrameworkCore;
using server.Interfaces;

namespace eCommerceUoWBasic.Data.Repositories;

public sealed class IdempotencyRecordRepository(DataContext context) : IIdempotencyRecordRepository
{
    public Task<IdempotencyRecord?> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default) =>
        context.IdempotencyRecords.SingleOrDefaultAsync(
            record => record.Key == key,
            cancellationToken);

    public void Add(IdempotencyRecord record) => context.IdempotencyRecords.Add(record);
}
