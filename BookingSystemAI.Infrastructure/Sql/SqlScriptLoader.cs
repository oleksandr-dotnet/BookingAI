using System.Reflection;

namespace BookingSystemAI.Infrastructure.Sql;

public sealed class SqlScriptLoader : ISqlScriptLoader
{
    private static readonly Assembly Assembly = typeof(SqlScriptLoader).Assembly;

    public string Load(string scriptName)
    {
        var resourceName = $"BookingSystemAI.Infrastructure.SqlScripts.{scriptName}";
        using var stream = Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"SQL script '{scriptName}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
