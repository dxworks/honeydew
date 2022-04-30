using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Honeydew.Extractors.VisualBasic.Visitors.Utils.VisualBasicExtractionHelperMethods;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class BaseTypesClassVisitor :
    IExtractionVisitor<ClassBlockSyntax, SemanticModel, IMembersClassType>,
    IExtractionVisitor<InterfaceBlockSyntax, SemanticModel, IMembersClassType>,
    IExtractionVisitor<StructureBlockSyntax, SemanticModel, IMembersClassType>
{
    public IMembersClassType Visit(ClassBlockSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        foreach (var inheritsStatementSyntax in syntaxNode.Inherits)
        {
            foreach (var typeSyntax in inheritsStatementSyntax.Types)
            {
                modelType.BaseTypes.Add(new VisualBasicBaseTypeModel
                {
                    Type = GetFullName(typeSyntax, semanticModel),
                    Kind = "class"
                });
            }
        }

        foreach (var implementsStatementSyntax in syntaxNode.Implements)
        {
            foreach (var typeSyntax in implementsStatementSyntax.Types)
            {
                modelType.BaseTypes.Add(new VisualBasicBaseTypeModel
                {
                    Type = GetFullName(typeSyntax, semanticModel),
                    Kind = "interface"
                });
            }
        }

        if (modelType.BaseTypes.Count == 0)
        {
            modelType.BaseTypes.Add(new VisualBasicBaseTypeModel
            {
                Type = new VisualBasicEntityTypeModel
                {
                    Name = "Object",
                    FullType = new GenericType
                    {
                        Name = "Object"
                    }
                },
                Kind = "class"
            });
        }

        return modelType;
    }

    public IMembersClassType Visit(InterfaceBlockSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        foreach (var inheritsStatementSyntax in syntaxNode.Inherits)
        {
            foreach (var typeSyntax in inheritsStatementSyntax.Types)
            {
                modelType.BaseTypes.Add(new VisualBasicBaseTypeModel
                {
                    Type = GetFullName(typeSyntax, semanticModel),
                    Kind = "interface"
                });
            }
        }

        return modelType;
    }

    public IMembersClassType Visit(StructureBlockSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        foreach (var inheritsStatementSyntax in syntaxNode.Implements)
        {
            foreach (var typeSyntax in inheritsStatementSyntax.Types)
            {
                modelType.BaseTypes.Add(new VisualBasicBaseTypeModel
                {
                    Type = GetFullName(typeSyntax, semanticModel),
                    Kind = "interface"
                });
            }
        }

        return modelType;
    }
}
