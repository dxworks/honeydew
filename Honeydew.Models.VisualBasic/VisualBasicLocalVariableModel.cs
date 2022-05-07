using Honeydew.Models.Types;

namespace Honeydew.Models.VisualBasic;

public record VisualBasicLocalVariableModel : ILocalVariableType
{
    public string Name { get; set; } = "";

    public IEntityType Type { get; set; }

    public string Modifier { get; set; } = "";

    public bool IsNullable { get; set; }
}
