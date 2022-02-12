using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.Fields;

public interface IVisualBasicFieldVisitor : IFieldVisitor,
    IExtractionVisitor<FieldDeclarationSyntax, SemanticModel, IFieldType>
{
}
