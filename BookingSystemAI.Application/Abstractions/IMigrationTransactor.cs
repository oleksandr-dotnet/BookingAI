namespace BookingSystemAI.Application.Abstractions;

public interface IMigrationTransactor
{
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);
}
