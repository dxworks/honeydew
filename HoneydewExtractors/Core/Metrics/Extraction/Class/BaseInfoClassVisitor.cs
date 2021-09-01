using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class
{
    public class BaseInfoClassVisitor : IRequireCSharpExtractionHelperMethodsVisitor,
        ICSharpClassVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

        public void Accept(IVisitor visitor)
        {
        }

        public IClassType Visit(BaseTypeDeclarationSyntax syntaxNode, IClassType modelType)
        {
            var accessModifier = CSharpConstants.DefaultClassAccessModifier;
            var modifier = "";
            CSharpConstants.SetModifiers(syntaxNode.Modifiers.ToString(), ref accessModifier,
                ref modifier);

            modelType.Name = CSharpHelperMethods.GetFullName(syntaxNode);
            modelType.AccessModifier = accessModifier;
            modelType.Modifier = modifier;
            modelType.ClassType = syntaxNode.Kind().ToString().Replace("Declaration", "").ToLower();
            modelType.ContainingTypeName = CSharpHelperMethods.GetParentDeclaredType(syntaxNode);

            return modelType;
        }
    }
}
