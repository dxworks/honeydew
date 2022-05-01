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

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Method.ReturnValue;

public class CSharpMethodReturnValueAttributeMetricTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly DotnetSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpMethodReturnValueAttributeMetricTests()
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
                        new CSharpMethodSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
                        {
                            new MethodInfoVisitor(),
                            attributeSetterVisitor,
                            new CSharpReturnValueSetterVisitor(_loggerMock.Object,
                                new List<ITypeVisitor<IReturnValueType>>
                                {
                                    new ReturnValueInfoVisitor(),
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

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal(2, classModel.Methods.Count);

        foreach (var methodType in classModel.Methods)
        {
            var attributeTypes = methodType.ReturnValue.Attributes;

            Assert.Equal(1, attributeTypes.Count);
            Assert.Equal("return", attributeTypes[0].Target);
            Assert.Equal("System.Diagnostics.CodeAnalysis.NotNullAttribute", attributeTypes[0].Name);
            Assert.Empty(attributeTypes[0].ParameterTypes);
        }
    }

    [Theory]
    [FileData("TestData/MethodReturnValueWithCustomAttribute.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithCustomAttribute(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classType = (CSharpClassModel)classTypes[1];

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


        var method3 = classType.Methods[2];
        Assert.Equal(1, method3.Attributes.Count);
        Assert.Equal("method", method3.Attributes[0].Target);
        Assert.Equal("System.ObsoleteAttribute", method3.Attributes[0].Name);

        var method3ReturnValueAttributes = method3.ReturnValue.Attributes;
        Assert.Equal(4, method3ReturnValueAttributes.Count);

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
    [FileData("TestData/MethodReturnValueWithExternAttribute.txt")]
    public void Extract_ShouldExtractAttribute_WhenProvidedWithExternAttribute(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classType = (CSharpClassModel)classTypes[0];

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


        var method3 = classType.Methods[2];
        Assert.Equal(1, method3.Attributes.Count);
        Assert.Equal("method", method3.Attributes[0].Target);
        Assert.Equal("ExternAttribute", method3.Attributes[0].Name);

        var method3ReturnValueAttributes = method3.ReturnValue.Attributes;
        Assert.Equal(4, method3ReturnValueAttributes.Count);

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
