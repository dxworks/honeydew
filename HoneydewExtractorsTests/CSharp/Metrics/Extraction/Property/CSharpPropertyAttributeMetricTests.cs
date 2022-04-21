using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Attributes;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Attribute;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Extraction.Property;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.Property;

public class CSharpPropertyAttributeMetricTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpPropertyAttributeMetricTests()
    {
        var compositeVisitor = new CompositeVisitor();

        var attributeSetterVisitor = new AttributeSetterVisitor(new List<IAttributeVisitor>
        {
            new AttributeInfoVisitor()
        });
        compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
        {
            new BaseInfoClassVisitor(),
            new PropertySetterClassVisitor(new List<ICSharpPropertyVisitor>
            {
                new PropertyInfoVisitor(),
                attributeSetterVisitor,
                new MethodAccessorSetterPropertyVisitor(new List<IMethodVisitor>
                {
                    new MethodInfoVisitor(),
                    attributeSetterVisitor,
                })
            })
        }));

        compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

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
        private int Field {{get;set;}}

        [System.Obsolete(""Message"")]
        public event System.Func<int> FField {{add{{}}remove{{}}}}
    }}
}}";

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        Assert.Equal(2, classModel.Properties.Count);

        foreach (var propertyType in classModel.Properties)
        {
            var attributeTypes = propertyType.Attributes;

            Assert.Equal(1, attributeTypes.Count);
            foreach (var attributeType in attributeTypes)
            {
                Assert.Equal("property", attributeType.Target);
                Assert.Equal("System.ObsoleteAttribute", attributeType.Name);
                Assert.Equal(1, attributeType.ParameterTypes.Count);
                Assert.Equal("string?", attributeType.ParameterTypes[0].Type.Name);
                Assert.Equal("string", attributeType.ParameterTypes[0].Type.FullType.Name);
                Assert.True(attributeType.ParameterTypes[0].Type.FullType.IsNullable);
            }
        }
    }


    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Property/Attributes/PropertyWithOneAttributeWithNoParams.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithOneAttributeWithNoParams(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];
        Assert.Equal(2, classModel.Properties.Count);

        foreach (var property in classModel.Properties)
        {
            var attributeTypes = property.Attributes;
            Assert.Equal(1, attributeTypes.Count);

            foreach (var attributeType in attributeTypes)
            {
                Assert.Equal("property", attributeType.Target);
                Assert.Equal("System.SerializableAttribute", attributeType.Name);
                Assert.Empty(attributeType.ParameterTypes);
            }
        }
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Property/Attributes/PropertyWithOneAttributeWithOneParam.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithOneAttributeWithOneParams(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        Assert.Equal(2, classModel.Properties.Count);

        foreach (var propertyType in classModel.Properties)
        {
            var attributeTypes = propertyType.Attributes;
            Assert.Equal(1, attributeTypes.Count);

            foreach (var attributeType in attributeTypes)
            {
                Assert.Equal("property", attributeType.Target);
                Assert.Equal("System.ObsoleteAttribute", attributeType.Name);
                Assert.Equal(1, attributeType.ParameterTypes.Count);
                Assert.Equal("string?", attributeType.ParameterTypes[0].Type.Name);
                Assert.Equal("string", attributeType.ParameterTypes[0].Type.FullType.Name);
                Assert.True(attributeType.ParameterTypes[0].Type.FullType.IsNullable);
            }
        }
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Property/Attributes/PropertyWithMultipleAttributesWithMultipleParams.txt")]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Property/Attributes/PropertyWithMultipleAttributesWithMultipleParamsInDifferentSections.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithMultipleAttributesWitMultipleParams(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var properties = ((ClassModel)classTypes[0]).Properties;

        Assert.Equal(2, properties.Count);

        foreach (var propertyType in properties)
        {
            var attributeTypes = propertyType.Attributes;
            Assert.Equal(3, attributeTypes.Count);
            foreach (var attribute in attributeTypes)
            {
                Assert.Equal("property", attribute.Target);
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
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Property/Attributes/PropertyWithCustomAttribute.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithCustomAttribute(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classType = (ClassModel)classTypes[1];

        Assert.Equal(2, classType.Properties.Count);

        foreach (var propertyType in classType.Properties)
        {
            var fieldAttributes = propertyType.Attributes;
            Assert.Equal(4, fieldAttributes.Count);
            foreach (var attribute in fieldAttributes)
            {
                Assert.Equal("property", attribute.Target);
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
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Property/Attributes/PropertyWithExternAttribute.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithExternAttribute(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classType = (ClassModel)classTypes[0];

        Assert.Equal(2, classType.Properties.Count);

        foreach (var propertyType in classType.Properties)
        {
            Assert.Equal(5, propertyType.Attributes.Count);
            foreach (var attribute in propertyType.Attributes)
            {
                Assert.Equal("property", attribute.Target);
            }

            var attribute1 = propertyType.Attributes[0];
            Assert.Equal("Extern", attribute1.Name);
            Assert.Equal(1, attribute1.ParameterTypes.Count);
            Assert.Equal("System.String", attribute1.ParameterTypes[0].Type.Name);

            var attribute2 = propertyType.Attributes[1];
            Assert.Equal("ExternAttribute", attribute2.Name);
            Assert.Empty(attribute2.ParameterTypes);

            var attribute3 = propertyType.Attributes[2];
            Assert.Equal("ExternAttribute", attribute3.Name);
            Assert.Equal(2, attribute3.ParameterTypes.Count);
            Assert.Equal("System.String", attribute3.ParameterTypes[0].Type.Name);
            Assert.Equal("System.Boolean", attribute3.ParameterTypes[1].Type.Name);

            var attribute4 = propertyType.Attributes[3];
            Assert.Equal("Extern", attribute4.Name);
            Assert.Equal(1, attribute4.ParameterTypes.Count);
            Assert.Equal("System.Int32", attribute4.ParameterTypes[0].Type.Name);

            var attribute5 = propertyType.Attributes[4];
            Assert.Equal("Extern", attribute5.Name);
            Assert.Equal(1, attribute5.ParameterTypes.Count);
            Assert.Equal("System.Object", attribute5.ParameterTypes[0].Type.Name);
        }
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Property/Attributes/PropertyWithAccessorsWithAttributes.txt")]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Property/Attributes/EventPropertyWithAccessorsWithAttributes.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithPropertyAccessors(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classType = (ClassModel)classTypes[0];

        Assert.Equal(5, classType.Properties.Count);
        foreach (var propertyType in classType.Properties)
        {
            Assert.Empty(propertyType.Attributes);

            foreach (var accessor in propertyType.Accessors)
            {
                Assert.Equal(3, accessor.Attributes.Count);

                foreach (var accessorAttribute in accessor.Attributes)
                {
                    Assert.Equal("method", accessorAttribute.Target);
                }

                var attribute1 = accessor.Attributes[0];
                Assert.Equal("Namespace1.MyAttribute", attribute1.Name);
                Assert.Equal(1, attribute1.ParameterTypes.Count);
                Assert.Equal("string", attribute1.ParameterTypes[0].Type.Name);

                var attribute2 = accessor.Attributes[1];
                Assert.Equal("System.ObsoleteAttribute", attribute2.Name);
                Assert.Empty(attribute2.ParameterTypes);

                var attribute3 = accessor.Attributes[2];
                Assert.Equal("ExternAttribute", attribute3.Name);
                Assert.Empty(attribute3.ParameterTypes);
            }
        }
    }

    [Theory]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Property/Attributes/PropertyWithAccessorsWithReturnValueAttributes.txt")]
    [FileData(
        "TestData/CSharp/Metrics/Extraction/Property/Attributes/EventPropertyWithAccessorsWithReturnValueAttributes.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithPropertyAccessorsForReturnValue(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classType = (ClassModel)classTypes[0];

        Assert.Equal(5, classType.Properties.Count);
        foreach (var propertyType in classType.Properties)
        {
            Assert.Empty(propertyType.Attributes);

            foreach (var accessor in propertyType.Accessors)
            {
                Assert.Equal(0, accessor.Attributes.Count);
                Assert.Equal(3, accessor.ReturnValue.Attributes.Count);

                foreach (var accessorAttribute in accessor.ReturnValue.Attributes)
                {
                    Assert.Equal("return", accessorAttribute.Target);
                }

                var attribute1 = accessor.ReturnValue.Attributes[0];
                Assert.Equal("Namespace1.MyAttribute", attribute1.Name);
                Assert.Equal(1, attribute1.ParameterTypes.Count);
                Assert.Equal("string", attribute1.ParameterTypes[0].Type.Name);

                var attribute2 = accessor.ReturnValue.Attributes[1];
                Assert.Equal("System.ObsoleteAttribute", attribute2.Name);
                Assert.Empty(attribute2.ParameterTypes);

                var attribute3 = accessor.ReturnValue.Attributes[2];
                Assert.Equal("ExternAttribute", attribute3.Name);
                Assert.Empty(attribute3.ParameterTypes);
            }
        }
    }
}
