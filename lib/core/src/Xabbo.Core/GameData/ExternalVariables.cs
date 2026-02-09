namespace Xabbo.Core.GameData;

/// <summary>
/// Defines a dictionary of external variables.
/// </summary>
public sealed class ExternalVariables : KeyValueDictionary
{
    public static ExternalVariables Load(string filePath) => new(filePath);
    private ExternalVariables(string filePath) : base(filePath) { }
}
