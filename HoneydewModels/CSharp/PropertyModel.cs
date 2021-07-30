using System.Collections.Generic;

namespace HoneydewModels.CSharp
{
    public record PropertyModel
    {
        public string Name { get; init; }

        public string Type { get; set; }

        public string Modifier { get; init; } = "";

        public string AccessModifier { get; init; }

        public bool IsEvent { get; init; }

        public string ContainingClassName { get; set; }
        
        public IList<string> Accessors { get; set; } = new List<string>();
        
        public IList<MethodCallModel> CalledMethods { get; init; } = new List<MethodCallModel>();
    }
}
