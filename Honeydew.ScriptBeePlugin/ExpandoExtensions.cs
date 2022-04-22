using Westwind.Utilities;

namespace Honeydew.ScriptBeePlugin;

public static class ExpandoExtensions
{
    public static bool HasProperty(this Expando expando, string propertyName)
    {
        return expando.Properties.ContainsKey(propertyName);
    }
}
