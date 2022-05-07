namespace Honeydew.Extractors.Visitors.Builder.Stages;

public interface IConstructorVisitorStage<TSemanticModel, TClassSyntaxNode, TConstructorSyntaxNode>
{
    IConstructorVisitorStage<TSemanticModel, TClassSyntaxNode, TConstructorSyntaxNode> AddConstructorVisitor<TType>(
        IExtractionVisitor<TConstructorSyntaxNode, TSemanticModel, TType> visitor);

}
