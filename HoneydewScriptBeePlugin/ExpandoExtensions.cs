using Westwind.Utilities;

namespace HoneydewScriptBeePlugin;

public static class ExpandoExtensions
{
    public static bool HasProperty(this Expando expando, string propertyName)
    {
        return expando.Properties.ContainsKey(propertyName);
    }
}
