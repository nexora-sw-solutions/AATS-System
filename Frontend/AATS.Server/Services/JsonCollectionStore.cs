using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace AATS.Server.Services;

public sealed class JsonCollectionStore
{
    private static readonly Regex InvalidFileNameCharacters = new($"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]+", RegexOptions.Compiled);

    private readonly SemaphoreSlim _mutex = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    private readonly string _dataDirectory;

    public JsonCollectionStore(IHostEnvironment environment)
    {
        _dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(_dataDirectory);
    }

    public async Task<IReadOnlyList<JsonObject>> GetCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        await _mutex.WaitAsync(cancellationToken);

        try
        {
            return await ReadCollectionInternalAsync(collectionName, cancellationToken);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<JsonObject> AddOrReplaceAsync(
        string collectionName,
        string idField,
        JsonObject item,
        CancellationToken cancellationToken = default)
    {
        await _mutex.WaitAsync(cancellationToken);

        try
        {
            var items = await ReadCollectionInternalAsync(collectionName, cancellationToken);
            var id = GetOrCreateId(item, idField);
            var existingIndex = FindIndexById(items, idField, id);

            if (existingIndex >= 0)
            {
                items[existingIndex] = item.DeepClone().AsObject();
            }
            else
            {
                items.Add(item.DeepClone().AsObject());
            }

            await WriteCollectionInternalAsync(collectionName, items, cancellationToken);
            return item;
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<int> DeleteAsync(
        string collectionName,
        string idField,
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default)
    {
        var normalizedIds = ids
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (normalizedIds.Count == 0)
        {
            return 0;
        }

        await _mutex.WaitAsync(cancellationToken);

        try
        {
            var items = await ReadCollectionInternalAsync(collectionName, cancellationToken);
            var beforeCount = items.Count;

            items.RemoveAll(item =>
            {
                var itemId = TryGetString(item, idField);
                return itemId is not null && normalizedIds.Contains(itemId);
            });

            if (items.Count != beforeCount)
            {
                await WriteCollectionInternalAsync(collectionName, items, cancellationToken);
            }

            return beforeCount - items.Count;
        }
        finally
        {
            _mutex.Release();
        }
    }

    private async Task<List<JsonObject>> ReadCollectionInternalAsync(string collectionName, CancellationToken cancellationToken)
    {
        var path = GetFilePath(collectionName);

        if (!File.Exists(path))
        {
            await File.WriteAllTextAsync(path, "[]", cancellationToken);
            return [];
        }

        await using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var items = await JsonSerializer.DeserializeAsync<List<JsonObject>>(stream, _jsonOptions, cancellationToken);
        return items ?? [];
    }

    private async Task WriteCollectionInternalAsync(string collectionName, List<JsonObject> items, CancellationToken cancellationToken)
    {
        var path = GetFilePath(collectionName);
        await using var stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, items, _jsonOptions, cancellationToken);
    }

    private string GetFilePath(string collectionName)
    {
        var sanitizedName = InvalidFileNameCharacters.Replace(collectionName.Trim().ToLowerInvariant().Replace(' ', '-'), "_");
        return Path.Combine(_dataDirectory, $"{sanitizedName}.json");
    }

    private static string GetOrCreateId(JsonObject item, string idField)
    {
        var id = TryGetString(item, idField);

        if (string.IsNullOrWhiteSpace(id))
        {
            id = Guid.NewGuid().ToString("N");
            item[idField] = id;
        }

        return id;
    }

    private static int FindIndexById(IEnumerable<JsonObject> items, string idField, string id)
    {
        var index = 0;

        foreach (var item in items)
        {
            var currentId = TryGetString(item, idField);

            if (string.Equals(currentId, id, StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }

            index++;
        }

        return -1;
    }

    private static string? TryGetString(JsonObject item, string fieldName)
    {
        if (!item.TryGetPropertyValue(fieldName, out var value) || value is null)
        {
            return null;
        }

        return value switch
        {
            JsonValue jsonValue when jsonValue.TryGetValue<string>(out var stringValue) => stringValue,
            _ => value.ToJsonString().Trim('"')
        };
    }
}
