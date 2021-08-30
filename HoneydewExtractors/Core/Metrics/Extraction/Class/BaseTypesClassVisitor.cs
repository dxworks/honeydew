using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class
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

                case ClassDeclarationSyntax classDeclarationSyntax:
                {
                    modelType.BaseTypes.Add(new BaseTypeModel
                    {
                        Type = CSharpHelperMethods.GetBaseClassName(classDeclarationSyntax),
                        Kind = "class"
                    });

                    foreach (var baseInterface in CSharpHelperMethods.GetBaseInterfaces(classDeclarationSyntax))
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
