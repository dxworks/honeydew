using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Metrics.CSharp
{
    public class CSharpSyntacticModel : ISyntacticModel
    {
        public SyntaxTree Tree { get; set; }
        public CompilationUnitSyntax CompilationUnitSyntax { get; set; }
    }
}
