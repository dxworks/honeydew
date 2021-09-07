using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class
{
    public class BaseTypesClassVisitor : IRequireCSharpExtractionHelperMethodsVisitor,
        ICSharpClassVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

        public void Accept(IVisitor visitor)
        {
        }

        public IClassType Visit(BaseTypeDeclarationSyntax syntaxNode, IClassType modelType)
        {
            switch (syntaxNode)
            {
                case InterfaceDeclarationSyntax interfaceDeclarationSyntax:
                {
                    foreach (var baseInterface in CSharpHelperMethods.GetBaseInterfaces(interfaceDeclarationSyntax))
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
                        Type = CSharpHelperMethods.GetBaseClassName(syntaxNode),
                        Kind = "class"
                    });

                    foreach (var baseInterface in CSharpHelperMethods.GetBaseInterfaces(syntaxNode))
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
}
