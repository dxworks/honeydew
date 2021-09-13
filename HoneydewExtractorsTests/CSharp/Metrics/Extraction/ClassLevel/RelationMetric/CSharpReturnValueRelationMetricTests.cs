using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Iterators;
using HoneydewModels.Types;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel.RelationMetric
{
    public class CSharpReturnValueRelationMetricTests
    {
        private readonly ReturnValueRelationVisitor _sut;
        private readonly CSharpFactExtractor _factExtractor;
        private readonly ClassTypePropertyIterator _classTypePropertyIterator;
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
        private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

        public CSharpReturnValueRelationMetricTests()
        {
            _sut = new ReturnValueRelationVisitor();

            var compositeVisitor = new CompositeVisitor();

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new MethodSetterClassVisitor(new List<IMethodVisitor>
                {
                    new MethodInfoVisitor(),
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
        public void PrettyPrint_ShouldReturnReturnValueDependency()
        {
            Assert.Equal("Return Value Dependency", _sut.PrettyPrint());
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/Relations/ClassWithTwoFunctions.txt")]
        public void Extract_ShouldHaveVoidReturnValues_WhenClassHasMethodsThatReturnVoid(string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            foreach (var model in classTypes)
            {
                _classTypePropertyIterator.Iterate(model);
            }

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.ReturnValueRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Single(dependencies);
            Assert.Equal(2, dependencies["void"]);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Relations/ReturnValueRelations/ClassWithMethodsWithPrimitiveReturnValue.txt")]
        public void Extract_ShouldHavePrimitiveReturnValues_WhenClassHasMethodsThatReturnPrimitiveValues(
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
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.ReturnValueRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(2, dependencies["int"]);
            Assert.Equal(1, dependencies["float"]);
            Assert.Equal(1, dependencies["string"]);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Relations/ReturnValueRelations/InterfaceWithMethodsWithPrimitiveReturnValue.txt")]
        public void Extract_ShouldHavePrimitiveReturnValues_WhenInterfaceHasMethodsWithPrimitiveReturnValues(
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
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.ReturnValueRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;
            Assert.Equal(4, dependencies.Count);
            Assert.Equal(1, dependencies["int"]);
            Assert.Equal(1, dependencies["float"]);
            Assert.Equal(1, dependencies["string"]);
            Assert.Equal(1, dependencies["void"]);
        }


        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Relations/ReturnValueRelations/InterfaceWithMethodsWithNonPrimitiveReturnValue.txt")]
        public void Extract_ShouldHaveDependenciesReturnValues_WhenInterfaceHasMethodsWithDependenciesReturnValues(
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
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.ReturnValueRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(2, dependencies.Count);
            Assert.Equal(2, dependencies["CSharpMetricExtractor"]);
            Assert.Equal(1, dependencies["IFactExtractor"]);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Relations/ReturnValueRelations/ClassWithMethodsWithNonPrimitiveReturnValue.txt")]
        public void Extract_ShouldHaveDependenciesReturnValues_WhenClassHasMethodsWithDependenciesReturnValues(
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
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.ReturnValueRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(2, dependencies.Count);
            Assert.Equal(1, dependencies["CSharpMetricExtractor"]);
            Assert.Equal(2, dependencies["IFactExtractor"]);
        }
    }
}
