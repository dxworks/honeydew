namespace Honeydew.Models.Types;

public interface IPropertyType : IFieldType, ITypeWithCyclomaticComplexity, ITypeWithLinesOfCode
{
    public IList<IAccessorMethodType> Accessors { get; set; }
}
