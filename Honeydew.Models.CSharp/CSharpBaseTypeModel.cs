using Honeydew.Models.Types;

namespace Honeydew.Models.CSharp;

public record CSharpBaseTypeModel : IBaseType
{
    public IEntityType Type { get; set; }

    public string Kind { get; set; } = "";
}
