namespace BookingSystemAI.Infrastructure.Sql;

public interface ISqlScriptLoader
{
    string Load(string scriptName);
}
