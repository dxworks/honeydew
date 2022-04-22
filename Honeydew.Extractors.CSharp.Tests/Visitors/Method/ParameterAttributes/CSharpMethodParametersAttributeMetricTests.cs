using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using HoneydewCore.Logging;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Method.ParameterAttributes;

public class CSharpMethodParametersAttributeMetricTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpMethodParametersAttributeMetricTests()
    {
        var compositeVisitor = new CompositeVisitor(_loggerMock.Object);

        var attributeSetterVisitor = new AttributeSetterVisitor(_loggerMock.Object, new List<IAttributeVisitor>
        {
            new AttributeInfoVisitor()
        });
        compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(_loggerMock.Object, new List<IClassVisitor>
        {
            new BaseInfoClassVisitor(),
            new MethodSetterClassVisitor(_loggerMock.Object, new List<ICSharpMethodVisitor>
            {
                new MethodInfoVisitor(),
                attributeSetterVisitor,
                new ParameterSetterVisitor(_loggerMock.Object, new List<IParameterVisitor>
                {
                    new ParameterInfoVisitor(),
                    attributeSetterVisitor
                })
            })
        }));

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("interface")]
    [InlineData("record")]
    [InlineData("struct")]
    public void Extract_ShouldExtractAttribute_WhenProvidedDifferentClassType(string classType)
    {
        var fileContent = $@"namespace Namespace1
{{
    using System.Diagnostics.CodeAnalysis;

    public {classType} Class1 
    {{
        public void Method1([AllowNull] int a, [AllowNull] int b) {{}}
    }}
}}";

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        Assert.Equal(1, classModel.Methods.Count);

        foreach (var parameterType in classModel.Methods[0].ParameterTypes)
        {
            var attributeTypes = parameterType.Attributes;

            Assert.Equal(1, attributeTypes.Count);
            Assert.Equal("param", attributeTypes[0].Target);
            Assert.Equal("System.Diagnostics.CodeAnalysis.AllowNullAttribute", attributeTypes[0].Name);
            Assert.Empty(attributeTypes[0].ParameterTypes);
        }
    }

    [Theory]
    [FileData("TestData/MethodParametersWithSystemAttributes.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithSystemAttributes(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];
        Assert.Equal(1, classModel.Methods.Count);
        Assert.Equal(2, classModel.Methods[0].ParameterTypes.Count);

        foreach (var parameterType in classModel.Methods[0].ParameterTypes)
        {
            var attributeTypes = parameterType.Attributes;
            Assert.Equal(2, attributeTypes.Count);
            foreach (var attributeType in attributeTypes)
            {
                Assert.Equal("param", attributeType.Target);
            }

            var attribute1 = attributeTypes[0];
            Assert.Equal("System.Diagnostics.CodeAnalysis.NotNullAttribute", attribute1.Name);
            Assert.Empty(attribute1.ParameterTypes);

            var attribute2 = attributeTypes[1];
            Assert.Equal("System.Diagnostics.CodeAnalysis.SuppressMessageAttribute", attribute2.Name);
            Assert.Equal(2, attribute2.ParameterTypes.Count);
            Assert.Equal("string", attribute2.ParameterTypes[0].Type.Name);
            Assert.Equal("string", attribute2.ParameterTypes[1].Type.Name);
        }
    }

    [Theory]
    [FileData("TestData/MethodParametersWithCustomAttribute.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithCustomAttribute(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classType = (ClassModel)classTypes[1];

        Assert.Equal(3, classType.Methods.Count);

        foreach (var methodType in classType.Methods)
        {
            foreach (var parameterType in methodType.ParameterTypes)
            {
                foreach (var attribute in parameterType.Attributes)
                {
                    Assert.Equal("param", attribute.Target);
                    Assert.Equal("MyNamespace.MyAttribute", attribute.Name);
                }
            }
        }

        var methodType1 = classType.Methods[0];
        Assert.Empty(methodType1.Attributes);
        Assert.Empty(methodType1.ReturnValue.Attributes);
        Assert.Equal(4, methodType1.ParameterTypes[0].Attributes.Count);
        Assert.Equal(4, methodType1.ParameterTypes[1].Attributes.Count);

        foreach (var parameterType in methodType1.ParameterTypes)
        {
            var attribute1 = parameterType.Attributes[0];
            Assert.Equal("MyNamespace.MyAttribute", attribute1.Name);
            Assert.Equal(1, attribute1.ParameterTypes.Count);
            Assert.Equal("string", attribute1.ParameterTypes[0].Type.Name);

            var attribute2 = parameterType.Attributes[1];
            Assert.Equal("MyNamespace.MyAttribute", attribute2.Name);
            Assert.Empty(attribute2.ParameterTypes);

            var attribute3 = parameterType.Attributes[2];
            Assert.Equal("MyNamespace.MyAttribute", attribute3.Name);
            Assert.Equal(1, attribute3.ParameterTypes.Count);
            Assert.Equal("string", attribute3.ParameterTypes[0].Type.Name);

            var attribute4 = parameterType.Attributes[3];
            Assert.Equal("MyNamespace.MyAttribute", attribute4.Name);
            Assert.Empty(attribute4.ParameterTypes);
        }


        var methodType2 = classType.Methods[1];
        Assert.Empty(methodType2.Attributes);
        Assert.Empty(methodType2.ReturnValue.Attributes);
        Assert.Equal(1, methodType2.ParameterTypes[0].Attributes.Count);

        foreach (var parameterType in methodType2.ParameterTypes)
        {
            var attribute1 = parameterType.Attributes[0];
            Assert.Equal("MyNamespace.MyAttribute", attribute1.Name);
            Assert.Equal(1, attribute1.ParameterTypes.Count);
            Assert.Equal("int", attribute1.ParameterTypes[0].Type.Name);
        }

        var methodType3 = classType.Methods[2];
        Assert.Equal(1, methodType3.Attributes.Count);
        Assert.Equal("method", methodType3.Attributes[0].Target);
        Assert.Equal("System.ObsoleteAttribute", methodType3.Attributes[0].Name);

        Assert.Empty(methodType3.ReturnValue.Attributes);

        Assert.Equal(4, methodType3.ParameterTypes[0].Attributes.Count);

        foreach (var parameterType in methodType3.ParameterTypes)
        {
            var attribute1 = parameterType.Attributes[0];
            Assert.Equal("MyNamespace.MyAttribute", attribute1.Name);
            Assert.Equal(1, attribute1.ParameterTypes.Count);
            Assert.Equal("string", attribute1.ParameterTypes[0].Type.Name);

            var attribute2 = parameterType.Attributes[1];
            Assert.Equal("MyNamespace.MyAttribute", attribute2.Name);
            Assert.Empty(attribute2.ParameterTypes);

            var attribute3 = parameterType.Attributes[2];
            Assert.Equal("MyNamespace.MyAttribute", attribute3.Name);
            Assert.Equal(2, attribute3.ParameterTypes.Count);
            Assert.Equal("string", attribute3.ParameterTypes[0].Type.Name);
            Assert.Equal("int", attribute3.ParameterTypes[1].Type.Name);

            var attribute4 = parameterType.Attributes[3];
            Assert.Equal("MyNamespace.MyAttribute", attribute4.Name);
            Assert.Empty(attribute4.ParameterTypes);
        }
    }

    [Theory]
    [FileData("TestData/MethodParametersWithExternAttribute.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithExternAttribute(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classType = (ClassModel)classTypes[0];

        Assert.Equal(3, classType.Methods.Count);

        foreach (var methodType in classType.Methods)
        {
            foreach (var parameterType in methodType.ParameterTypes)
            {
                foreach (var attribute in parameterType.Attributes)
                {
                    Assert.Equal("param", attribute.Target);
                    Assert.Equal("ExternAttribute", attribute.Name);
                }
            }
        }

        var methodType1 = classType.Methods[0];
        Assert.Empty(methodType1.Attributes);
        Assert.Empty(methodType1.ReturnValue.Attributes);
        Assert.Equal(4, methodType1.ParameterTypes[0].Attributes.Count);
        Assert.Equal(4, methodType1.ParameterTypes[1].Attributes.Count);

        foreach (var parameterType in methodType1.ParameterTypes)
        {
            var attribute1 = parameterType.Attributes[0];
            Assert.Equal("ExternAttribute", attribute1.Name);
            Assert.Equal(1, attribute1.ParameterTypes.Count);
            Assert.Equal("System.String", attribute1.ParameterTypes[0].Type.Name);

            var attribute2 = parameterType.Attributes[1];
            Assert.Equal("ExternAttribute", attribute2.Name);
            Assert.Empty(attribute2.ParameterTypes);

            var attribute3 = parameterType.Attributes[2];
            Assert.Equal("ExternAttribute", attribute3.Name);
            Assert.Equal(1, attribute3.ParameterTypes.Count);
            Assert.Equal("System.String", attribute3.ParameterTypes[0].Type.Name);

            var attribute4 = parameterType.Attributes[3];
            Assert.Equal("ExternAttribute", attribute4.Name);
            Assert.Empty(attribute4.ParameterTypes);
        }


        var methodType2 = classType.Methods[1];
        Assert.Empty(methodType2.Attributes);
        Assert.Empty(methodType2.ReturnValue.Attributes);
        Assert.Equal(1, methodType2.ParameterTypes[0].Attributes.Count);

        foreach (var parameterType in methodType2.ParameterTypes)
        {
            var attribute1 = parameterType.Attributes[0];
            Assert.Equal("ExternAttribute", attribute1.Name);
            Assert.Equal(1, attribute1.ParameterTypes.Count);
            Assert.Equal("System.Int32", attribute1.ParameterTypes[0].Type.Name);
        }

        var methodType3 = classType.Methods[2];
        Assert.Equal(1, methodType3.Attributes.Count);
        Assert.Equal("method", methodType3.Attributes[0].Target);
        Assert.Equal("System.ObsoleteAttribute", methodType3.Attributes[0].Name);

        Assert.Equal(1, methodType3.ReturnValue.Attributes.Count);
        Assert.Equal("return", methodType3.ReturnValue.Attributes[0].Target);
        Assert.Equal("ExternAttribute", methodType3.ReturnValue.Attributes[0].Name);

        Assert.Equal(4, methodType3.ParameterTypes[0].Attributes.Count);

        foreach (var parameterType in methodType3.ParameterTypes)
        {
            var attribute1 = parameterType.Attributes[0];
            Assert.Equal("ExternAttribute", attribute1.Name);
            Assert.Equal(1, attribute1.ParameterTypes.Count);
            Assert.Equal("System.String", attribute1.ParameterTypes[0].Type.Name);

            var attribute2 = parameterType.Attributes[1];
            Assert.Equal("ExternAttribute", attribute2.Name);
            Assert.Empty(attribute2.ParameterTypes);

            var attribute3 = parameterType.Attributes[2];
            Assert.Equal("ExternAttribute", attribute3.Name);
            Assert.Equal(2, attribute3.ParameterTypes.Count);
            Assert.Equal("System.String", attribute3.ParameterTypes[0].Type.Name);
            Assert.Equal("System.Int32", attribute3.ParameterTypes[1].Type.Name);

            var attribute4 = parameterType.Attributes[3];
            Assert.Equal("ExternAttribute", attribute4.Name);
            Assert.Empty(attribute4.ParameterTypes);
        }
    }
}
