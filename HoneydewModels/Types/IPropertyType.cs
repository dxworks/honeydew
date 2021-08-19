using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public interface IPropertyType : IFieldType, ICallingMethodsType, ICyclomaticComplexityType, ITypeWithLinesOfCode
    {
        public IList<string> Accessors { get; set; }
    }
}
