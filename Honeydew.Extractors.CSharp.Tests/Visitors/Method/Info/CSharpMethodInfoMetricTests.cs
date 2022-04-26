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

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Method.Info;

public class CSharpMethodInfoMetricTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly DotnetSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpMethodInfoMetricTests()
    {
        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new CSharpMethodSetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
                        {
                            new MethodInfoVisitor(),
                            new CSharpCalledMethodSetterVisitor(_loggerMock.Object,
                                new List<ITypeVisitor<IMethodCallType>>
                                {
                                    new MethodCallInfoVisitor()
                                }),
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
    [FileData("TestData/ClassesWithNoMethods.txt")]
    public void Extract_ShouldHaveNoMethods_WhenGivenClassAndRecordsWithFieldsOnly(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(2, classTypes.Count);

        foreach (var classType in classTypes)
        {
            var classModel = (CSharpClassModel)classType;

            Assert.Empty(classModel.Methods);
        }
    }

    [Theory]
    [FileData("TestData/ClassHierarchyWithMethods.txt")]
    public void Extract_ShouldHaveMethods_WhenGivenAClassHierarchy(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(2, classTypes.Count);

        var classModel1 = (CSharpClassModel)classTypes[0];
        Assert.Equal(3, classModel1.Methods.Count);

        Assert.Equal("G", classModel1.Methods[0].Name);
        Assert.Equal("int", classModel1.Methods[0].ReturnValue.Type.Name);
        Assert.Equal(1, classModel1.Methods[0].ParameterTypes.Count);
        var parameterModel = (CSharpParameterModel)classModel1.Methods[0].ParameterTypes[0];
        Assert.Equal("float", parameterModel.Type.Name);
        Assert.Equal("", parameterModel.Modifier);
        Assert.Null(parameterModel.DefaultValue);
        Assert.Equal("protected", classModel1.Methods[0].AccessModifier);
        Assert.Equal("", classModel1.Methods[0].Modifier);
        Assert.Empty(classModel1.Methods[0].CalledMethods);

        Assert.Equal("H", classModel1.Methods[1].Name);
        Assert.Equal("bool", classModel1.Methods[1].ReturnValue.Type.Name);
        Assert.Empty(classModel1.Methods[1].ParameterTypes);
        Assert.Equal("public", classModel1.Methods[1].AccessModifier);
        Assert.Equal("virtual", classModel1.Methods[1].Modifier);
        Assert.Empty(classModel1.Methods[1].CalledMethods);

        Assert.Equal("X", classModel1.Methods[2].Name);
        Assert.Equal("int", classModel1.Methods[2].ReturnValue.Type.Name);
        Assert.Empty(classModel1.Methods[2].ParameterTypes);
        Assert.Equal("public", classModel1.Methods[2].AccessModifier);
        Assert.Equal("abstract", classModel1.Methods[2].Modifier);
        Assert.Empty(classModel1.Methods[2].CalledMethods);

        var classModel2 = (CSharpClassModel)classTypes[1];
        Assert.Equal(2, classModel2.Methods.Count);

        Assert.Equal("X", classModel2.Methods[0].Name);
        Assert.Equal("int", classModel2.Methods[0].ReturnValue.Type.Name);
        Assert.Empty(classModel2.Methods[0].ParameterTypes);
        Assert.Equal("public", classModel2.Methods[0].AccessModifier);
        Assert.Equal("override", classModel2.Methods[0].Modifier);
        Assert.Empty(classModel2.Methods[0].CalledMethods);

        Assert.Equal("H", classModel2.Methods[1].Name);
        Assert.Equal("bool", classModel2.Methods[1].ReturnValue.Type.Name);
        Assert.Empty(classModel1.Methods[1].ParameterTypes);
        Assert.Equal("public", classModel2.Methods[1].AccessModifier);
        Assert.Equal("override", classModel2.Methods[1].Modifier);
        Assert.Empty(classModel2.Methods[1].CalledMethods);
    }

    [Theory]
    [FileData("TestData/MethodWillNullDefaultValueParameter.txt")]
    public void Extract_ShouldExtractNullDefaultValue(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal("null", ((CSharpParameterModel)((CSharpClassModel)classTypes[0]).Methods[0].ParameterTypes[0]).DefaultValue);
    }
}
