using System.Data;

namespace server.Interfaces;

public interface IUnitOfWork
{
    IProductRepository Products { get; }
    IIdempotencyRecordRepository IdempotencyRecords { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        IsolationLevel isolationLevel,
        Func<IUnitOfWorkTransaction, CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default);
}
