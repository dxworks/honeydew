using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.ModelCreators
{
    public class
        CSharpPropertyModelCreator : AbstractModelCreator<BasePropertyDeclarationSyntax, IPropertyType,
            ICSharpPropertyVisitor>
    {
        public CSharpPropertyModelCreator(IEnumerable<ICSharpPropertyVisitor> visitors) : base(visitors)
        {
        }
    }
}
