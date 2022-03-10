namespace HoneydewScriptBeePlugin.Models;

public class EnumModel : EntityModel
{
    public string Type { get; set; }

    public IDictionary<string, int> Labels { get; set; } = new Dictionary<string, int>();
}
