namespace HoneydewModels.Types;

public interface IAttributeType : IMethodSignatureType
{
    public IEntityType Type { get; set; }

    public string TargetType { get; set; }
}
