using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Class.BaseType;

public class CSharpBaseClassMetricTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpBaseClassMetricTests()
    {
        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterCompilationUnitVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new BaseTypesClassVisitor()
                    })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [InlineData("class", "object")]
    [InlineData("struct", "System.ValueType")]
    public void Extract_ShouldHaveBaseClassObject_WhenClassDoesNotExtendsAnyClass(string classType, string baseType)
    {
        var fileContent = $@"using System;

                                    namespace App
                                    {{                                       

                                        {classType} MyClass
                                        {{                                           
                                            public void Foo() {{ }}
                                        }}
                                    }}";

        var syntacticModelCreator = new CSharpSyntacticModelCreator();
        var semanticModelCreator = new CSharpSemanticModelCreator(new CSharpCompilationMaker());
        var syntaxTree = syntacticModelCreator.Create(fileContent);
        var semanticModel = semanticModelCreator.Create(syntaxTree);

        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var classModel = classTypes[0];

        Assert.Equal("App.MyClass", classModel.Name);
        Assert.Equal(1, classModel.BaseTypes.Count);

        Assert.Equal(baseType, classModel.BaseTypes[0].Type.Name);
        Assert.Equal("class", classModel.BaseTypes[0].Kind);
    }

    [Theory]
    [FileData("TestData/SimpleRecord.txt")]
    [FileData("TestData/SimpleRecord2.txt")]
    public void Extract_ShouldHaveBaseClass_WhenProvidedWithRecord(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var classModel = classTypes[0];

        Assert.Equal(2, classModel.BaseTypes.Count);

        Assert.Equal("App.MyClass", classModel.Name);
        Assert.Equal("object", classModel.BaseTypes[0].Type.Name);
        Assert.Equal("class", classModel.BaseTypes[0].Kind);

        Assert.Equal("System.IEquatable<App.MyClass>", classModel.BaseTypes[1].Type.Name);
        Assert.Equal("interface", classModel.BaseTypes[1].Kind);
    }

    [Theory]
    [FileData("TestData/SimpleInterface.txt")]
    public void Extract_ShouldHaveNoBaseClass_WhenProvidedWithInterface(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var classModel = classTypes[0];

        Assert.Equal("App.MyClass", classModel.Name);
        Assert.Equal("App.MyClass", classModel.Name);
        Assert.Empty(classModel.BaseTypes);
    }

    [Theory]
    [FileData("TestData/ClassThatExtendsExternClasses.txt")]
    public void Extract_ShouldHaveBaseClassIMetric_WhenClassExtendsIMetricInterface(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var classType = classTypes[0];

        Assert.Equal("App.Domain.MyClass", classType.Name);

        var baseTypes = classType.BaseTypes;

        Assert.Equal(2, baseTypes.Count);
        Assert.Equal("IMetric", baseTypes[0].Type.Name);
        Assert.Equal("class", baseTypes[0].Kind);

        Assert.Equal("IMetric2", baseTypes[1].Type.Name);
        Assert.Equal("interface", baseTypes[1].Kind);
    }

    [Theory]
    [FileData("TestData/ClassThatExtendsOtherClass.txt")]
    public void Extract_ShouldHaveBaseObjectAndNoInterfaces_WhenClassOnlyExtendsOtherClass(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(2, classTypes.Count);

        var baseClassType = classTypes[0];
        var baseClassBaseTypes = baseClassType.BaseTypes;

        Assert.Equal("App.Parent", baseClassType.Name);
        Assert.Equal(1, baseClassBaseTypes.Count);
        Assert.Equal("object", baseClassBaseTypes[0].Type.Name);
        Assert.Equal("class", baseClassBaseTypes[0].Kind);

        var classType = classTypes[1];
        var baseTypes = classType.BaseTypes;

        Assert.Equal("App.ChildClass", classType.Name);
        Assert.Equal(1, baseTypes.Count);
        Assert.Equal("App.Parent", baseTypes[0].Type.Name);
        Assert.Equal("class", baseTypes[0].Kind);
    }

    [Theory]
    [FileData("TestData/ClassThatExtendsOtherClassAndExternInterfaces.txt")]
    public void Extract_ShouldHaveBaseObjectAndInterfaces_WhenClassExtendsOtherClassAndImplementsMultipleInterfaces(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(2, classTypes.Count);

        var baseClassType = classTypes[0];
        var baseClassBaseTypes = baseClassType.BaseTypes;

        Assert.Equal("App.Parent", baseClassType.Name);
        Assert.Equal(1, baseClassBaseTypes.Count);
        Assert.Equal("object", baseClassBaseTypes[0].Type.Name);
        Assert.Equal("class", baseClassBaseTypes[0].Kind);

        var classType = classTypes[1];
        var baseTypes = classType.BaseTypes;

        Assert.Equal("App.ChildClass", classType.Name);
        Assert.Equal(3, baseTypes.Count);
        Assert.Equal("App.Parent", baseTypes[0].Type.Name);
        Assert.Equal("class", baseTypes[0].Kind);

        Assert.Equal("IMetric", baseTypes[1].Type.Name);
        Assert.Equal("interface", baseTypes[1].Kind);

        Assert.Equal("IMetricExtractor", baseTypes[2].Type.Name);
        Assert.Equal("interface", baseTypes[2].Kind);
    }
}
