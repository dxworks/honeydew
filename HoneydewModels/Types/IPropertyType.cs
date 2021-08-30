using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public interface IPropertyType : IFieldType, ITypeWithCyclomaticComplexity, ITypeWithLinesOfCode
    {
        public IList<IMethodType> Accessors { get; set; }
    }
}
