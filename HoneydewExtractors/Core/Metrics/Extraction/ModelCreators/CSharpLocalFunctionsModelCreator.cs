using System.Collections.Generic;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.ModelCreators
{
    public class CSharpLocalFunctionsModelCreator : AbstractModelCreator<LocalFunctionStatementSyntax, IMethodTypeWithLocalFunctions,
        ICSharpLocalFunctionVisitor>
    {
        public CSharpLocalFunctionsModelCreator(IEnumerable<ICSharpLocalFunctionVisitor> visitors) : base(visitors)
        {
        }
    }
}
