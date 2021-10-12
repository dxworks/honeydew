using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Constructor;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Extraction.Parameter;
using HoneydewExtractors.CSharp.Metrics.Iterators;
using HoneydewModels.Types;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel.RelationMetric
{
    public class CSharpParameterRelationMetricTests
    {
        private readonly ParameterRelationVisitor _sut;
        private readonly CSharpFactExtractor _factExtractor;
        private readonly ClassTypePropertyIterator _classTypePropertyIterator;
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
        private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

        public CSharpParameterRelationMetricTests()
        {
            _sut = new ParameterRelationVisitor();

            var compositeVisitor = new CompositeVisitor();

            var parameterSetterVisitor = new ParameterSetterVisitor(new List<IParameterVisitor>
            {
                new ParameterInfoVisitor()
            });
            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new MethodSetterClassVisitor(new List<IMethodVisitor>
                {
                    new MethodInfoVisitor(),
                    parameterSetterVisitor
                }),
                new ConstructorSetterClassVisitor(new List<IConstructorVisitor>
                {
                    new ConstructorInfoVisitor(),
                    parameterSetterVisitor
                })
            }));

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _factExtractor = new CSharpFactExtractor(compositeVisitor);

            _classTypePropertyIterator = new ClassTypePropertyIterator(new List<IModelVisitor<IClassType>>
            {
                _sut
            });
        }

        [Fact]
        public void PrettyPrint_ShouldReturnParameterDependency()
        {
            Assert.Equal("Parameter Dependency", _sut.PrettyPrint());
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/Relations/ClassWithTwoFunctions.txt")]
        public void Extract_ShouldHaveNoParameters_WhenClassHasMethodsWithNoParameters(string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            foreach (var model in classTypes)
            {
                _classTypePropertyIterator.Iterate(model);
            }

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.ParameterRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dictionary = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;
            Assert.Empty(dictionary);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Relations/ParameterRelations/ClassWithNoArgConstructor.txt")]
        public void Extract_ShouldHaveNoParameters_WhenClassHasConstructorWithNoParameters(string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            foreach (var model in classTypes)
            {
                _classTypePropertyIterator.Iterate(model);
            }

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.ParameterRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dictionary = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;
            Assert.Empty(dictionary);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Relations/ParameterRelations/ClassWithMethodsWithPrimitiveParams.txt")]
        public void Extract_ShouldHavePrimitiveParameters_WhenClassHasMethodsWithPrimitiveParameters(
            string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            foreach (var model in classTypes)
            {
                _classTypePropertyIterator.Iterate(model);
            }

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.ParameterRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(3, dependencies["int"]);
            Assert.Equal(2, dependencies["float"]);
            Assert.Equal(1, dependencies["string"]);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Relations/ParameterRelations/InterfaceWithMethodsWithPrimitiveParams.txt")]
        public void Extract_ShouldHavePrimitiveParameters_WhenInterfaceHasMethodsWithPrimitiveParameters(
            string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            foreach (var model in classTypes)
            {
                _classTypePropertyIterator.Iterate(model);
            }

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.ParameterRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(3, dependencies["int"]);
            Assert.Equal(2, dependencies["float"]);
            Assert.Equal(1, dependencies["string"]);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Relations/ParameterRelations/InterfaceWithMethodsWithNonPrimitiveParams.txt")]
        public void Extract_ShouldHaveDependenciesParameters_WhenInterfaceHasMethodsWithDependenciesParameters(
            string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            foreach (var model in classTypes)
            {
                _classTypePropertyIterator.Iterate(model);
            }

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.ParameterRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(2, dependencies["CSharpMetricExtractor"]);
            Assert.Equal(1, dependencies["IFactExtractor"]);
            Assert.Equal(1, dependencies["int"]);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Relations/ParameterRelations/ClassWithMethodsWithNonPrimitiveParams.txt")]
        public void Extract_ShouldHaveDependenciesParameters_WhenClassHasMethodsWithDependenciesParameters(
            string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            foreach (var model in classTypes)
            {
                _classTypePropertyIterator.Iterate(model);
            }

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.ParameterRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(4, dependencies.Count);
            Assert.Equal(2, dependencies["CSharpMetricExtractor"]);
            Assert.Equal(1, dependencies["IFactExtractor"]);
            Assert.Equal(2, dependencies["int"]);
            Assert.Equal(1, dependencies["string"]);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Relations/ParameterRelations/ClassWithConstructorsWithNonPrimitiveParams.txt")]
        public void Extract_ShouldHaveDependenciesParameters_WhenClassHasConstructorWithDependenciesParameters(
            string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            foreach (var model in classTypes)
            {
                _classTypePropertyIterator.Iterate(model);
            }

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.ParameterRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(4, dependencies.Count);
            Assert.Equal(2, dependencies["CSharpMetricExtractor"]);
            Assert.Equal(1, dependencies["IFactExtractor"]);
            Assert.Equal(2, dependencies["int"]);
            Assert.Equal(1, dependencies["string"]);
        }
    }
}
