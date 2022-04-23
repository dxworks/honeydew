using Honeydew.Extractors.CSharp.Visitors.Utils;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class AccessFieldVisitor : IExtractionVisitor<ExpressionSyntax, SemanticModel, AccessedField>
{
    public AccessedField Visit(ExpressionSyntax syntaxNode, SemanticModel semanticModel, AccessedField modelType)
    {
        return CSharpExtractionHelperMethods.GetAccessField(syntaxNode, semanticModel);
    }
}
