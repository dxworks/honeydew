namespace DxWorks.ScriptBee.Plugins.Honeydew.Models;

public class FieldModel : MemberModel
{
    public EntityType Type { get; set; }

    public bool IsEvent { get; set; }

    public IList<FieldAccess> Accesses { get; set; } = new List<FieldAccess>();

    public bool IsNullable => Type is { IsNullable: true };

    public IDictionary<string, int> Metrics { get; init; } = new Dictionary<string, int>();
}
