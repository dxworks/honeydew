using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Visitors.CompilationUnit;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.ModelCreators
{
    public class CSharpCompilationUnitModelCreator : AbstractModelCreator<CompilationUnitSyntax, ICompilationUnitType,
        ICSharpCompilationUnitVisitor>
    {
        public CSharpCompilationUnitModelCreator(IEnumerable<ICSharpCompilationUnitVisitor> visitors) : base(visitors)
        {
        }
    }
}
