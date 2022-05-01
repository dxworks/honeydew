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

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Property.AccessorMethods;

public class CSharpAccessorMethodsTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly DotnetSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpAccessorMethodsTests()
    {
        var calledMethodSetterVisitor = new CSharpCalledMethodSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IMethodCallType>>
            {
                new MethodCallInfoVisitor()
            });

        var methodInfoVisitor = new MethodInfoVisitor();
        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new CSharpPropertySetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IPropertyType>>
                        {
                            new PropertyInfoVisitor(),
                            new CSharpAccessorMethodSetterVisitor(_loggerMock.Object,
                                new List<ITypeVisitor<IAccessorMethodType>>
                                {
                                    methodInfoVisitor,
                                    calledMethodSetterVisitor
                                })
                        })
                    })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/ClassWithPropertyOnlyGet.txt")]
    public void Extract_ShouldHaveOnlyGetAccessors_WhenGivenClassWithProperty(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal(3, classModel.Properties.Count);
        foreach (var property in classModel.Properties)
        {
            Assert.Equal(1, property.Accessors.Count);
            Assert.Equal("get", property.Accessors[0].Name);
        }
    }

    [Theory]
    [FileData("TestData/ClassWithPropertyOnlySet.txt")]
    public void Extract_ShouldHaveOnlySetAccessors_WhenGivenClassWithProperty(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal(2, classModel.Properties.Count);
        foreach (var property in classModel.Properties)
        {
            Assert.Equal(1, property.Accessors.Count);
            Assert.Equal("set", property.Accessors[0].Name);
        }
    }

    [Theory]
    [FileData("TestData/ClassWithPropertyOnlyInit.txt")]
    public void Extract_ShouldHaveOnlyInitAccessors_WhenGivenClassWithProperty(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal(2, classModel.Properties.Count);
        foreach (var property in classModel.Properties)
        {
            Assert.Equal(1, property.Accessors.Count);
            Assert.Equal("init", property.Accessors[0].Name);
        }
    }

    [Theory]
    [FileData("TestData/ClassWithPropertyGetSet.txt")]
    public void Extract_ShouldHaveGetSetAccessors_WhenGivenClassWithProperty(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal(2, classModel.Properties.Count);
        foreach (var property in classModel.Properties)
        {
            Assert.Equal(2, property.Accessors.Count);
            Assert.Equal("get", property.Accessors[0].Name);
            Assert.Equal("set", property.Accessors[1].Name);
        }
    }

    [Theory]
    [FileData("TestData/ClassWithPropertyGetInit.txt")]
    public void Extract_ShouldHaveGetInitAccessors_WhenGivenClassWithProperty(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal(2, classModel.Properties.Count);
        foreach (var property in classModel.Properties)
        {
            Assert.Equal(2, property.Accessors.Count);
            Assert.Equal("get", property.Accessors[0].Name);
            Assert.Equal("init", property.Accessors[1].Name);
        }
    }

    [Theory]
    [FileData("TestData/ClassWithPropertyExpressionBody_CalledMethods.txt")]
    public void Extract_ShouldCalledMethods_WhenGivenPropertyWithExpressionBodiedMember(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(2, classTypes.Count);

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal(1, classModel.Properties.Count);
        var property = classModel.Properties[0];
        Assert.Equal("GetExpressionBody", property.Name);
        Assert.Equal(1, property.Accessors.Count);
        Assert.Equal("get", property.Accessors[0].Name);

        var accessor = property.Accessors[0];

        Assert.Equal(3, accessor.CalledMethods.Count);

        Assert.Equal("Method", accessor.CalledMethods[0].Name);
        Assert.Equal("ExternClass", accessor.CalledMethods[0].DefinitionClassName);
        Assert.Equal("ExternClass", accessor.CalledMethods[0].LocationClassName);
        Assert.Equal(1, accessor.CalledMethods[0].ParameterTypes.Count);
        Assert.Equal("int", accessor.CalledMethods[0].ParameterTypes[0].Type.Name);

        Assert.Equal("Method2", accessor.CalledMethods[1].Name);
        Assert.Equal("Namespace1.Class2", accessor.CalledMethods[1].DefinitionClassName);
        Assert.Equal("Namespace1.Class2", accessor.CalledMethods[1].LocationClassName);
        Assert.Equal(1, accessor.CalledMethods[1].ParameterTypes.Count);
        Assert.Equal("int", accessor.CalledMethods[1].ParameterTypes[0].Type.Name);

        Assert.Equal("MyMethod", accessor.CalledMethods[2].Name);
        Assert.Equal("Namespace1.Class1", accessor.CalledMethods[2].DefinitionClassName);
        Assert.Equal("Namespace1.Class1", accessor.CalledMethods[2].LocationClassName);
        Assert.Empty(accessor.CalledMethods[2].ParameterTypes);
    }

    [Theory]
    [FileData("TestData/ClassWithPropertiesWithStatementBody.txt")]
    public void Extract_ShouldHaveCyclomaticComplexity_WhenGivenClassWithProperties(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal(2, classModel.Properties.Count);

        foreach (var property in classModel.Properties)
        {
            Assert.Equal(12, property.CyclomaticComplexity);

            foreach (var accessor in property.Accessors)
            {
                Assert.Equal(6, accessor.CyclomaticComplexity);
            }
        }
    }
}
