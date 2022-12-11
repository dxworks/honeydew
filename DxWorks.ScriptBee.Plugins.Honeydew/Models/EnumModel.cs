namespace DxWorks.ScriptBee.Plugins.Honeydew.Models;

public class EnumModel : EntityModel
{
    public string Type { get; set; }

    public IList<EnumLabelModel> Labels { get; set; } = new List<EnumLabelModel>();
}
