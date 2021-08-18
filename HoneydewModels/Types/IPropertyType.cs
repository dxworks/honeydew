using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public interface IPropertyType : IFieldType, ICallingMethodsType, ICyclomaticComplexityType
    {
        public IList<string> Accessors { get; set; }

        public LinesOfCode Loc { get; set; }
    }
}
