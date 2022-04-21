using Honeydew.Models.Types;

namespace Honeydew.Models.CSharp;

public record ReturnValueModel : IReturnValueType
{
    public IEntityType Type { get; set; }

    public string Modifier { get; set; } = "";

    public bool IsNullable { get; set; }

    public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();
}
