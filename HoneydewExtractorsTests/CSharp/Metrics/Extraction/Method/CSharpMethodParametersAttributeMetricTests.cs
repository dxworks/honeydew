using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Attributes;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Attribute;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Extraction.Parameter;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.Method
{
    public class CSharpMethodParametersAttributeMetricTests
    {
        private readonly CSharpFactExtractor _factExtractor;
        private readonly Mock<ILogger> _loggerMock = new();

        public CSharpMethodParametersAttributeMetricTests()
        {
            var compositeVisitor = new CompositeVisitor();

            var attributeSetterVisitor = new AttributeSetterVisitor(new List<IAttributeVisitor>
            {
                new AttributeInfoVisitor()
            });
            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<IClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new MethodSetterClassVisitor(new List<ICSharpMethodVisitor>
                {
                    new MethodInfoVisitor(),
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

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(1, classModel.Methods.Count);

            foreach (var parameterType in classModel.Methods[0].ParameterTypes)
            {
                var attributeTypes = parameterType.Attributes;

                Assert.Equal(1, attributeTypes.Count);
                Assert.Equal("parameter", attributeTypes[0].Target);
                Assert.Equal("System.Diagnostics.CodeAnalysis.AllowNullAttribute", attributeTypes[0].Name);
                Assert.Empty(attributeTypes[0].ParameterTypes);

                Assert.Equal("Namespace1.Class1.Method1(int, int)", attributeTypes[0].ContainingTypeName);
            }
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/ParameterAttributes/MethodParametersWithSystemAttributes.txt")]
        public void Extract_ShouldExtractAttribute_WhenProvidedWithSystemAttributes(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(1, classModel.Methods.Count);
            Assert.Equal(2, classModel.Methods[0].ParameterTypes.Count);

            foreach (var parameterType in classModel.Methods[0].ParameterTypes)
            {
                var attributeTypes = parameterType.Attributes;
                Assert.Equal(2, attributeTypes.Count);
                foreach (var attributeType in attributeTypes)
                {
                    Assert.Equal("parameter", attributeType.Target);
                    Assert.Equal("MyNamespace.MyClass.Method1(int, int)", attributeType.ContainingTypeName);
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
            "TestData/CSharp/Metrics/Extraction/Method/ParameterAttributes/MethodParametersWithCustomAttribute.txt")]
        public void Extract_ShouldExtractAttribute_WhenProvidedWithCustomAttribute(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classType = (ClassModel)classTypes[1];

            Assert.Equal(3, classType.Methods.Count);

            foreach (var methodType in classType.Methods)
            {
                foreach (var parameterType in methodType.ParameterTypes)
                {
                    foreach (var attribute in parameterType.Attributes)
                    {
                        Assert.Equal("parameter", attribute.Target);
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

                foreach (var attributeType in parameterType.Attributes)
                {
                    Assert.Equal("MyNamespace.MyClass.Method1(int, int)", attributeType.ContainingTypeName);
                }
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

                Assert.Equal("MyNamespace.MyClass.Method2(int)", attribute1.ContainingTypeName);
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

                foreach (var attributeType in parameterType.Attributes)
                {
                    Assert.Equal("MyNamespace.MyClass.Method3(int)", attributeType.ContainingTypeName);
                }
            }
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/ParameterAttributes/MethodParametersWithExternAttribute.txt")]
        public void Extract_ShouldExtractAttribute_WhenProvidedWithExternAttribute(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classType = (ClassModel)classTypes[0];

            Assert.Equal(3, classType.Methods.Count);

            foreach (var methodType in classType.Methods)
            {
                foreach (var parameterType in methodType.ParameterTypes)
                {
                    foreach (var attribute in parameterType.Attributes)
                    {
                        Assert.Equal("parameter", attribute.Target);
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

                foreach (var attributeType in parameterType.Attributes)
                {
                    Assert.Equal("MyNamespace.MyClass.Method1(int, int)", attributeType.ContainingTypeName);
                }
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

                Assert.Equal("MyNamespace.MyClass.Method2(int)", attribute1.ContainingTypeName);
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

                foreach (var attributeType in parameterType.Attributes)
                {
                    Assert.Equal("MyNamespace.MyClass.Method3(int)", attributeType.ContainingTypeName);
                }
            }
        }
    }
}
