using System.Collections.Generic;
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
using HoneydewExtractors.CSharp.Metrics.Extraction.Delegate;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Extraction.Parameter;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method.LocalFunctions;
using HoneydewModels.CSharp;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.Attributes
{
    public class CSharpAttributeForGenericTypesTests
    {
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpAttributeForGenericTypesTests()
        {
            var compositeVisitor = new CompositeVisitor();

            var genericParameterSetterVisitor = new GenericParameterSetterVisitor(new List<IGenericParameterVisitor>
            {
                new GenericParameterInfoVisitor(),
                new AttributeSetterVisitor(new List<IAttributeVisitor>
                {
                    new AttributeInfoVisitor()
                })
            });
            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                genericParameterSetterVisitor,
                new MethodSetterClassVisitor(new List<IMethodVisitor>
                {
                    new MethodInfoVisitor(),
                    genericParameterSetterVisitor,
                    new LocalFunctionsSetterClassVisitor(new List<ILocalFunctionVisitor>
                    {
                        new LocalFunctionInfoVisitor(new List<ILocalFunctionVisitor>
                        {
                            genericParameterSetterVisitor
                        }),
                        genericParameterSetterVisitor
                    })
                })
            }));
            compositeVisitor.Add(new DelegateSetterCompilationUnitVisitor(new List<IDelegateVisitor>
            {
                new BaseInfoDelegateVisitor(),
                genericParameterSetterVisitor
            }));

            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker()), compositeVisitor);
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
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            var methodModel = (MethodModel)classModel.Methods[0];
            var genericParameters = new[]
            {
                classModel.GenericParameters[0],
                classModel.GenericParameters[1],
                methodModel.GenericParameters[0],
                methodModel.LocalFunctions[0].GenericParameters[0],
                methodModel.LocalFunctions[0].LocalFunctions[0].GenericParameters[0],
                classTypes[2].GenericParameters[0],
            };

            foreach (var genericParameter in genericParameters)
            {
                Assert.Equal(3, genericParameter.Attributes.Count);
                
                Assert.Equal("parameter", genericParameter.Attributes[0].Target);
                Assert.Equal("System.ComponentModel.TypeConverterAttribute", genericParameter.Attributes[0].Name);
                Assert.Equal("Namespace1.MyAttribute", genericParameter.Attributes[1].Name);
                Assert.Equal("Extern", genericParameter.Attributes[2].Name);
            }
        }
    }
}
