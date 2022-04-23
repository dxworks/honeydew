using Honeydew.Extractors.Visitors.Setters;

namespace Honeydew.Extractors.Visitors.Builder.Stages;

public interface IClassVisitorStage<TSemanticModel, TCompilationUnitSyntaxNode, TClassSyntaxNode>
{
    IClassVisitorStage<TSemanticModel, TCompilationUnitSyntaxNode, TClassSyntaxNode> AddClassVisitor<TType>(
        IExtractionVisitor<TClassSyntaxNode, TSemanticModel, TType> visitor);

    IConstructorVisitorStage<TSemanticModel, TClassSyntaxNode, TConstructorSyntaxNode>
        AddConstructorSetterVisitor<TConstructorSyntaxNode>(
            IConstructorSetterClassVisitor<TClassSyntaxNode, TSemanticModel, TConstructorSyntaxNode> setterVisitor);
}
