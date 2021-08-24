using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public interface IPropertyType : IFieldType, ICallingMethodsType, ITypeWithCyclomaticComplexity, ITypeWithLinesOfCode
    {
        public IList<string> Accessors { get; set; }
    }
}
