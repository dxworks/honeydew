using Honeydew.Models.Types;

namespace Honeydew.Extractors.Visitors.Extraction;

public interface IClassSetterCompilationUnitVisitor<in TSyntaxNode, in TSemanticModel, TClassSyntaxNode> :
    ISetterVisitor<TSyntaxNode, TSemanticModel, ICompilationUnitType, TClassSyntaxNode, IMembersClassType>
{
    string ISetterVisitor<TSyntaxNode, TSemanticModel, ICompilationUnitType, TClassSyntaxNode, IMembersClassType>.Name()
    {
        return "Class";
    }

    ICompilationUnitType IExtractionVisitor<TSyntaxNode, TSemanticModel, ICompilationUnitType>.Visit(
        TSyntaxNode syntaxNode, TSemanticModel semanticModel, ICompilationUnitType modelType)
    {
        foreach (var wrappedSyntaxNode in GetWrappedSyntaxNodes(syntaxNode))
        {
            var classType = ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            modelType.ClassTypes.Add(classType);
        }

        return modelType;
    }
}
