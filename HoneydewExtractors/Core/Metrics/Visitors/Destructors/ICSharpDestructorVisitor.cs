using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.Destructors
{
    public interface ICSharpDestructorVisitor : IDestructorVisitor,
        IExtractionVisitor<DestructorDeclarationSyntax, IDestructorType>
    {
    }
}
