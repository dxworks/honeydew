using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using static Honeydew.Extractors.VisualBasic.Visitors.Utils.VisualBasicExtractionHelperMethods;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class ReturnValueInfoVisitor :
        IExtractionVisitor<ReturnValueModel, SemanticModel, IReturnValueType>
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
}
