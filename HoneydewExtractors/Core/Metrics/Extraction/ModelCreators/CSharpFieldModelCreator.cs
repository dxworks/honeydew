using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Visitors.Fields;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.ModelCreators
{
    public class
        CSharpFieldModelCreator : AbstractModelCreator<BaseFieldDeclarationSyntax, IFieldType, ICSharpFieldVisitor>
    {
        public CSharpFieldModelCreator(IEnumerable<ICSharpFieldVisitor> visitors) : base(visitors)
        {
        }
    }
}
