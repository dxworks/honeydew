using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.ModelCreators
{
    public class
        CSharpClassModelCreator : AbstractModelCreator<BaseTypeDeclarationSyntax, IMembersClassType, ICSharpClassVisitor>
    {
        public CSharpClassModelCreator(IEnumerable<ICSharpClassVisitor> visitors) : base(visitors)
        {
        }
    }
}
