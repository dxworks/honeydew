using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Attributes;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Attribute;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewModels.CSharp;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.Method
{
    public class CSharpMethodAttributeMetricTests
    {
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpMethodAttributeMetricTests()
        {
            var compositeVisitor = new CompositeVisitor();

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<IClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new MethodSetterClassVisitor(new List<ICSharpMethodVisitor>
                {
                    new MethodInfoVisitor(),
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
        public void Method1() {{}}

        [System.Obsolete(""Message"")]
        public int Method2(int a) {{return a;}}
    }}
}}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(2, classModel.Methods.Count);

            foreach (var methodType in classModel.Methods)
            {
                var attributeTypes = methodType.Attributes;

                Assert.Equal(1, attributeTypes.Count);
                Assert.Equal("method", attributeTypes[0].Target);
                Assert.Equal("System.ObsoleteAttribute", attributeTypes[0].Name);
                Assert.Equal(1, attributeTypes[0].ParameterTypes.Count);
                Assert.Equal("string?", attributeTypes[0].ParameterTypes[0].Type.Name);
            }

            Assert.Equal("Namespace1.Class1.Method1()", classModel.Methods[0].Attributes[0].ContainingTypeName);
            Assert.Equal("Namespace1.Class1.Method2(int)", classModel.Methods[1].Attributes[0].ContainingTypeName);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/Attributes/MethodWithOneAttributeWithNoParams.txt")]
        public void Extract_ShouldExtractAttribute_WhenProvidedWithOneAttributeWithNoParams(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(2, classModel.Methods.Count);

            foreach (var methodType in classModel.Methods)
            {
                var attributeTypes = methodType.Attributes;
                Assert.Equal(1, attributeTypes.Count);
                Assert.Equal("method", attributeTypes[0].Target);
                Assert.Equal("System.SerializableAttribute", attributeTypes[0].Name);
                Assert.Empty(attributeTypes[0].ParameterTypes);
            }

            Assert.Equal("Namespace1.Class1.Method1()", classModel.Methods[0].Attributes[0].ContainingTypeName);
            Assert.Equal("Namespace1.Class1.Method2()", classModel.Methods[1].Attributes[0].ContainingTypeName);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/Attributes/MethodWithOneAttributeWithOneParam.txt")]
        public void Extract_ShouldExtractAttribute_WhenProvidedWithOneAttributeWithOneParams(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(2, classModel.Methods.Count);

            foreach (var methodType in classModel.Methods)
            {
                var attributeTypes = methodType.Attributes;
                Assert.Equal(1, attributeTypes.Count);
                Assert.Equal("method", attributeTypes[0].Target);
                Assert.Equal("System.ObsoleteAttribute", attributeTypes[0].Name);
                Assert.Equal(1, attributeTypes[0].ParameterTypes.Count);
                Assert.Equal("string?", attributeTypes[0].ParameterTypes[0].Type.Name);
            }

            Assert.Equal("Namespace1.Class1.Method(int)", classModel.Methods[0].Attributes[0].ContainingTypeName);
            Assert.Equal("Namespace1.Class1.Method(string)", classModel.Methods[1].Attributes[0].ContainingTypeName);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/Attributes/MethodWithMultipleAttributesWithMultipleParams.txt")]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/Attributes/MethodWithMultipleAttributesWithMultipleParamsInDifferentSections.txt")]
        public void Extract_ShouldExtractAttribute_WhenProvidedWithMultipleAttributesWitMultipleParams(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var methods = ((ClassModel)classTypes[0]).Methods;

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
                Assert.Equal("bool", attribute1.ParameterTypes[1].Type.Name);

                var attribute2 = attributeTypes[1];
                Assert.Equal("System.SerializableAttribute", attribute2.Name);
                Assert.Empty(attribute2.ParameterTypes);

                var attribute3 = attributeTypes[2];
                Assert.Equal("System.AttributeUsageAttribute", attribute3.Name);
                Assert.Equal(1, attribute3.ParameterTypes.Count);
                Assert.Equal("System.AttributeTargets", attribute3.ParameterTypes[0].Type.Name);
            }

            Assert.Equal("Namespace1.Class1.Method(double, float)",
                ((ClassModel)classTypes[0]).Methods[0].Attributes[0].ContainingTypeName);
            Assert.Equal("Namespace1.Class1.Method(double, float, double)",
                ((ClassModel)classTypes[0]).Methods[1].Attributes[0].ContainingTypeName);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/Attributes/MethodWithCustomAttribute.txt")]
        public void Extract_ShouldExtractAttribute_WhenProvidedWithCustomAttribute(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classType = (ClassModel)classTypes[1];

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

            Assert.Equal("MyNamespace.MyClass.Method1(int, int)",
                classType.Methods[0].Attributes[0].ContainingTypeName);
            Assert.Equal("MyNamespace.MyClass.Method2(int)", classType.Methods[1].Attributes[0].ContainingTypeName);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/Attributes/MethodWithExternAttribute.txt")]
        public void Extract_ShouldExtractAttribute_WhenProvidedWithExternAttribute(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classType = (ClassModel)classTypes[0];

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

            Assert.Equal("Namespace1.Class1.Method()", classType.Methods[0].Attributes[0].ContainingTypeName);
            Assert.Equal("Namespace1.Class1.Sum(int, int)", classType.Methods[1].Attributes[0].ContainingTypeName);
        }
    }
}
