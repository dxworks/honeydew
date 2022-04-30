using Honeydew.Models.Types;

namespace Honeydew.Models.VisualBasic;

public record VisualBasicEntityTypeModel : IEntityType
{
    public string Name { get; set; } = "";

    public GenericType FullType { get; set; }

    public bool IsExtern { get; set; }
}
