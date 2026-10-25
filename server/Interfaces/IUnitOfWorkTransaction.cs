namespace server.Interfaces;

public interface IUnitOfWorkTransaction
{
    Task CreateSavepointAsync(string name, CancellationToken cancellationToken = default);
    Task RollbackToSavepointAsync(string name, CancellationToken cancellationToken = default);
}
