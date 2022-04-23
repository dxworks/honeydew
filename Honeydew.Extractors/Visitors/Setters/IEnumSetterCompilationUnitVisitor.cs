using Honeydew.Models.Types;

namespace Honeydew.Extractors.Visitors.Setters;

public interface IEnumSetterCompilationUnitVisitor<in TSyntaxNode, TSemanticModel, TClassSyntaxNode> :
    ISetterVisitor<TSyntaxNode, TSemanticModel, ICompilationUnitType, TClassSyntaxNode, IEnumType>
{
    string ISetterVisitor<TSyntaxNode, TSemanticModel, ICompilationUnitType, TClassSyntaxNode, IEnumType>.Name()
    {
        return "Enum";
    }

    ICompilationUnitType IExtractionVisitor<TSyntaxNode, TSemanticModel, ICompilationUnitType>.Visit(
        TSyntaxNode syntaxNode, TSemanticModel semanticModel, ICompilationUnitType modelType)
    {
        foreach (var wrappedSyntaxNode in GetWrappedSyntaxNodes(syntaxNode))
        {
            var enumType = ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            modelType.ClassTypes.Add(enumType);
        }

        return modelType;
    }
}
