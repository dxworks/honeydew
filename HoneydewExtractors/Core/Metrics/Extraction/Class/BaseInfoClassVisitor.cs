using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class
{
    public class BaseInfoClassVisitor : ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>,
        ICSharpClassVisitor
    {
        public IClassType Visit(BaseTypeDeclarationSyntax syntaxNode, IClassType modelType)
        {
            var accessModifier = CSharpConstants.DefaultClassAccessModifier;
            var modifier = "";
            CSharpConstants.SetModifiers(syntaxNode.Modifiers.ToString(), ref accessModifier,
                ref modifier);

            modelType.Name = InheritedSemanticModel.GetFullName(syntaxNode);
            modelType.AccessModifier = accessModifier;
            modelType.Modifier = modifier;
            modelType.ClassType = syntaxNode.Kind().ToString().Replace("Declaration", "").ToLower();

            return modelType;
        }
    }
}
