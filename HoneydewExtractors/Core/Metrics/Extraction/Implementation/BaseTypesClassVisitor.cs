﻿using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Implementation
{
    public class BaseTypesClassVisitor : ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>,
        ICSharpClassVisitor
    {
        public IMembersClassType Visit(BaseTypeDeclarationSyntax syntaxNode, IMembersClassType modelType)
        {
            switch (syntaxNode)
            {
                case InterfaceDeclarationSyntax interfaceDeclarationSyntax:
                {
                    foreach (var baseInterface in InheritedSemanticModel.GetBaseInterfaces(interfaceDeclarationSyntax))
                    {
                        modelType.BaseTypes.Add(new BaseTypeModel
                        {
                            Name = baseInterface,
                            ClassType = "interface"
                        });
                    }
                }
                    break;

                case ClassDeclarationSyntax classDeclarationSyntax:
                {
                    modelType.BaseTypes.Add(new BaseTypeModel
                    {
                        Name = InheritedSemanticModel.GetBaseClassName(classDeclarationSyntax),
                        ClassType = "class"
                    });

                    foreach (var baseInterface in InheritedSemanticModel.GetBaseInterfaces(classDeclarationSyntax))
                    {
                        modelType.BaseTypes.Add(new BaseTypeModel
                        {
                            Name = baseInterface,
                            ClassType = "interface"
                        });
                    }
                }
                    break;
            }

            return modelType;
        }
    }
}
