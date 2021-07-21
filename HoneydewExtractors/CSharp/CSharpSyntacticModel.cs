using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp
{
    public class CSharpSyntacticModel : ISyntacticModel
    {
        public SyntaxTree Tree { get; set; }
        public CompilationUnitSyntax CompilationUnitSyntax { get; set; }
    }
}
