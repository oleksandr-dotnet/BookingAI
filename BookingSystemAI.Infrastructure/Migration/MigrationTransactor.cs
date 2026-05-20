using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingSystemAI.Infrastructure.CompanyImport;

public sealed class MigrationTransactor(ApplicationDbContext dbContext) : IMigrationTransactor
{
    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await action(cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
