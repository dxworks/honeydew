using System.Collections.Generic;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.Extractors.Metrics.SyntacticMetrics;
using HoneydewCore.IO.Writers;
using HoneydewCore.Models;
using Xunit;

namespace HoneydewCoreTest.IO.Writers
{
    public class SolutionModelRawExporterTests
    {
        private readonly ISolutionModelExporter _sut;

        public SolutionModelRawExporterTests()
        {
            _sut = new RawModelExporter();
        }

        [Fact]
        public void Export_ShouldReturnRawModel_WhenModelHasNoCompilationUnits()
        {
            var solutionModel = new SolutionModel();
            const string expectedString = "{\"Projects\":null,\"ProjectClassModels\":[]}";
            
            var exportString = solutionModel.Export(_sut);

            Assert.Equal(expectedString, exportString);
        }

        [Fact]
        public void Export_ShouldReturnRawModel_WhenModelHasOneCompilationUnitWithOneClassAndNoMetrics()
        {
            var solutionModel = new SolutionModel();
            var compilationUnitModels = new CompilationUnitModel();
            compilationUnitModels.ClassModels.Add(new ClassModel
            {
                Name = "FirstClass",
                Namespace = "SomeNamespace"
            });
            const string path = "pathToClass";


            const string expectedString =
                "{\"Projects\":null,\"ProjectClassModels\":[{\"Model\":{\"Metrics\":{\"MetricValues\":{}},\"Name\":\"FirstClass\",\"Namespace\":\"SomeNamespace\"},\"Path\":\"pathToClass\"}]}";

            solutionModel.Add(new List<CompilationUnitModel>()
            {
                compilationUnitModels
            }, path);

            var exportString = solutionModel.Export(_sut);

            Assert.Equal(expectedString, exportString);
        }

        [Fact]
        public void Export_ShouldReturnRawModel_WhenModelHasOneCompilationUnitWithOneClassAndMetrics()
        {
            var solutionModel = new SolutionModel();
            var compilationUnitModels = new CompilationUnitModel();

            var classModel = new ClassModel
            {
                Name = "FirstClass",
                Namespace = "SomeNamespace"
            };
            var baseClassMetric = new BaseClassMetric
            {
                InheritanceMetric = new InheritanceMetric {Interfaces = {"Interface1"}, BaseClassName = "Object"}
            };

            classModel.Metrics.Add(baseClassMetric);

            compilationUnitModels.ClassModels.Add(classModel);
            compilationUnitModels.SyntacticMetrics.Add(new UsingsCountMetric()
            {
                
            });

            const string path = "pathToClass";

            const string expectedString =
                @"{""Projects"":null,""ProjectClassModels"":[{""Model"":{""Metrics"":{""MetricValues"":{""HoneydewCore.Extractors.Metrics.SemanticMetrics.BaseClassMetric"":{""Value"":{""Interfaces"":[""Interface1""],""BaseClassName"":""Object""}}}},""Name"":""FirstClass"",""Namespace"":""SomeNamespace""},""Path"":""pathToClass""}]}";


            solutionModel.Add(new List<CompilationUnitModel>
            {
                compilationUnitModels
            }, path);

            
            var exportString = solutionModel.Export(_sut);

            Assert.Equal(expectedString, exportString);
        }
    }
}