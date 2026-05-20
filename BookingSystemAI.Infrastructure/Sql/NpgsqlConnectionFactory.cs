using Microsoft.Extensions.Configuration;
using Npgsql;

namespace BookingSystemAI.Infrastructure.Sql;

public interface INpgsqlConnectionFactory
{
    NpgsqlConnection CreateConnection();
}

public sealed class NpgsqlConnectionFactory(IConfiguration configuration) : INpgsqlConnectionFactory
{
    public NpgsqlConnection CreateConnection()
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured.");
        return new NpgsqlConnection(connectionString);
    }
}
