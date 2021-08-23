using HoneydewExtractors.Core.Metrics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics
{
    public class CSharpSyntacticModel : ISyntacticModel
    {
        public SyntaxTree Tree { get; set; }
        public CompilationUnitSyntax CompilationUnitSyntax { get; set; }
    }
}
