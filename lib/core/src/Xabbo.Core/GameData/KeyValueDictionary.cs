using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Xabbo.Core;

public abstract class KeyValueDictionary : IReadOnlyDictionary<string, string>
{
    private readonly ImmutableDictionary<string, string> _entries;

    public IEnumerable<string> Keys => _entries.Keys;
    public IEnumerable<string> Values => _entries.Values;
    public int Count => _entries.Count;

    public string this[string key] => _entries[key];

    protected KeyValueDictionary(string filePath)
    {
        Dictionary<string, string> entries = [];

        foreach (string line in File.ReadLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            int index = line.IndexOf('=');
            if (index < 0)
            {
                Debug.Log($"Invalid line in {GetType().Name}: '{line}'.");
                continue;
            }
            string key = line[..index];
            string value = line[(index + 1)..];
            if (!entries.TryAdd(key, value))
                Debug.Log($"Duplicate key in {GetType().Name}: '{key}'.");
        }

        _entries = entries.ToImmutableDictionary();
    }

    public bool ContainsKey(string key) => _entries.ContainsKey(key);
    public bool TryGetValue(string key, [NotNullWhen(true)] out string? value) => _entries.TryGetValue(key, out value);
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _entries.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}