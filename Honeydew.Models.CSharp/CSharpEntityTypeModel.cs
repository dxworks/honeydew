using Honeydew.Models.Types;

namespace Honeydew.Models.CSharp;

public record CSharpEntityTypeModel : IEntityType
{
    public string Name { get; set; } = "";

    public GenericType FullType { get; set; }

    public bool IsExtern { get; set; }
}
