using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Models;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Field.Attributes;

public class CSharpFieldAttributeMetricTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpFieldAttributeMetricTests()
    {
        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterCompilationUnitVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new CSharpFieldSetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IFieldType>>
                        {
                            new FieldInfoVisitor(),
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
        private int Field;

        [System.Obsolete(""Message"")]
        public event System.Func<int> FField;
    }}
}}";
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        Assert.Equal(2, classModel.Fields.Count);

        foreach (var fieldType in classModel.Fields)
        {
            var attributeTypes = fieldType.Attributes;

            Assert.Equal(1, attributeTypes.Count);

            foreach (var attributeType in attributeTypes)
            {
                Assert.Equal("field", attributeType.Target);
                Assert.Equal("System.ObsoleteAttribute", attributeType.Name);
                Assert.Equal(1, attributeType.ParameterTypes.Count);
                Assert.Equal("string?", attributeType.ParameterTypes[0].Type.Name);
                Assert.Equal("string", attributeType.ParameterTypes[0].Type.FullType.Name);
                Assert.True(attributeType.ParameterTypes[0].Type.FullType.IsNullable);
            }
        }
    }


    [Theory]
    [FileData("TestData/FieldWithOneAttributeWithNoParams.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithOneAttributeWithNoParams(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];
        Assert.Equal(2, classModel.Fields.Count);

        foreach (var fieldType in classModel.Fields)
        {
            var attributeTypes = fieldType.Attributes;
            Assert.Equal(1, attributeTypes.Count);

            foreach (var attributeType in attributeTypes)
            {
                Assert.Equal("field", attributeType.Target);
                Assert.Equal("System.SerializableAttribute", attributeType.Name);
                Assert.Empty(attributeType.ParameterTypes);
            }
        }
    }

    [Theory]
    [FileData("TestData/FieldWithOneAttributeWithOneParam.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithOneAttributeWithOneParams(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        Assert.Equal(2, classModel.Fields.Count);

        foreach (var fieldType in classModel.Fields)
        {
            var attributeTypes = fieldType.Attributes;
            Assert.Equal(1, attributeTypes.Count);

            foreach (var attributeType in attributeTypes)
            {
                Assert.Equal("field", attributeType.Target);
                Assert.Equal("System.ObsoleteAttribute", attributeType.Name);
                Assert.Equal(1, attributeType.ParameterTypes.Count);
                Assert.Equal("string?", attributeType.ParameterTypes[0].Type.Name);
                Assert.Equal("string", attributeType.ParameterTypes[0].Type.FullType.Name);
                Assert.True(attributeType.ParameterTypes[0].Type.FullType.IsNullable);
            }
        }
    }

    [Theory]
    [FileData("TestData/FieldWithMultipleAttributesWithMultipleParams.txt")]
    [FileData("TestData/FieldWithMultipleAttributesWithMultipleParamsInDifferentSections.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithMultipleAttributesWitMultipleParams(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var fields = ((ClassModel)classTypes[0]).Fields;

        Assert.Equal(2, fields.Count);

        foreach (var fieldType in fields)
        {
            var attributeTypes = fieldType.Attributes;
            Assert.Equal(3, attributeTypes.Count);
            foreach (var attribute in attributeTypes)
            {
                Assert.Equal("field", attribute.Target);
            }

            var attribute1 = attributeTypes[0];
            Assert.Equal(2, attribute1.ParameterTypes.Count);
            Assert.Equal("System.ObsoleteAttribute", attribute1.Name);
            Assert.Equal("string?", attribute1.ParameterTypes[0].Type.Name);
            Assert.Equal("string", attribute1.ParameterTypes[0].Type.FullType.Name);
            Assert.True(attribute1.ParameterTypes[0].Type.FullType.IsNullable);
            Assert.Equal("bool", attribute1.ParameterTypes[1].Type.Name);
            Assert.Equal("bool", attribute1.ParameterTypes[1].Type.FullType.Name);
            Assert.False(attribute1.ParameterTypes[1].Type.FullType.IsNullable);

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
    [FileData("TestData/FieldWithCustomAttribute.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithCustomAttribute(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classType = (ClassModel)classTypes[1];

        Assert.Equal(2, classType.Fields.Count);

        foreach (var fieldType in classType.Fields)
        {
            var fieldAttributes = fieldType.Attributes;
            Assert.Equal(4, fieldAttributes.Count);
            foreach (var attribute in fieldAttributes)
            {
                Assert.Equal("field", attribute.Target);
                Assert.Equal("MyNamespace.MyAttribute", attribute.Name);
            }

            var attribute1 = fieldAttributes[0];
            Assert.Equal(1, attribute1.ParameterTypes.Count);
            Assert.Equal("string", attribute1.ParameterTypes[0].Type.Name);

            var attribute2 = fieldAttributes[1];
            Assert.Empty(attribute2.ParameterTypes);

            var attribute3 = fieldAttributes[2];
            Assert.Equal(1, attribute3.ParameterTypes.Count);
            Assert.Equal("string", attribute3.ParameterTypes[0].Type.Name);

            var attribute4 = fieldAttributes[3];
            Assert.Empty(attribute4.ParameterTypes);
        }
    }

    [Theory]
    [FileData("TestData/FieldWithExternAttribute.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithExternAttribute(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classType = (ClassModel)classTypes[0];

        Assert.Equal(2, classType.Fields.Count);

        foreach (var fieldType in classType.Fields)
        {
            Assert.Equal(5, fieldType.Attributes.Count);
            foreach (var attribute in fieldType.Attributes)
            {
                Assert.Equal("field", attribute.Target);
            }

            var attribute1 = fieldType.Attributes[0];
            Assert.Equal("Extern", attribute1.Name);
            Assert.Equal(1, attribute1.ParameterTypes.Count);
            Assert.Equal("System.String", attribute1.ParameterTypes[0].Type.Name);

            var attribute2 = fieldType.Attributes[1];
            Assert.Equal("ExternAttribute", attribute2.Name);
            Assert.Empty(attribute2.ParameterTypes);

            var attribute3 = fieldType.Attributes[2];
            Assert.Equal("ExternAttribute", attribute3.Name);
            Assert.Equal(2, attribute3.ParameterTypes.Count);
            Assert.Equal("System.String", attribute3.ParameterTypes[0].Type.Name);
            Assert.Equal("System.Boolean", attribute3.ParameterTypes[1].Type.Name);

            var attribute4 = fieldType.Attributes[3];
            Assert.Equal("Extern", attribute4.Name);
            Assert.Equal(1, attribute4.ParameterTypes.Count);
            Assert.Equal("System.Int32", attribute4.ParameterTypes[0].Type.Name);

            var attribute5 = fieldType.Attributes[4];
            Assert.Equal("Extern", attribute5.Name);
            Assert.Equal(1, attribute5.ParameterTypes.Count);
            Assert.Equal("System.Object", attribute5.ParameterTypes[0].Type.Name);
        }
    }
}
