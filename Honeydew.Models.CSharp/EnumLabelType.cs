using Honeydew.Models.Types;

namespace Honeydew.Models.CSharp;

public record EnumLabelType : IEnumLabelType
{
    public string Name { get; set; }

    public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();
}
