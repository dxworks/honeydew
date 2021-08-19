using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.ModelCreators
{
    public class CSharpMethodCallModelCreator : AbstractModelCreator<InvocationExpressionSyntax, IMethodSignatureType,
        ICSharpMethodSignatureVisitor>
    {
        public CSharpMethodCallModelCreator(IEnumerable<ICSharpMethodSignatureVisitor> visitors) : base(visitors)
        {
        }
    }
}
