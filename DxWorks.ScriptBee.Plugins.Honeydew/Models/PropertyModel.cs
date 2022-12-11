namespace DxWorks.ScriptBee.Plugins.Honeydew.Models;

public class PropertyModel : FieldModel
{
    public IList<MethodModel> Accessors { get; set; } = new List<MethodModel>();

    public LinesOfCode LinesOfCode { get; set; }

    public int CyclomaticComplexity { get; set; }
}
