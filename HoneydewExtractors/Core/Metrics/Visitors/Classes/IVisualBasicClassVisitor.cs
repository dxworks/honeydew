using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.Classes;

public interface IVisualBasicClassVisitor : IClassVisitor,
    IExtractionVisitor<ClassStatementSyntax, SemanticModel, IMembersClassType>
{
}
