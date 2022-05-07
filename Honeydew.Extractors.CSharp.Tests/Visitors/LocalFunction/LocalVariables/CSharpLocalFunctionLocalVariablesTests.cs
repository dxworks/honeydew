using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.LocalFunction.LocalVariables;

public class CSharpLocalFunctionLocalVariablesTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly DotnetSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpLocalFunctionLocalVariablesTests()
    {
        var localVariablesTypeSetterVisitor = new CSharpLocalVariablesTypeSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ILocalVariableType>>
            {
                new LocalVariableInfoVisitor()
            });

        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new CSharpMethodSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
                        {
                            new MethodInfoVisitor(),
                            new CSharpLocalFunctionsSetterVisitor(_loggerMock.Object,
                                new List<ITypeVisitor<IMethodTypeWithLocalFunctions>>
                                {
                                    new LocalFunctionInfoVisitor(_loggerMock.Object,
                                        new List<ITypeVisitor<IMethodTypeWithLocalFunctions>>
                                        {
                                            localVariablesTypeSetterVisitor,
                                        }),
                                    localVariablesTypeSetterVisitor,
                                }),
                            localVariablesTypeSetterVisitor
                        }),
                    })
            });


        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/LocalFunctionWithLocalVariables.txt")]
    public void Extract_ShouldExtractLocalVariables_WhenProvidedWithLocalFunctionWithLocalVariables(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];
        var method = (CSharpMethodModel)classModel.Methods[0];
        Assert.Equal(1, method.LocalFunctions.Count);

        foreach (var localFunction in method.LocalFunctions)
        {
            Assert.Equal(3, localFunction.LocalVariableTypes.Count);
            Assert.Equal("int", localFunction.LocalVariableTypes[0].Type.Name);
            Assert.Equal("Namespace1.Class2", localFunction.LocalVariableTypes[1].Type.Name);
            Assert.Equal("ExternClass", localFunction.LocalVariableTypes[2].Type.Name);

            Assert.Equal(1, localFunction.LocalFunctions.Count);
            foreach (var innerLocalFunction in localFunction.LocalFunctions)
            {
                Assert.Equal(3, innerLocalFunction.LocalVariableTypes.Count);
                Assert.Equal("int", innerLocalFunction.LocalVariableTypes[0].Type.Name);
                Assert.Equal("Namespace1.Class2", innerLocalFunction.LocalVariableTypes[1].Type.Name);
                Assert.Equal("ExternClass", innerLocalFunction.LocalVariableTypes[2].Type.Name);
            }
        }
    }

    [Theory]
    [FileData("TestData/LocalFunctionWithArrayLocalVariables.txt")]
    public void Extract_ShouldExtractLocalVariables_WhenProvidedWithLocalFunctionWithArrayLocalVariables(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];
        var method = (CSharpMethodModel)classModel.Methods[0];
        Assert.Equal(1, method.LocalFunctions.Count);

        foreach (var localFunction in method.LocalFunctions)
        {
            Assert.Equal(3, localFunction.LocalVariableTypes.Count);
            Assert.Equal("int[]", localFunction.LocalVariableTypes[0].Type.Name);
            Assert.Equal("Namespace1.Class2[]", localFunction.LocalVariableTypes[1].Type.Name);
            Assert.Equal("ExternClass[]", localFunction.LocalVariableTypes[2].Type.Name);

            Assert.Equal(1, localFunction.LocalFunctions.Count);
            foreach (var innerLocalFunction in localFunction.LocalFunctions)
            {
                Assert.Equal(3, innerLocalFunction.LocalVariableTypes.Count);
                Assert.Equal("int[]", innerLocalFunction.LocalVariableTypes[0].Type.Name);
                Assert.Equal("Namespace1.Class2[]", innerLocalFunction.LocalVariableTypes[1].Type.Name);
                Assert.Equal("ExternClass[]", innerLocalFunction.LocalVariableTypes[2].Type.Name);
            }
        }
    }


    [Theory]
    [FileData(
        "TestData/LocalFunctionWithLocalVariableFromIfAndSwitch.txt")]
    public void Extract_ShouldExtractLocalVariables_WhenProvidedWithLocalFunctionsWithLocalVariablesFromIfAndSwitch(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];
        var method = (CSharpMethodModel)classModel.Methods[0];
        Assert.Equal(1, method.LocalFunctions.Count);

        foreach (var localFunction in method.LocalFunctions)
        {
            Assert.Equal(2, localFunction.LocalVariableTypes.Count);
            Assert.Equal("Namespace1.Class2", localFunction.LocalVariableTypes[0].Type.Name);
            Assert.Equal("Namespace1.Class3", localFunction.LocalVariableTypes[1].Type.Name);

            Assert.Equal(1, localFunction.LocalFunctions.Count);
            foreach (var innerLocalFunction in localFunction.LocalFunctions)
            {
                Assert.Equal(2, innerLocalFunction.LocalVariableTypes.Count);
                Assert.Equal("Namespace1.Class2", innerLocalFunction.LocalVariableTypes[0].Type.Name);
                Assert.Equal("Namespace1.Class3", innerLocalFunction.LocalVariableTypes[1].Type.Name);
            }
        }
    }


    [Theory]
    [FileData("TestData/LocalFunctionWithLocalVariableFromForeach.txt")]
    public void Extract_ShouldExtractLocalVariables_WhenProvidedWithLocalFunctionsWithLocalVariablesFromForeach(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        foreach (var methodType in classModel.Methods)
        {
            var method = (CSharpMethodModel)methodType;
            Assert.Equal(1, method.LocalFunctions.Count);

            foreach (var localFunction in method.LocalFunctions)
            {
                Assert.Equal(2, localFunction.LocalVariableTypes.Count);

                Assert.Equal(1, localFunction.LocalFunctions.Count);
                foreach (var innerLocalFunction in localFunction.LocalFunctions)
                {
                    Assert.Equal(2, innerLocalFunction.LocalVariableTypes.Count);
                }
            }
        }


        var method0 = (CSharpMethodModel)classModel.Methods[0];
        Assert.Equal("Namespace1.Class2", method0.LocalFunctions[0].LocalVariableTypes[0].Type.Name);
        Assert.Equal("Namespace1.Class2", method0.LocalFunctions[0].LocalVariableTypes[1].Type.Name);
        Assert.Equal("Namespace1.Class2",
            method0.LocalFunctions[0].LocalFunctions[0].LocalVariableTypes[0].Type.Name);
        Assert.Equal("Namespace1.Class2",
            method0.LocalFunctions[0].LocalFunctions[0].LocalVariableTypes[1].Type.Name);

        var method1 = (CSharpMethodModel)classModel.Methods[1];
        Assert.Equal("int", method1.LocalFunctions[0].LocalVariableTypes[0].Type.Name);
        Assert.Equal("int", method1.LocalFunctions[0].LocalVariableTypes[1].Type.Name);
        Assert.Equal("int", method1.LocalFunctions[0].LocalFunctions[0].LocalVariableTypes[0].Type.Name);
        Assert.Equal("int", method1.LocalFunctions[0].LocalFunctions[0].LocalVariableTypes[1].Type.Name);

        var method2 = (CSharpMethodModel)classModel.Methods[2];
        Assert.Equal("ExternClass", method2.LocalFunctions[0].LocalVariableTypes[0].Type.Name);
        Assert.Equal("ExternClass", method2.LocalFunctions[0].LocalVariableTypes[1].Type.Name);
        Assert.Equal("ExternClass", method2.LocalFunctions[0].LocalFunctions[0].LocalVariableTypes[0].Type.Name);
        Assert.Equal("ExternClass", method2.LocalFunctions[0].LocalFunctions[0].LocalVariableTypes[1].Type.Name);
    }
}
