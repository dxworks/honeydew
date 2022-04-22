using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using HoneydewCore.Logging;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Class.Attributes;

public class CSharpAttributeForGenericTypesTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public CSharpAttributeForGenericTypesTests()
    {
        var compositeVisitor = new CompositeVisitor(_loggerMock.Object);

        var genericParameterSetterVisitor = new GenericParameterSetterVisitor(_loggerMock.Object,
            new List<IGenericParameterVisitor>
            {
                new GenericParameterInfoVisitor(),
                new AttributeSetterVisitor(_loggerMock.Object, new List<IAttributeVisitor>
                {
                    new AttributeInfoVisitor()
                })
            });
        compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(_loggerMock.Object, new List<ICSharpClassVisitor>
        {
            new BaseInfoClassVisitor(),
            genericParameterSetterVisitor,
            new MethodSetterClassVisitor(_loggerMock.Object, new List<IMethodVisitor>
            {
                new MethodInfoVisitor(),
                genericParameterSetterVisitor,
                new LocalFunctionsSetterClassVisitor(_loggerMock.Object, new List<ILocalFunctionVisitor>
                {
                    new LocalFunctionInfoVisitor(_loggerMock.Object, new List<ILocalFunctionVisitor>
                    {
                        genericParameterSetterVisitor
                    }),
                    genericParameterSetterVisitor
                })
            })
        }));
        compositeVisitor.Add(new DelegateSetterCompilationUnitVisitor(_loggerMock.Object, new List<IDelegateVisitor>
        {
            new BaseInfoDelegateVisitor(),
            genericParameterSetterVisitor
        }));

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("record")]
    [InlineData("struct")]
    [InlineData("interface")]
    public void Extract_ShouldHaveAttributesToGenericParameters_WhenProvidedWithTypesThatSupportGenericParameters(
        string classType)
    {
        var fileContent = $@"namespace Namespace1
{{
    using System;

    public {classType} Class1<
        [System.ComponentModel.TypeConverter] [My] [Extern]
        T,
        [System.ComponentModel.TypeConverter, My, Extern]
        TK>
    {{
        public TP Method<[System.ComponentModel.TypeConverter] [My] [Extern] TP>(TP tp)
        {{
            TL1 LocalFunction<[System.ComponentModel.TypeConverter, My, Extern] TL1>(TL1 tl1)
            {{
                TL2 LocalFunction2<[System.ComponentModel.TypeConverter, My, Extern] TL2>(TL2 tl2)
                {{
                    return tl2;
                }}
                
                return tl1;
            }}

            return tp;
        }}
    }}

    public delegate T Delegate1<[System.ComponentModel.TypeConverter] [My] [Extern] T>();

    [AttributeUsage(AttributeTargets.GenericParameter, AllowMultiple = true)]
    public class MyAttribute : Attribute
    {{
    }}
}}
";

        var syntacticModelCreator = new CSharpSyntacticModelCreator();
        var semanticModelCreator = new CSharpSemanticModelCreator(new CSharpCompilationMaker());
        var syntaxTree = syntacticModelCreator.Create(fileContent);
        var semanticModel = semanticModelCreator.Create(syntaxTree);

        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        var methodModel = (MethodModel)classModel.Methods[0];
        var genericParameters = new[]
        {
            classModel.GenericParameters[0],
            classModel.GenericParameters[1],
            methodModel.GenericParameters[0],
            methodModel.LocalFunctions[0].GenericParameters[0],
            methodModel.LocalFunctions[0].LocalFunctions[0].GenericParameters[0],
            (classTypes[2] as ITypeWithGenericParameters)?.GenericParameters[0],
        };

        foreach (var genericParameter in genericParameters)
        {
            Assert.Equal(3, genericParameter!.Attributes.Count);

            Assert.Equal("param", genericParameter.Attributes[0].Target);
            Assert.Equal("System.ComponentModel.TypeConverterAttribute", genericParameter.Attributes[0].Name);
            Assert.Equal("Namespace1.MyAttribute", genericParameter.Attributes[1].Name);
            Assert.Equal("Extern", genericParameter.Attributes[2].Name);
        }
    }
}
