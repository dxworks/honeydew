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

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Method.Attributes;

public class CSharpMethodAttributeMetricTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly DotnetSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpMethodAttributeMetricTests()
    {
        var compositeVisitor =
            new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object, new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new CSharpMethodSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
                        {
                            new MethodInfoVisitor(),
                            new CSharpAttributeSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IAttributeType>>
                            {
                                new AttributeInfoVisitor()
                            })
                        })
                    })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("record")]
    [InlineData("struct")]
    public void Extract_ShouldExtractAttribute_WhenProvidedDifferentClassType(string classType)
    {
        var fileContent = $@"namespace Namespace1
{{
    public {classType} Class1 
    {{
        [System.Obsolete(""Message"")]
        public void Method1() {{}}

        [System.Obsolete(""Message"")]
        public int Method2(int a) {{return a;}}
    }}
}}";

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal(2, classModel.Methods.Count);

        foreach (var methodType in classModel.Methods)
        {
            var attributeTypes = methodType.Attributes;

            Assert.Equal(1, attributeTypes.Count);
            Assert.Equal("method", attributeTypes[0].Target);
            Assert.Equal("System.ObsoleteAttribute", attributeTypes[0].Name);
            Assert.Equal(1, attributeTypes[0].ParameterTypes.Count);
            Assert.Equal("string?", attributeTypes[0].ParameterTypes[0].Type.Name);
            Assert.Equal("string", attributeTypes[0].ParameterTypes[0].Type.FullType.Name);
            Assert.True(attributeTypes[0].ParameterTypes[0].Type.FullType.IsNullable);
        }
    }

    [Theory]
    [FileData("TestData/MethodWithOneAttributeWithNoParams.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithOneAttributeWithNoParams(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];
        Assert.Equal(2, classModel.Methods.Count);

        foreach (var methodType in classModel.Methods)
        {
            var attributeTypes = methodType.Attributes;
            Assert.Equal(1, attributeTypes.Count);
            Assert.Equal("method", attributeTypes[0].Target);
            Assert.Equal("System.SerializableAttribute", attributeTypes[0].Name);
            Assert.Empty(attributeTypes[0].ParameterTypes);
        }
    }

    [Theory]
    [FileData("TestData/MethodWithOneAttributeWithOneParam.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithOneAttributeWithOneParams(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal(2, classModel.Methods.Count);

        foreach (var methodType in classModel.Methods)
        {
            var attributeTypes = methodType.Attributes;
            Assert.Equal(1, attributeTypes.Count);
            Assert.Equal("method", attributeTypes[0].Target);
            Assert.Equal("System.ObsoleteAttribute", attributeTypes[0].Name);
            Assert.Equal(1, attributeTypes[0].ParameterTypes.Count);
            Assert.Equal("string?", attributeTypes[0].ParameterTypes[0].Type.Name);
            Assert.Equal("string", attributeTypes[0].ParameterTypes[0].Type.FullType.Name);
            Assert.True(attributeTypes[0].ParameterTypes[0].Type.FullType.IsNullable);
        }
    }

    [Theory]
    [FileData("TestData/MethodWithMultipleAttributesWithMultipleParams.txt")]
    [FileData("TestData/MethodWithMultipleAttributesWithMultipleParamsInDifferentSections.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithMultipleAttributesWitMultipleParams(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var methods = ((CSharpClassModel)classTypes[0]).Methods;

        Assert.Equal(2, methods.Count);

        foreach (var methodType in methods)
        {
            var attributeTypes = methodType.Attributes;
            Assert.Equal(3, attributeTypes.Count);
            foreach (var attribute in attributeTypes)
            {
                Assert.Equal("method", attribute.Target);
            }

            var attribute1 = attributeTypes[0];
            Assert.Equal(2, attribute1.ParameterTypes.Count);
            Assert.Equal("System.ObsoleteAttribute", attribute1.Name);
            Assert.Equal("string?", attribute1.ParameterTypes[0].Type.Name);
            Assert.Equal("string", attribute1.ParameterTypes[0].Type.FullType.Name);
            Assert.True(attribute1.ParameterTypes[0].Type.FullType.IsNullable);
            Assert.Equal("bool", attribute1.ParameterTypes[1].Type.Name);

            var attribute2 = attributeTypes[1];
            Assert.Equal("System.SerializableAttribute", attribute2.Name);
            Assert.Empty(attribute2.ParameterTypes);

            var attribute3 = attributeTypes[2];
            Assert.Equal("System.AttributeUsageAttribute", attribute3.Name);
            Assert.Equal(1, attribute3.ParameterTypes.Count);
            Assert.Equal("System.AttributeTargets", attribute3.ParameterTypes[0].Type.Name);
        }
    }

    [Theory]
    [FileData("TestData/MethodWithCustomAttribute.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithCustomAttribute(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classType = (CSharpClassModel)classTypes[1];

        Assert.Equal(2, classType.Methods.Count);

        foreach (var methodType in classType.Methods)
        {
            var methodTypeAttributes = methodType.Attributes;
            Assert.Equal(4, methodTypeAttributes.Count);
            foreach (var attribute in methodTypeAttributes)
            {
                Assert.Equal("method", attribute.Target);
                Assert.Equal("MyNamespace.MyAttribute", attribute.Name);
            }

            var attribute1 = methodTypeAttributes[0];
            Assert.Equal(1, attribute1.ParameterTypes.Count);
            Assert.Equal("string", attribute1.ParameterTypes[0].Type.Name);

            var attribute2 = methodTypeAttributes[1];
            Assert.Empty(attribute2.ParameterTypes);

            var attribute3 = methodTypeAttributes[2];
            Assert.Equal(1, attribute3.ParameterTypes.Count);
            Assert.Equal("string", attribute3.ParameterTypes[0].Type.Name);

            var attribute4 = methodTypeAttributes[3];
            Assert.Empty(attribute4.ParameterTypes);
        }
    }

    [Theory]
    [FileData("TestData/MethodWithExternAttribute.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithExternAttribute(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classType = (CSharpClassModel)classTypes[0];

        Assert.Equal(2, classType.Methods.Count);

        foreach (var methodType in classType.Methods)
        {
            Assert.Equal(5, methodType.Attributes.Count);
            foreach (var attribute in methodType.Attributes)
            {
                Assert.Equal("method", attribute.Target);
            }

            var attribute1 = methodType.Attributes[0];
            Assert.Equal("Extern", attribute1.Name);
            Assert.Equal(1, attribute1.ParameterTypes.Count);
            Assert.Equal("System.String", attribute1.ParameterTypes[0].Type.Name);

            var attribute2 = methodType.Attributes[1];
            Assert.Equal("ExternAttribute", attribute2.Name);
            Assert.Empty(attribute2.ParameterTypes);

            var attribute3 = methodType.Attributes[2];
            Assert.Equal("ExternAttribute", attribute3.Name);
            Assert.Equal(2, attribute3.ParameterTypes.Count);
            Assert.Equal("System.String", attribute3.ParameterTypes[0].Type.Name);
            Assert.Equal("System.Boolean", attribute3.ParameterTypes[1].Type.Name);

            var attribute4 = methodType.Attributes[3];
            Assert.Equal("Extern", attribute4.Name);
            Assert.Equal(1, attribute4.ParameterTypes.Count);
            Assert.Equal("System.Int32", attribute4.ParameterTypes[0].Type.Name);

            var attribute5 = methodType.Attributes[4];
            Assert.Equal("Extern", attribute5.Name);
            Assert.Equal(1, attribute5.ParameterTypes.Count);
            Assert.Equal("System.Object", attribute5.ParameterTypes[0].Type.Name);
        }
    }
}
