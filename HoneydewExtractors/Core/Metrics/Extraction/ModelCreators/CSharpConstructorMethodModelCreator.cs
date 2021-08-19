using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.ModelCreators
{
    public class CSharpConstructorMethodModelCreator : AbstractModelCreator<ConstructorDeclarationSyntax,
        IConstructorType, ICSharpConstructorVisitor>
    {
        public CSharpConstructorMethodModelCreator(IEnumerable<ICSharpConstructorVisitor> visitors) : base(visitors)
        {
        }
    }
}
