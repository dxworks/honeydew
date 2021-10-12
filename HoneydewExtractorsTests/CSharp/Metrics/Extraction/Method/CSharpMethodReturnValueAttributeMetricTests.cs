using System.Collections.Generic;
using HoneydewCore.Logging;
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
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.Method
{
    public class CSharpMethodReturnValueAttributeMetricTests
    {
        private readonly CSharpFactExtractor _factExtractor;
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
        private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

        public CSharpMethodReturnValueAttributeMetricTests()
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
    using System;
    using System.Diagnostics.CodeAnalysis;

    public {classType} Class1 
    {{
        [return: NotNull]
        public static int? Method1()
        {{
            return 2;
        }}

         [return: NotNull]
        public static string? Method1(int a)
        {{
            return ""aa"";
        }}
    }}
}}";

            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(2, classModel.Methods.Count);

            foreach (var methodType in classModel.Methods)
            {
                var attributeTypes = methodType.ReturnValue.Attributes;

                Assert.Equal(1, attributeTypes.Count);
                Assert.Equal("return", attributeTypes[0].Target);
                Assert.Equal("System.Diagnostics.CodeAnalysis.NotNullAttribute", attributeTypes[0].Name);
                Assert.Empty(attributeTypes[0].ParameterTypes);
            }

            Assert.Equal("Namespace1.Class1.Method1()",
                classModel.Methods[0].ReturnValue.Attributes[0].ContainingTypeName);
            Assert.Equal("Namespace1.Class1.Method1(int)",
                classModel.Methods[1].ReturnValue.Attributes[0].ContainingTypeName);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/ReturnValueAttributes/MethodReturnValueWithCustomAttribute.txt")]
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
                var methodTypeAttributes = methodType.ReturnValue.Attributes;
                foreach (var attribute in methodTypeAttributes)
                {
                    Assert.Equal("return", attribute.Target);
                    Assert.Equal("MyNamespace.MyAttribute", attribute.Name);
                }
            }

            var method1 = classType.Methods[0];
            Assert.Empty(method1.Attributes);

            var method1ReturnValueAttributes = method1.ReturnValue.Attributes;
            Assert.Equal(4, method1ReturnValueAttributes.Count);

            foreach (var attributeType in method1ReturnValueAttributes)
            {
                Assert.Equal("MyNamespace.MyClass.Method1(int, int)", attributeType.ContainingTypeName);
            }

            var attribute1 = method1ReturnValueAttributes[0];
            Assert.Equal(1, attribute1.ParameterTypes.Count);
            Assert.Equal("string", attribute1.ParameterTypes[0].Type.Name);

            var attribute2 = method1ReturnValueAttributes[1];
            Assert.Empty(attribute2.ParameterTypes);

            var attribute3 = method1ReturnValueAttributes[2];
            Assert.Equal(1, attribute3.ParameterTypes.Count);
            Assert.Equal("string", attribute3.ParameterTypes[0].Type.Name);

            var attribute4 = method1ReturnValueAttributes[3];
            Assert.Empty(attribute4.ParameterTypes);


            var method2 = classType.Methods[1];
            Assert.Empty(method2.Attributes);

            var method2ReturnValueAttributes = method2.ReturnValue.Attributes;
            Assert.Equal(1, method2ReturnValueAttributes.Count);

            var attribute5 = method2ReturnValueAttributes[0];
            Assert.Equal(1, attribute5.ParameterTypes.Count);
            Assert.Equal("int", attribute5.ParameterTypes[0].Type.Name);
            Assert.Equal("MyNamespace.MyClass.Method2()", attribute5.ContainingTypeName);


            var method3 = classType.Methods[2];
            Assert.Equal(1, method3.Attributes.Count);
            Assert.Equal("method", method3.Attributes[0].Target);
            Assert.Equal("System.ObsoleteAttribute", method3.Attributes[0].Name);

            var method3ReturnValueAttributes = method3.ReturnValue.Attributes;
            Assert.Equal(4, method3ReturnValueAttributes.Count);

            foreach (var attributeType in method3ReturnValueAttributes)
            {
                Assert.Equal("MyNamespace.MyClass.Method3(int)", attributeType.ContainingTypeName);
            }

            var attribute6 = method3ReturnValueAttributes[0];
            Assert.Equal(1, attribute6.ParameterTypes.Count);
            Assert.Equal("string", attribute6.ParameterTypes[0].Type.Name);

            var attribute7 = method3ReturnValueAttributes[1];
            Assert.Empty(attribute7.ParameterTypes);

            var attribute8 = method3ReturnValueAttributes[2];
            Assert.Equal(2, attribute8.ParameterTypes.Count);
            Assert.Equal("string", attribute8.ParameterTypes[0].Type.Name);
            Assert.Equal("int", attribute8.ParameterTypes[1].Type.Name);

            var attribute9 = method3ReturnValueAttributes[3];
            Assert.Empty(attribute9.ParameterTypes);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/ReturnValueAttributes/MethodReturnValueWithExternAttribute.txt")]
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
                var methodTypeAttributes = methodType.ReturnValue.Attributes;
                foreach (var attribute in methodTypeAttributes)
                {
                    Assert.Equal("return", attribute.Target);
                    Assert.Equal("ExternAttribute", attribute.Name);
                }
            }

            var method1 = classType.Methods[0];
            Assert.Empty(method1.Attributes);

            var method1ReturnValueAttributes = method1.ReturnValue.Attributes;
            Assert.Equal(4, method1ReturnValueAttributes.Count);

            foreach (var attributeType in method1ReturnValueAttributes)
            {
                Assert.Equal("MyNamespace.MyClass.Method1(int, int)", attributeType.ContainingTypeName);
            }

            var attribute1 = method1ReturnValueAttributes[0];
            Assert.Equal(1, attribute1.ParameterTypes.Count);
            Assert.Equal("System.String", attribute1.ParameterTypes[0].Type.Name);

            var attribute2 = method1ReturnValueAttributes[1];
            Assert.Empty(attribute2.ParameterTypes);

            var attribute3 = method1ReturnValueAttributes[2];
            Assert.Equal(1, attribute3.ParameterTypes.Count);
            Assert.Equal("System.String", attribute3.ParameterTypes[0].Type.Name);

            var attribute4 = method1ReturnValueAttributes[3];
            Assert.Empty(attribute4.ParameterTypes);


            var method2 = classType.Methods[1];
            Assert.Empty(method2.Attributes);

            var method2ReturnValueAttributes = method2.ReturnValue.Attributes;
            Assert.Equal(1, method2ReturnValueAttributes.Count);

            var attribute5 = method2ReturnValueAttributes[0];
            Assert.Equal(1, attribute5.ParameterTypes.Count);
            Assert.Equal("System.Int32", attribute5.ParameterTypes[0].Type.Name);
            Assert.Equal("MyNamespace.MyClass.Method2()", attribute5.ContainingTypeName);


            var method3 = classType.Methods[2];
            Assert.Equal(1, method3.Attributes.Count);
            Assert.Equal("method", method3.Attributes[0].Target);
            Assert.Equal("ExternAttribute", method3.Attributes[0].Name);

            var method3ReturnValueAttributes = method3.ReturnValue.Attributes;
            Assert.Equal(4, method3ReturnValueAttributes.Count);

            foreach (var attributeType in method3ReturnValueAttributes)
            {
                Assert.Equal("MyNamespace.MyClass.Method3(int)", attributeType.ContainingTypeName);
            }

            var attribute6 = method3ReturnValueAttributes[0];
            Assert.Equal(1, attribute6.ParameterTypes.Count);
            Assert.Equal("System.String", attribute6.ParameterTypes[0].Type.Name);

            var attribute7 = method3ReturnValueAttributes[1];
            Assert.Empty(attribute7.ParameterTypes);

            var attribute8 = method3ReturnValueAttributes[2];
            Assert.Equal(2, attribute8.ParameterTypes.Count);
            Assert.Equal("System.String", attribute8.ParameterTypes[0].Type.Name);
            Assert.Equal("System.Int32", attribute8.ParameterTypes[1].Type.Name);

            var attribute9 = method3ReturnValueAttributes[3];
            Assert.Empty(attribute9.ParameterTypes);
        }
    }
}
