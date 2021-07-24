using HoneydewExtractors.Core;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.CSharp.Metrics
{
    public class
        CSharpFactExtractor : FactExtractor<ClassModel, CSharpSyntacticModel, CSharpSemanticModel, CSharpSyntaxNode>
    {
        public CSharpFactExtractor() : base(new CSharpSyntacticModelCreator(),
            new CSharpSemanticModelCreator(), new CSharpClassModelExtractor())
        {
        }
    }
}
