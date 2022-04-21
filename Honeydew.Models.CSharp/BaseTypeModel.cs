using Honeydew.Models.Types;

namespace Honeydew.Models.CSharp;

public record BaseTypeModel : IBaseType
{
    public IEntityType Type { get; set; }

    public string Kind { get; set; }
}
