using Honeydew.Models.Types;

namespace Honeydew.Models.CSharp;

public record AttributeModel : IAttributeType
{
    public string Name { get; set; }

    public IEntityType Type { get; set; }
    
    public IList<IParameterType> ParameterTypes { get; set; } = new List<IParameterType>();

    public string Target { get; set; }
}
