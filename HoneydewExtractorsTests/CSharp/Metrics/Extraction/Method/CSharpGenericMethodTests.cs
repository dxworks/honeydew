using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Extraction.Parameter;
using HoneydewModels.CSharp;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.Method
{
    public class CSharpGenericMethodTests
    {
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpGenericMethodTests()
        {
            var compositeVisitor = new CompositeVisitor();

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new MethodSetterClassVisitor(new List<IMethodVisitor>
                {
                    new MethodInfoVisitor(),
                    new GenericParameterSetterVisitor(new List<IGenericParameterVisitor>
                    {
                        new GenericParameterInfoVisitor()
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
        [InlineData("interface")]
        public void Extract_ShouldHaveGenericMethod_WhenProvidedDifferentNonGenericClassType(string classType)
        {
            var fileContent = $@"namespace Namespace1
{{
    public {classType} Class1 
    {{
        public T Method<T>(T t) {{ return t; }} 
    }}
}}";
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal("Namespace1.Class1", classModel.Name);
            Assert.Equal("Method", classModel.Methods[0].Name);
            Assert.Equal(1, classModel.Methods[0].GenericParameters.Count);
            Assert.Equal("T", classModel.Methods[0].GenericParameters[0].Name);
            Assert.Equal("", classModel.Methods[0].GenericParameters[0].Modifier);
            Assert.Empty(classModel.Methods[0].GenericParameters[0].Constraints);
        }

        [Theory]
        [InlineData("class")]
        [InlineData("record")]
        [InlineData("struct")]
        [InlineData("interface")]
        public void Extract_ShouldHaveGenericMethodWithMultipleGenericParams_WhenProvidedDifferentNonGenericClassType(
            string classType)
        {
            var fileContent = $@"namespace Namespace1
{{
    public {classType} Class1
    {{
        public T Method<T, R> (R r) {{ return default; }}
    }}
}}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal("Method", classModel.Methods[0].Name);
            Assert.Equal(2, classModel.Methods[0].GenericParameters.Count);
            Assert.Equal("T", classModel.Methods[0].GenericParameters[0].Name);
            Assert.Equal("R", classModel.Methods[0].GenericParameters[1].Name);
        }


        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/GenericExtraction/GenericMethodWithPredefinedConstrains.txt")]
        public void Extract_ShouldHaveGenericTypesWithPredefinedConstrains_WhenProvidedWithClass(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            var methodModel = classModel.Methods[0];

            Assert.Equal("Method", methodModel.Name);
            Assert.Equal(4, methodModel.GenericParameters.Count);
            Assert.Equal("T", methodModel.GenericParameters[0].Name);
            Assert.Equal(1, methodModel.GenericParameters[0].Constraints.Count);
            Assert.Equal("struct", methodModel.GenericParameters[0].Constraints[0].Name);

            Assert.Equal("TK", methodModel.GenericParameters[1].Name);
            Assert.Equal(1, methodModel.GenericParameters[1].Constraints.Count);
            Assert.Equal("class?", methodModel.GenericParameters[1].Constraints[0].Name);

            Assert.Equal("TR", methodModel.GenericParameters[2].Name);
            Assert.Equal(1, methodModel.GenericParameters[2].Constraints.Count);
            Assert.Equal("notnull", methodModel.GenericParameters[2].Constraints[0].Name);

            Assert.Equal("TP", methodModel.GenericParameters[3].Name);
            Assert.Equal(1, methodModel.GenericParameters[3].Constraints.Count);
            Assert.Equal("Namespace1.IInterface2<T, Namespace1.IInterface2<T, TK>>",
                methodModel.GenericParameters[3].Constraints[0].Name);
            Assert.Equal("Namespace1.IInterface2", methodModel.GenericParameters[3].Constraints[0].FullType.Name);
            Assert.Equal(2, methodModel.GenericParameters[3].Constraints[0].FullType.ContainedTypes.Count);
            Assert.Equal("T", methodModel.GenericParameters[3].Constraints[0].FullType.ContainedTypes[0].Name);
            Assert.Equal("Namespace1.IInterface2",
                methodModel.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].Name);
            Assert.Equal(2,
                methodModel.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].ContainedTypes.Count);
            Assert.Equal("T",
                methodModel.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].ContainedTypes[0].Name);
            Assert.Equal("TK",
                methodModel.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].ContainedTypes[1].Name);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/GenericExtraction/GenericMethodWithMultipleConstrains.txt")]
        public void Extract_ShouldHaveGenericTypesWithMultipleConstrains_WhenProvidedWithClass(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            var methodModel = classModel.Methods[0];
            
            Assert.Equal("Method", methodModel.Name);
            Assert.Equal(3, methodModel.GenericParameters.Count);
            Assert.Equal("T", methodModel.GenericParameters[0].Name);
            Assert.Equal(2, methodModel.GenericParameters[0].Constraints.Count);
            Assert.Equal("Namespace1.IInterface", methodModel.GenericParameters[0].Constraints[0].Name);
            Assert.Equal("Namespace1.IInterface2<TK, TR>", methodModel.GenericParameters[0].Constraints[1].Name);
            Assert.Equal("Namespace1.IInterface2", methodModel.GenericParameters[0].Constraints[1].FullType.Name);
            Assert.Equal(2, methodModel.GenericParameters[0].Constraints[1].FullType.ContainedTypes.Count);
            Assert.Equal("TK", methodModel.GenericParameters[0].Constraints[1].FullType.ContainedTypes[0].Name);
            Assert.Equal("TR", methodModel.GenericParameters[0].Constraints[1].FullType.ContainedTypes[1].Name);
        }
    }
}
