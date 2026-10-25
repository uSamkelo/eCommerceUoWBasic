using System.Data;
using eCommerceUoWBasic.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using server.Interfaces;

namespace eCommerceUoWBasic.Data;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly DataContext _context;

    public UnitOfWork(
        DataContext context,
        IProductRepository products,
        IIdempotencyRecordRepository idempotencyRecords)
    {
        _context = context;
        Products = products;
        IdempotencyRecords = idempotencyRecords;
    }

    public IProductRepository Products { get; }

    public IIdempotencyRecordRepository IdempotencyRecords { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        IsolationLevel isolationLevel,
        Func<IUnitOfWorkTransaction, CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(
                isolationLevel,
                cancellationToken);
            var result = await operation(
                new UnitOfWorkTransaction(transaction),
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        });
    }

    private sealed class UnitOfWorkTransaction(IDbContextTransaction transaction) : IUnitOfWorkTransaction
    {
        public Task CreateSavepointAsync(
            string name,
            CancellationToken cancellationToken = default) =>
            transaction.CreateSavepointAsync(name, cancellationToken);

        public Task RollbackToSavepointAsync(
            string name,
            CancellationToken cancellationToken = default) =>
            transaction.RollbackToSavepointAsync(name, cancellationToken);
    }
}
