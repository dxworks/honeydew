using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public class VisualBasicDestructorSetterVisitor :
    CompositeVisitor<IDestructorType>,
    IDestructorSetterClassVisitor<ClassBlockSyntax, SemanticModel, MethodBlockSyntax>
{
    public VisualBasicDestructorSetterVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<IDestructorType>> visitors) : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IDestructorType CreateWrappedType() => new VisualBasicDestructorModel();

    public IEnumerable<MethodBlockSyntax> GetWrappedSyntaxNodes(ClassBlockSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes()
            .OfType<MethodBlockSyntax>()
            .Where(m => m.SubOrFunctionStatement.Identifier.ToString() == "Finalize");
    }
}
