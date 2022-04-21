using HoneydewModels.Types;

namespace HoneydewModels.CSharp;

public record LocalVariableModel : ILocalVariableType
{
    public IEntityType Type { get; set; }

    public string Modifier { get; set; } = "";

    public bool IsNullable { get; set; }

    public string Name { get; set; }
}
