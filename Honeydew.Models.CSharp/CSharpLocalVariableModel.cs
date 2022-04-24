using Honeydew.Models.Types;

namespace Honeydew.Models.CSharp;

public record CSharpLocalVariableModel : ILocalVariableType
{
    public string Name { get; set; } = "";

    public IEntityType Type { get; set; }

    public string Modifier { get; set; } = "";

    public bool IsNullable { get; set; }
}
