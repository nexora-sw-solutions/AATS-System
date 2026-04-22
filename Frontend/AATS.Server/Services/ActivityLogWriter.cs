using System.Text.Json.Nodes;

namespace AATS.Server.Services;

public sealed class ActivityLogWriter
{
    private readonly JsonCollectionStore _store;

    public ActivityLogWriter(JsonCollectionStore store)
    {
        _store = store;
    }

    public Task WriteAsync(
        string action,
        string module,
        string details,
        string branch,
        CancellationToken cancellationToken = default)
    {
        var entry = new JsonObject
        {
            ["id"] = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
            ["timestamp"] = DateTime.UtcNow,
            ["user"] = "Desktop User",
            ["action"] = action,
            ["module"] = module,
            ["branch"] = string.IsNullOrWhiteSpace(branch) ? "Central" : branch,
            ["details"] = details
        };

        return _store.AddOrReplaceAsync("activity-logs", "id", entry, cancellationToken);
    }
}
