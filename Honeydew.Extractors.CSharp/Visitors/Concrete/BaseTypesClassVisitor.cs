using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Honeydew.Extractors.CSharp.Visitors.Utils.CSharpExtractionHelperMethods;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class BaseTypesClassVisitor : IExtractionVisitor<TypeDeclarationSyntax, SemanticModel, IMembersClassType>
{
    public IMembersClassType Visit(TypeDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        switch (syntaxNode)
        {
            case InterfaceDeclarationSyntax interfaceDeclarationSyntax:
            {
                foreach (var baseInterface in GetBaseInterfaces(interfaceDeclarationSyntax, semanticModel))
                {
                    modelType.BaseTypes.Add(new BaseTypeModel
                    {
                        Type = baseInterface,
                        Kind = "interface"
                    });
                }
            }
                break;

            default:
            {
                modelType.BaseTypes.Add(new BaseTypeModel
                {
                    Type = GetBaseClassName(syntaxNode, semanticModel),
                    Kind = "class"
                });

                foreach (var baseInterface in GetBaseInterfaces(syntaxNode, semanticModel))
                {
                    modelType.BaseTypes.Add(new BaseTypeModel
                    {
                        Type = baseInterface,
                        Kind = "interface"
                    });
                }
            }
                break;
        }

        return modelType;
    }
}
