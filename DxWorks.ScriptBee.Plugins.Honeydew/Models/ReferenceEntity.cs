namespace DxWorks.ScriptBee.Plugins.Honeydew.Models;

public abstract class ReferenceEntity
{
    private readonly Dictionary<string, object?> _properties = new();

    public object? this[string propertyName]
    {
        get => _properties[propertyName];
        set => _properties[propertyName] = value;
    }

    public IReadOnlyDictionary<string, object?> GetProperties()
    {
        return _properties;
    }

    public bool HasProperty(string propertyName)
    {
        return _properties.ContainsKey(propertyName);
    }
}
