using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.AccessedFields;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.AccessField;

public class AccessFieldVisitor : ICSharpAccessedFieldsVisitor
{
    public void Accept(IVisitor visitor)
    {
    }

    public AccessedField Visit(ExpressionSyntax syntaxNode, SemanticModel semanticModel, AccessedField modelType)
    {
        return CSharpExtractionHelperMethods.GetAccessField(syntaxNode, semanticModel);
    }
}
