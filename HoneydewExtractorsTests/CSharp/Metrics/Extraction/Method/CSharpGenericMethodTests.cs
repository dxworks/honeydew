using System.Collections.Generic;
using HoneydewCore.Logging;
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
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method.LocalFunctions;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.Method
{
    public class CSharpGenericMethodTests
    {
        private readonly CSharpFactExtractor _factExtractor;
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
        private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

        public CSharpGenericMethodTests()
        {
            var compositeVisitor = new CompositeVisitor();

            var genericParameterSetterVisitor = new GenericParameterSetterVisitor(new List<IGenericParameterVisitor>
            {
                new GenericParameterInfoVisitor()
            });
            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
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
                }),
            }));

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _factExtractor = new CSharpFactExtractor(compositeVisitor);
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

            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

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

            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

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
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

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
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

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


        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/GenericExtraction/GenericLocalFunctionWithPredefinedConstrains.txt")]
        public void Extract_ShouldHaveLocalFunctionGenericMethodWithConstraints_WhenProvidedWithClass(
            string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            var methodModel = (MethodModel)classModel.Methods[0];

            var localFunctions = new[]
            {
                methodModel.LocalFunctions[0],
                methodModel.LocalFunctions[0].LocalFunctions[0]
            };

            Assert.Equal("LocalFunction", localFunctions[0].Name);
            Assert.Equal("LocalFunction2", localFunctions[1].Name);

            for (var i = 0; i < localFunctions.Length; i++)
            {
                var localFunction = localFunctions[i];
                Assert.Equal(4, localFunction.GenericParameters.Count);
                Assert.Equal("T" + (i + 1), localFunction.GenericParameters[0].Name);
                Assert.Equal(1, localFunction.GenericParameters[0].Constraints.Count);
                Assert.Equal("struct", localFunction.GenericParameters[0].Constraints[0].Name);

                Assert.Equal("TK" + (i + 1), localFunction.GenericParameters[1].Name);
                Assert.Equal(1, localFunction.GenericParameters[1].Constraints.Count);
                Assert.Equal("class?", localFunction.GenericParameters[1].Constraints[0].Name);

                Assert.Equal("TR" + (i + 1), localFunction.GenericParameters[2].Name);
                Assert.Equal(1, localFunction.GenericParameters[2].Constraints.Count);
                Assert.Equal("notnull", localFunction.GenericParameters[2].Constraints[0].Name);

                Assert.Equal("TP" + (i + 1), localFunction.GenericParameters[3].Name);
                Assert.Equal(1, localFunction.GenericParameters[3].Constraints.Count);
                Assert.Equal($"Namespace1.IInterface2<T{i + 1}, Namespace1.IInterface2<T{i + 1}, TK{i + 1}>>",
                    localFunction.GenericParameters[3].Constraints[0].Name);
                Assert.Equal("Namespace1.IInterface2", localFunction.GenericParameters[3].Constraints[0].FullType.Name);
                Assert.Equal(2, localFunction.GenericParameters[3].Constraints[0].FullType.ContainedTypes.Count);
                Assert.Equal("T" + (i + 1),
                    localFunction.GenericParameters[3].Constraints[0].FullType.ContainedTypes[0].Name);
                Assert.Equal("Namespace1.IInterface2",
                    localFunction.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].Name);
                Assert.Equal(2,
                    localFunction.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].ContainedTypes.Count);
                Assert.Equal("T" + (i + 1),
                    localFunction.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].ContainedTypes[0]
                        .Name);
                Assert.Equal("TK" + (i + 1),
                    localFunction.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].ContainedTypes[1]
                        .Name);
            }
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/GenericExtraction/GenericLocalFunctionWithMultipleConstrains.txt")]
        public void Extract_ShouldHaveGenericLocalMethodWithMultipleConstrains_WhenProvidedWithClass(
            string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            var methodModel = (MethodModel)classModel.Methods[0];

            var localFunctions = new[]
            {
                methodModel.LocalFunctions[0],
                methodModel.LocalFunctions[0].LocalFunctions[0]
            };

            Assert.Equal("LocalFunction", localFunctions[0].Name);
            Assert.Equal("LocalFunction2", localFunctions[1].Name);


            for (var i = 0; i < localFunctions.Length; i++)
            {
                var localFunction = localFunctions[i];
                Assert.Equal(3, localFunction.GenericParameters.Count);
                Assert.Equal("T" + (i + 1), localFunction.GenericParameters[0].Name);
                Assert.Equal(2, localFunction.GenericParameters[0].Constraints.Count);
                Assert.Equal("Namespace1.IInterface", localFunction.GenericParameters[0].Constraints[0].Name);
                Assert.Equal($"Namespace1.IInterface2<TK{i + 1}, TR{i + 1}>",
                    localFunction.GenericParameters[0].Constraints[1].Name);
                Assert.Equal("Namespace1.IInterface2", localFunction.GenericParameters[0].Constraints[1].FullType.Name);
                Assert.Equal(2, localFunction.GenericParameters[0].Constraints[1].FullType.ContainedTypes.Count);
                Assert.Equal("TK" + (i + 1),
                    localFunction.GenericParameters[0].Constraints[1].FullType.ContainedTypes[0].Name);
                Assert.Equal("TR" + (i + 1),
                    localFunction.GenericParameters[0].Constraints[1].FullType.ContainedTypes[1].Name);
            }
        }
    }
}
