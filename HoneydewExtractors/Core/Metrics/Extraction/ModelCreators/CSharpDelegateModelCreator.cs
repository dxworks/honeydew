using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.ModelCreators
{
    public class CSharpDelegateModelCreator : AbstractModelCreator<DelegateDeclarationSyntax, IDelegateType,
        ICSharpDelegateVisitor>
    {
        public CSharpDelegateModelCreator(IEnumerable<ICSharpDelegateVisitor> visitors) : base(visitors)
        {
        }
    }
}
