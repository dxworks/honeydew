using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;

using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Honeydew.Extractors.VisualBasic.Visitors.Utils.VisualBasicExtractionHelperMethods;
using static Honeydew.Models.VisualBasic.VisualBasicConstants;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class MethodInfoVisitor :
    IExtractionVisitor<MethodStatementSyntax, SemanticModel, IMethodType>,
    IExtractionVisitor<AccessorBlockSyntax, SemanticModel, IAccessorMethodType>
{
    public IMethodType Visit(MethodStatementSyntax syntaxNode, SemanticModel semanticModel, IMethodType modelType)
    {

        var isInterface = syntaxNode.GetParentDeclarationSyntax<InterfaceBlockSyntax>() != null;
        var accessModifier = isInterface
            ? DefaultInterfaceMethodAccessModifier
            : DefaultClassMethodAccessModifier;
        var modifier = isInterface
            ? DefaultInterfaceMethodModifier
            : syntaxNode.Modifiers.ToString();
        
        SetModifiers(syntaxNode.Modifiers.ToString(), ref accessModifier, ref modifier);
        
        modelType.Name = syntaxNode.Identifier.ToString();

        modelType.Modifier = modifier;
        modelType.AccessModifier = accessModifier;
        modelType.CyclomaticComplexity = CalculateCyclomaticComplexity(syntaxNode);

        return modelType;
    }

    public IAccessorMethodType Visit(AccessorBlockSyntax syntaxNode, SemanticModel semanticModel,
        IAccessorMethodType modelMethodType)
    {
        var accessModifier = "Public";
        var modifier = syntaxNode.AccessorStatement.Modifiers.ToString();
    
        SetModifiers(syntaxNode.AccessorStatement.Modifiers.ToString(), ref accessModifier, ref modifier);
    
        modelMethodType.Name = syntaxNode.AccessorStatement.AccessorKeyword.ToString();
    
        modelMethodType.Modifier = modifier;
        modelMethodType.AccessModifier = accessModifier;
        modelMethodType.CyclomaticComplexity = CalculateCyclomaticComplexity(syntaxNode);
    
        return modelMethodType;
    }
}
