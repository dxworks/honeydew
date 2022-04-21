using System.Collections.Generic;

namespace Honeydew.Models.Types;

public interface IPropertyType : IFieldType, ITypeWithCyclomaticComplexity, ITypeWithLinesOfCode
{
    public IList<IAccessorType> Accessors { get; set; }
}
