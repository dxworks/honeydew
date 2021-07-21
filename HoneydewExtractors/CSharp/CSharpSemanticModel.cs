using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.CSharp
{
    public class CSharpSemanticModel : ISemanticModel
    {
        public SemanticModel Model { get; set; }
    }
}
