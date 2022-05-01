using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using static Honeydew.Extractors.VisualBasic.Visitors.Utils.VisualBasicExtractionHelperMethods;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class ReturnValueInfoVisitor :
        IExtractionVisitor<ReturnValueModel, SemanticModel, IReturnValueType>
    // IExtractionVisitor<ReturnValueModel, SemanticModel, IReturnValueType>
{
    public IReturnValueType Visit(ReturnValueModel returnValueModel, SemanticModel semanticModel,
        IReturnValueType modelType)
    {
        IEntityType returnType;
        var isNullable = false;
        if (returnValueModel.ReturnType is null)
        {
            returnType = new VisualBasicEntityTypeModel
            {
                Name = "Void",
                FullType = new GenericType
                {
                    Name = "Void"
                }
            };
        }
        else
        {
            returnType = GetFullName(returnValueModel.ReturnType, semanticModel, out isNullable);
        }
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
