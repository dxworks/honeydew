using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Honeydew.Extractors.VisualBasic.Visitors.Utils.VisualBasicExtractionHelperMethods;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class ReturnValueInfoVisitor :
        IExtractionVisitor<TypeSyntax, SemanticModel, IReturnValueType>
    // IExtractionVisitor<AccessorReturnValue, SemanticModel, IReturnValueType>
{
    public IReturnValueType Visit(TypeSyntax syntaxNode, SemanticModel semanticModel, IReturnValueType modelType)
    {
        var returnType = GetFullName(syntaxNode, semanticModel, out var isNullable);
        // var returnTypeModifier = SetTypeModifier(syntaxNode.ToString(), "");

        modelType.Type = returnType;
        modelType.Modifier = "";
        modelType.IsNullable = isNullable;

        return modelType;
    }

    // public IReturnValueType Visit(AccessorReturnValue syntaxNode, SemanticModel semanticModel,
    //     IReturnValueType modelType)
    // {
    //     if (syntaxNode.Type != "get")
    //     {
    //         modelType.Type = new VisualBasicEntityTypeModel
    //         {
    //             Name = "void"
    //         };
    //         modelType.IsNullable = false;
    //         modelType.Modifier = "";
    //
    //         return modelType;
    //     }
    //
    //     var returnType = GetFullName(syntaxNode.ReturnType, semanticModel, out var isNullable);
    //     var returnTypeModifier = SetTypeModifier(syntaxNode.ToString(), "");
    //
    //     modelType.Type = returnType;
    //     modelType.Modifier = returnTypeModifier;
    //     modelType.IsNullable = isNullable;
    //
    //     return modelType;
    // }
}
