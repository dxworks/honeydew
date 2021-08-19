using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.ModelCreators
{
    public class CSharpMethodModelCreator : AbstractModelCreator<MethodDeclarationSyntax, IMethodType, ICSharpMethodVisitor>
    {
        public CSharpMethodModelCreator(IEnumerable<ICSharpMethodVisitor> visitors) : base(visitors)
        {
        }
    }
}
