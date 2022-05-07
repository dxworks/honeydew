using Honeydew.Models.Types;

namespace Honeydew.Models.VisualBasic;

public record VisualBasicBaseTypeModel : IBaseType
{
    public IEntityType Type { get; set; }

    public string Kind { get; set; } = "";
}
