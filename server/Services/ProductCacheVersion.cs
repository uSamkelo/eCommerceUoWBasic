namespace server.Services;

public sealed class ProductCacheVersion
{
    private long _current;

    public long Current => Interlocked.Read(ref _current);

    public void Invalidate() => Interlocked.Increment(ref _current);
}
