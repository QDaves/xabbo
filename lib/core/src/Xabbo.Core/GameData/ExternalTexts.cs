using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Xabbo.Core.GameData;

/// <summary>
/// Defines a dictionary of external texts.
/// </summary>
public sealed class ExternalTexts : KeyValueDictionary
{
    public static ExternalTexts Load(string filePath) => new(filePath);
    private ExternalTexts(string filePath) : base(filePath) { }

    /// <summary>
    /// Attempts to get a poster name by its variant from the external texts.
    /// </summary>
    public bool TryGetPosterName(string variant, [NotNullWhen(true)] out string? name)
        => TryGetValue($"poster_{variant}_name", out name);

    /// <summary>
    /// Attempts to get a poster description by its variant from the external texts.
    /// </summary>
    public bool TryGetPosterDescription(string variant, [NotNullWhen(true)] out string? description)
        => TryGetValue($"poster_{variant}_desc", out description);

    /// <summary>
    /// Attempts to get a badge name by its code from the external texts.
    /// </summary>
    public bool TryGetBadgeName(string code, [NotNullWhen(true)] out string? name)
        => TryGetValue($"badge_name_{code}", out name) || TryGetValue($"{code}_badge_name", out name);

    /// <summary>
    /// Gets a badge name by its code from the external texts. Returns <c>null</c> if it is not found.
    /// </summary>
    public string? GetBadgeName(string code)
        => TryGetBadgeName(code, out string? name) ? name : null;

    /// <summary>
    /// Attempts to get a badge description by its code from the external texts.
    /// </summary>
    public bool TryGetBadgeDescription(string code, [NotNullWhen(true)] out string? description)
        => TryGetValue($"badge_desc_{code}", out description);

    /// <summary>
    /// Gets a badge description by its code from the external texts. Returns <c>null</c> if it is not found.
    /// </summary>
    public string? GetBadgeDescription(string code)
        => TryGetBadgeDescription(code, out string? description) ? description : null;

    /// <summary>
    /// Attempts to get an effect name by its ID from the external texts.
    /// </summary>
    public bool TryGetEffectName(int id, [NotNullWhen(true)] out string? name)
        => TryGetValue($"fx_{id}", out name);

    /// <summary>
    /// Gets an effect name by its ID from the external texts. Returns <c>null</c> if it is not found.
    /// </summary>
    public string? GetEffectName(int id)
        => TryGetEffectName(id, out string? name) ? name : null;

    /// <summary>
    /// Attempts to get an effect description by its ID from the external texts.
    /// </summary>
    public bool TryGetEffectDescription(int id, [NotNullWhen(true)] out string? description)
        => TryGetValue($"fx_{id}_desc", out description);

    /// <summary>
    /// Gets an effect description by its ID from the external texts. Returns <c>null</c> if it is not found.
    /// </summary>
    public string? GetEffectDescription(int id)
        => TryGetEffectDescription(id, out string? description) ? description : null;

    /// <summary>
    /// Attempts to get a hand item name by its ID from the external texts.
    /// </summary>
    public bool TryGetHandItemName(int id, [NotNullWhen(true)] out string? name)
        => TryGetValue($"handitem{id}", out name);

    /// <summary>
    /// Gets a hand item name by its ID from the external texts. Returns <c>null</c> if it is not found.
    /// </summary>
    public string? GetHandItemName(int id)
        => TryGetHandItemName(id, out string? name) ? name : null;

    /// <summary>
    /// Gets all hand item IDs matching the specified name from the external texts.
    /// </summary>
    public IEnumerable<int> GetHandItemIds(string name)
    {
        foreach (var (key, value) in this.Where(x => x.Value.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            if (key.StartsWith("handitem"))
                if (int.TryParse(key[8..], out int id))
                    yield return id;
        }
    }
}
