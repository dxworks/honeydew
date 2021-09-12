using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Attributes;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Attribute;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Constructor;
using HoneydewExtractors.CSharp.Metrics.Extraction.Parameter;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.Constructor
{
    public class CSharpConstructorParametersAttributeMetricTests
    {
        private readonly CSharpFactExtractor _factExtractor;
        private readonly Mock<ILogger> _loggerMock = new();

        public CSharpConstructorParametersAttributeMetricTests()
        {
            var compositeVisitor = new CompositeVisitor();

            var attributeSetterVisitor = new AttributeSetterVisitor(new List<IAttributeVisitor>
            {
                new AttributeInfoVisitor()
            });
            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<IClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new ConstructorSetterClassVisitor(new List<ICSharpConstructorVisitor>
                {
                    new ConstructorInfoVisitor(),
                    attributeSetterVisitor,
                    new ParameterSetterVisitor(new List<IParameterVisitor>
                    {
                        new ParameterInfoVisitor(),
                        attributeSetterVisitor
                    })
                })
            }));

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker(_loggerMock.Object)), compositeVisitor);
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

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(1, classModel.Constructors.Count);

            foreach (var parameterType in classModel.Constructors[0].ParameterTypes)
            {
                var attributeTypes = parameterType.Attributes;

                Assert.Equal(1, attributeTypes.Count);
                Assert.Equal("parameter", attributeTypes[0].Target);
                Assert.Equal("System.Diagnostics.CodeAnalysis.AllowNullAttribute", attributeTypes[0].Name);
                Assert.Empty(attributeTypes[0].ParameterTypes);

                Assert.Equal("Namespace1.Class1.Class1(int, int)", attributeTypes[0].ContainingTypeName);
            }
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Constructor/ParameterAttributes/ConstructorParametersWithSystemAttributes.txt")]
        public void Extract_ShouldExtractAttribute_WhenProvidedWithSystemAttributes(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(1, classModel.Constructors.Count);
            Assert.Equal(2, classModel.Constructors[0].ParameterTypes.Count);

            foreach (var parameterType in classModel.Constructors[0].ParameterTypes)
            {
                var attributeTypes = parameterType.Attributes;
                Assert.Equal(2, attributeTypes.Count);
                foreach (var attributeType in attributeTypes)
                {
                    Assert.Equal("parameter", attributeType.Target);
                    Assert.Equal("MyNamespace.MyClass.MyClass(int, int)", attributeType.ContainingTypeName);
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
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Constructor/ParameterAttributes/ConstructorParametersWithCustomAttribute.txt")]
        public void Extract_ShouldExtractAttribute_WhenProvidedWithCustomAttribute(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classType = (ClassModel)classTypes[1];

            Assert.Equal(2, classType.Constructors.Count);

            foreach (var constructorType in classType.Constructors)
            {
                foreach (var parameterType in constructorType.ParameterTypes)
                {
                    Assert.Equal(4, parameterType.Attributes.Count);
                    foreach (var attribute in parameterType.Attributes)
                    {
                        Assert.Equal("parameter", attribute.Target);
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

                foreach (var attributeType in parameterType.Attributes)
                {
                    Assert.Equal("MyNamespace.MyClass.MyClass(int, int)", attributeType.ContainingTypeName);
                }
            }
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Constructor/ParameterAttributes/ConstructorParametersWithExternAttribute.txt")]
        public void Extract_ShouldExtractAttribute_WhenProvidedWithExternAttribute(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classType = (ClassModel)classTypes[0];

            Assert.Equal(2, classType.Constructors.Count);

            foreach (var constructorType in classType.Constructors)
            {
                foreach (var parameterType in constructorType.ParameterTypes)
                {
                    Assert.Equal(4, parameterType.Attributes.Count);
                    foreach (var attribute in parameterType.Attributes)
                    {
                        Assert.Equal("parameter", attribute.Target);
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

                foreach (var attributeType in parameterType.Attributes)
                {
                    Assert.Equal("MyNamespace.MyClass.MyClass(int, int)", attributeType.ContainingTypeName);
                }
            }

            var constructorType2 = classType.Constructors[1];
            Assert.Equal(1, constructorType2.Attributes.Count);
            Assert.Equal("constructor", constructorType2.Attributes[0].Target);
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

                foreach (var attributeType in parameterType.Attributes)
                {
                    Assert.Equal("MyNamespace.MyClass.MyClass(int)", attributeType.ContainingTypeName);
                }
            }
        }
    }
}
