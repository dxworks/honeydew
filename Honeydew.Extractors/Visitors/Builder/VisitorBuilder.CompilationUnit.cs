// using System;
// using Honeydew.Extractors.Visitors.Builder.Stages;
// using Honeydew.Extractors.Visitors.Setters;
//
// namespace Honeydew.Extractors.Visitors.Builder;
//
// // _compilationUnitVisitor.Add(visitor);
// // return this;
//
// public abstract partial class VisitorBuilder<TSemanticModel, TCompilationUnitSyntaxNode> :
//     ICompilationUnitVisitorStage<TSemanticModel, TCompilationUnitSyntaxNode>
// {
//     public ICompilationUnitVisitorStage<TSemanticModel, TCompilationUnitSyntaxNode> AddCompilationUnitVisitor<TType>(
//         IExtractionVisitor<TCompilationUnitSyntaxNode, TSemanticModel, TType> visitor)
//     {
//         throw new NotImplementedException();
//     }
//
//     public IClassVisitorStage<TSemanticModel, TCompilationUnitSyntaxNode, TClassSyntaxNode> AddClassSetterVisitor<
//         TClassSyntaxNode>(
//         IClassSetterCompilationUnitVisitor<TCompilationUnitSyntaxNode, TSemanticModel, TClassSyntaxNode> visitor)
//     {
//         throw new NotImplementedException();
//     }
//
//     public IDelegateVisitorStage AddDelegateSetterVisitor<TDelegateSyntaxNode>(
//         IDelegateSetterCompilationUnitVisitor<TCompilationUnitSyntaxNode, TSemanticModel, TDelegateSyntaxNode> visitor)
//     {
//         throw new NotImplementedException();
//     }
//
//     public IEnumVisitorStage AddEnumSetterVisitor<TEnumSyntaxNode>(
//         IEnumSetterCompilationUnitVisitor<TCompilationUnitSyntaxNode, TSemanticModel, TEnumSyntaxNode> visitor)
//     {
//         throw new NotImplementedException();
//     }
// }
