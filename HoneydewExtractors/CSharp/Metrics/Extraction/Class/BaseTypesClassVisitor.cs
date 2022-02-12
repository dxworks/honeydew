using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class;

public class BaseTypesClassVisitor : ICSharpClassVisitor
{
    public void Accept(IVisitor visitor)
    {
    }

    public IClassType Visit(BaseTypeDeclarationSyntax syntaxNode, SemanticModel semanticModel, IClassType modelType)
    {
        switch (syntaxNode)
        {
            case InterfaceDeclarationSyntax interfaceDeclarationSyntax:
            {
                foreach (var baseInterface in CSharpExtractionHelperMethods.GetBaseInterfaces(
                             interfaceDeclarationSyntax, semanticModel))
                {
                    modelType.BaseTypes.Add(new BaseTypeModel
                    {
                        Type = baseInterface,
                        Kind = "interface"
                    });
                }
            }
                break;

            case EnumDeclarationSyntax:
            {
                modelType.BaseTypes.Add(new BaseTypeModel
                {
                    Type = new EntityTypeModel
                    {
                        Name = "System.Enum",
                        FullType = new GenericType
                        {
                            Name = "System.Enum"
                        }
                    },
                    Kind = "class"
                });
            }
                break;
            default:
            {
                modelType.BaseTypes.Add(new BaseTypeModel
                {
                    Type = CSharpExtractionHelperMethods.GetBaseClassName(syntaxNode, semanticModel),
                    Kind = "class"
                });

                foreach (var baseInterface in CSharpExtractionHelperMethods.GetBaseInterfaces(syntaxNode, semanticModel))
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
