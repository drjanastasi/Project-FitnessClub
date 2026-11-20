namespace FitnessClub.Api.Data;
public interface IUnitOfWork
{
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action);
    Task<int> SaveChangesAsync();
}
public class UnitOfWork : IUnitOfWork
{
    private readonly FitnessClubDbContext _db;
    public UnitOfWork(FitnessClubDbContext db)
    {
        _db = db;
    }
    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var result = await action();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
}
