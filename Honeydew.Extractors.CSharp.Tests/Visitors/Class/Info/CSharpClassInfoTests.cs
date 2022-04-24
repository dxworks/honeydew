using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Class.Info;

public class CSharpClassInfoTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpClassInfoTests()
    {
        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterCompilationUnitVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new CSharpMethodSetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
                        {
                            new MethodInfoVisitor(),
                            new CSharpParameterSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IParameterType>>
                            {
                                new ParameterInfoVisitor()
                            }),
                            new CSharpReturnValueSetterVisitor(_loggerMock.Object,
                                new List<ITypeVisitor<IReturnValueType>>
                                {
                                    new ReturnValueInfoVisitor()
                                })
                        })
                    })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/InterfaceWithImplementedMethods.txt")]
    public void Extract_ShouldHaveMethods_WhenProvidedWithInterfaceWithImplementedMethods(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal("Namespace1.IInterface", classModel.Name);

        Assert.Equal(2, classModel.Methods.Count);
        Assert.Equal("Method1", classModel.Methods[0].Name);
        Assert.Equal("void", classModel.Methods[0].ReturnValue.Type.Name);
        Assert.Equal(1, classModel.Methods[0].ParameterTypes.Count);
        Assert.Equal("int", classModel.Methods[0].ParameterTypes[0].Type.Name);

        Assert.Equal("Method2", classModel.Methods[1].Name);
        Assert.Equal("int", classModel.Methods[1].ReturnValue.Type.Name);
        Assert.Equal(2, classModel.Methods[1].ParameterTypes.Count);
        Assert.Equal("string", classModel.Methods[1].ParameterTypes[0].Type.Name);
        Assert.Equal("string", classModel.Methods[1].ParameterTypes[1].Type.Name);
    }

    [Theory]
    [FileData(
        "TestData/PartialClass.txt")]
    public void Extract_ShouldHaveBaseObjectAndInterfaces_WhenClassExtendsOtherClassAndImplementsMultipleInterfaces(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal("partial", classTypes[0].Modifier);
        Assert.Equal("private", classTypes[0].AccessModifier);
        Assert.Equal("Namespace1.C1", classTypes[0].Name);
        Assert.Equal("Namespace1", classTypes[0].ContainingNamespaceName);
        Assert.Equal("", classTypes[0].ContainingClassName);

        Assert.Equal("partial", classTypes[1].Modifier);
        Assert.Equal("public", classTypes[1].AccessModifier);
        Assert.Equal("Namespace1.C1", classTypes[1].Name);
        Assert.Equal("Namespace1", classTypes[1].ContainingNamespaceName);
        Assert.Equal("", classTypes[1].ContainingClassName);
    }
}
