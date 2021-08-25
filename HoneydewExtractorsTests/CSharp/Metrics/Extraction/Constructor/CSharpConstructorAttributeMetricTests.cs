using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Extraction.Attribute;
using HoneydewExtractors.Core.Metrics.Extraction.Class;
using HoneydewExtractors.Core.Metrics.Extraction.Common;
using HoneydewExtractors.Core.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.Core.Metrics.Extraction.Constructor;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Attributes;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.CSharp;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.Constructor
{
    public class CSharpConstructorAttributeMetricTests
    {
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpConstructorAttributeMetricTests()
        {
            var compositeVisitor = new CompositeVisitor();

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<IClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new ConstructorSetterClassVisitor(new List<ICSharpConstructorVisitor>
                {
                    new ConstructorInfoVisitor(),
                    new AttributeSetterVisitor(new List<IAttributeVisitor>
                    {
                        new AttributeInfoVisitor()
                    })
                })
            }));

            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker()), compositeVisitor);
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
        public Class1() {{}}

        [System.Obsolete(""Message"")]
        public Class1(int a) {{}}
    }}
}}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(2, classModel.Constructors.Count);

            foreach (var constructorType in classModel.Constructors)
            {
                var attributeTypes = constructorType.Attributes;

                Assert.Equal(1, attributeTypes.Count);
                Assert.Equal("constructor", attributeTypes[0].Target);
                Assert.Equal("System.ObsoleteAttribute", attributeTypes[0].Name);
                Assert.Equal("Namespace1.Class1", attributeTypes[0].ContainingTypeName);
                Assert.Equal(1, attributeTypes[0].ParameterTypes.Count);
                Assert.Equal("string?", attributeTypes[0].ParameterTypes[0].Type.Name);
            }
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Constructor/Attributes/ConstructorWithOneAttributeWithNoParams.txt")]
        public void Extract_ShouldExtractAttribute_WhenProvidedWithOneAttributeWithNoParams(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(1, classModel.Constructors.Count);

            var attributeTypes = classModel.Constructors[0].Attributes;
            Assert.Equal(1, attributeTypes.Count);
            Assert.Equal("constructor", attributeTypes[0].Target);
            Assert.Equal("System.SerializableAttribute", attributeTypes[0].Name);
            Assert.Equal("Namespace1.Class1", attributeTypes[0].ContainingTypeName);
            Assert.Empty(attributeTypes[0].ParameterTypes);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Constructor/Attributes/ConstructorWithOneAttributeWithOneParam.txt")]
        public void Extract_ShouldExtractAttribute_WhenProvidedWithOneAttributeWithOneParams(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(1, classModel.Constructors.Count);

            var attributeTypes = classModel.Constructors[0].Attributes;
            Assert.Equal(1, attributeTypes.Count);
            Assert.Equal("constructor", attributeTypes[0].Target);
            Assert.Equal("System.ObsoleteAttribute", attributeTypes[0].Name);
            Assert.Equal("Namespace1.Class1", attributeTypes[0].ContainingTypeName);
            Assert.Equal(1, attributeTypes[0].ParameterTypes.Count);
            Assert.Equal("string?", attributeTypes[0].ParameterTypes[0].Type.Name);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Constructor/Attributes/ConstructorWithMultipleAttributesWithMultipleParams.txt")]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Constructor/Attributes/ConstructorWithMultipleAttributesWithMultipleParamsInDifferentSections.txt")]
        public void Extract_ShouldExtractAttribute_WhenProvidedWithMultipleAttributesWitMultipleParams(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var constructors = ((ClassModel)classTypes[0]).Constructors;

            Assert.Equal(1, constructors.Count);

            foreach (var fieldType in constructors)
            {
                var attributeTypes = fieldType.Attributes;
                Assert.Equal(3, attributeTypes.Count);
                foreach (var attribute in attributeTypes)
                {
                    Assert.Equal("constructor", attribute.Target);
                    Assert.Equal("Namespace1.Class1", attribute.ContainingTypeName);
                }

                var attribute1 = attributeTypes[0];
                Assert.Equal(2, attribute1.ParameterTypes.Count);
                Assert.Equal("System.ObsoleteAttribute", attribute1.Name);
                Assert.Equal("string?", attribute1.ParameterTypes[0].Type.Name);
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
            "TestData/CSharp/Metrics/Extraction/Constructor/Attributes/ConstructorWithCustomAttribute.txt")]
        public void Extract_ShouldExtractAttribute_WhenProvidedWithCustomAttribute(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classType = (ClassModel)classTypes[1];

            Assert.Equal(1, classType.Constructors.Count);

            foreach (var fieldType in classType.Constructors)
            {
                var fieldAttributes = fieldType.Attributes;
                Assert.Equal(4, fieldAttributes.Count);
                foreach (var attribute in fieldAttributes)
                {
                    Assert.Equal("constructor", attribute.Target);
                    Assert.Equal("MyNamespace.MyClass", attribute.ContainingTypeName);
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
            "TestData/CSharp/Metrics/Extraction/Constructor/Attributes/ConstructorWithExternAttribute.txt")]
        public void Extract_ShouldExtractAttribute_WhenProvidedWithExternAttribute(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classType = (ClassModel)classTypes[0];

            Assert.Equal(1, classType.Constructors.Count);

            foreach (var fieldType in classType.Constructors)
            {
                Assert.Equal(5, fieldType.Attributes.Count);
                foreach (var attribute in fieldType.Attributes)
                {
                    Assert.Equal("constructor", attribute.Target);
                    Assert.Equal("Namespace1.Class1", attribute.ContainingTypeName);
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
}
