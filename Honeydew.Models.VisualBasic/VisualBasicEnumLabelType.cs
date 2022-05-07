using Honeydew.Models.Types;

namespace Honeydew.Models.VisualBasic;

public record VisualBasicEnumLabelType : IEnumLabelType
{
    public string Name { get; set; } = "";

    public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();
}
