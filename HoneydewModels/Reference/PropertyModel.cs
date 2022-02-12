using System.Collections.Generic;

namespace HoneydewModels.Reference
{
    public class PropertyModel : FieldModel
    {
        public IList<MethodModel> Accessors { get; set; } = new List<MethodModel>();

        public LinesOfCode Loc { get; set; }

        public int CyclomaticComplexity { get; set; }
    }
}
