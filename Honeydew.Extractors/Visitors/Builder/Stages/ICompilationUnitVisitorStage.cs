using Honeydew.Extractors.Visitors.Extraction;

namespace Honeydew.Extractors.Visitors.Builder.Stages;

public interface ICompilationUnitVisitorStage<TSemanticModel, TCompilationUnitSyntaxNode> 
{
    public ICompilationUnitVisitorStage<TSemanticModel, TCompilationUnitSyntaxNode>  AddCompilationUnitVisitor<TType>(
        IExtractionVisitor<TCompilationUnitSyntaxNode, TSemanticModel, TType> visitor);

    public IClassVisitorStage<TSemanticModel, TCompilationUnitSyntaxNode, TClassSyntaxNode>  AddClassSetterVisitor<TClassSyntaxNode>(
        IClassSetterCompilationUnitVisitor<TCompilationUnitSyntaxNode, TSemanticModel, TClassSyntaxNode> visitor);

    public IDelegateVisitorStage AddDelegateSetterVisitor<TDelegateSyntaxNode>(
        IDelegateSetterCompilationUnitVisitor<TCompilationUnitSyntaxNode, TSemanticModel, TDelegateSyntaxNode> visitor);

    public IEnumVisitorStage AddEnumSetterVisitor<TEnumSyntaxNode>(
        IEnumSetterCompilationUnitVisitor<TCompilationUnitSyntaxNode, TSemanticModel, TEnumSyntaxNode> visitor);
}
