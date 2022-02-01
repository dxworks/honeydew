using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Destructors;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Destructor
{
    public class DestructorInfoVisitor : IRequireCSharpExtractionHelperMethodsVisitor,
        ICSharpDestructorVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

        public void Accept(IVisitor visitor)
        {
        }

        public IDestructorType Visit(DestructorDeclarationSyntax syntaxNode, IDestructorType modelType)
        {
            var containingClassName = "";
            if (syntaxNode.Parent is BaseTypeDeclarationSyntax baseTypeDeclarationSyntax)
            {
                containingClassName = CSharpHelperMethods.GetFullName(baseTypeDeclarationSyntax).Name;
            }

            modelType.Name = $"~{syntaxNode.Identifier.ToString()}";
            modelType.ContainingTypeName = containingClassName;
            modelType.Modifier = "";
            modelType.AccessModifier = "";
            modelType.CyclomaticComplexity = CSharpHelperMethods.CalculateCyclomaticComplexity(syntaxNode);

            return modelType;
        }
    }
}
