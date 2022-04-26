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

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Constructor.ParameterAttributes;

public class CSharpConstructorParametersAttributeMetricTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly DotnetSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpConstructorParametersAttributeMetricTests()
    {
        var attributeSetterVisitor = new CSharpAttributeSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IAttributeType>>
            {
                new AttributeInfoVisitor()
            });

        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new CSharpConstructorSetterClassVisitor(_loggerMock.Object,
                            new List<ITypeVisitor<IConstructorType>>
                            {
                                new ConstructorInfoVisitor(),
                                attributeSetterVisitor,
                                new CSharpParameterSetterVisitor(_loggerMock.Object,
                                    new List<ITypeVisitor<IParameterType>>
                                    {
                                        new ParameterInfoVisitor(),
                                        attributeSetterVisitor
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
    using System.Diagnostics.CodeAnalysis;

    public {classType} Class1 
    {{
        public Class1([AllowNull] int a, [AllowNull] int b) {{}}
    }}
}}";

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal(1, classModel.Constructors.Count);

        foreach (var parameterType in classModel.Constructors[0].ParameterTypes)
        {
            var attributeTypes = parameterType.Attributes;

            Assert.Equal(1, attributeTypes.Count);
            Assert.Equal("param", attributeTypes[0].Target);
            Assert.Equal("System.Diagnostics.CodeAnalysis.AllowNullAttribute", attributeTypes[0].Name);
            Assert.Empty(attributeTypes[0].ParameterTypes);
        }
    }

    [Theory]
    [FileData("TestData/ConstructorParametersWithSystemAttributes.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithSystemAttributes(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];
        Assert.Equal(1, classModel.Constructors.Count);
        Assert.Equal(2, classModel.Constructors[0].ParameterTypes.Count);

        foreach (var parameterType in classModel.Constructors[0].ParameterTypes)
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
    [FileData("TestData/ConstructorParametersWithCustomAttribute.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithCustomAttribute(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classType = (CSharpClassModel)classTypes[1];

        Assert.Equal(2, classType.Constructors.Count);

        foreach (var constructorType in classType.Constructors)
        {
            foreach (var parameterType in constructorType.ParameterTypes)
            {
                Assert.Equal(4, parameterType.Attributes.Count);
                foreach (var attribute in parameterType.Attributes)
                {
                    Assert.Equal("param", attribute.Target);
                    Assert.Equal("MyNamespace.MyAttribute", attribute.Name);
                }
            }
        }

        var constructorType1 = classType.Constructors[0];
        Assert.Empty(constructorType1.Attributes);
        Assert.Equal(4, constructorType1.ParameterTypes[0].Attributes.Count);
        Assert.Equal(4, constructorType1.ParameterTypes[1].Attributes.Count);

        foreach (var parameterType in constructorType1.ParameterTypes)
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
    }

    [Theory]
    [FileData(
        "TestData/ConstructorParametersWithExternAttribute.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithExternAttribute(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classType = (CSharpClassModel)classTypes[0];

        Assert.Equal(2, classType.Constructors.Count);

        foreach (var constructorType in classType.Constructors)
        {
            foreach (var parameterType in constructorType.ParameterTypes)
            {
                Assert.Equal(4, parameterType.Attributes.Count);
                foreach (var attribute in parameterType.Attributes)
                {
                    Assert.Equal("param", attribute.Target);
                    Assert.Equal("ExternAttribute", attribute.Name);
                }
            }
        }

        var constructorType1 = classType.Constructors[0];
        Assert.Empty(constructorType1.Attributes);
        Assert.Equal(4, constructorType1.ParameterTypes[0].Attributes.Count);
        Assert.Equal(4, constructorType1.ParameterTypes[1].Attributes.Count);

        foreach (var parameterType in constructorType1.ParameterTypes)
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

        var constructorType2 = classType.Constructors[1];
        Assert.Equal(1, constructorType2.Attributes.Count);
        Assert.Equal("method", constructorType2.Attributes[0].Target);
        Assert.Equal("System.ObsoleteAttribute", constructorType2.Attributes[0].Name);

        Assert.Equal(4, constructorType2.ParameterTypes[0].Attributes.Count);

        foreach (var parameterType in constructorType2.ParameterTypes)
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
