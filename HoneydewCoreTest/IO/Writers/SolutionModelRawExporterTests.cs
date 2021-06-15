using System.Collections.Generic;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.Extractors.Models;
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
            const string expectedString = "{\"Namespaces\":[]}";

            var exportString = solutionModel.Export(_sut);

            Assert.Equal(expectedString, exportString);
        }

        [Fact]
        public void Export_ShouldReturnRawModel_WhenModelHasOneCompilationUnitWithOneClassAndNoMetrics()
        {
            var solutionModel = new SolutionModel();
            var classModels = new List<ClassModel>
            {
                new()
                {
                    FilePath = "pathToClass",
                    Name = "FirstClass",
                    Namespace = "SomeNamespace",
                }
            };

            const string expectedString =
                @"{""Namespaces"":[{""Name"":""SomeNamespace"",""ClassModels"":[{""Path"":""pathToClass"",""FullName"":""SomeNamespace.FirstClass"",""Metrics"":[]}]}]}";

            foreach (var classModel in classModels)
            {
                solutionModel.Add(classModel);
            }

            var exportString = solutionModel.Export(_sut);

            Assert.Equal(expectedString, exportString);
        }

        [Fact]
        public void Export_ShouldReturnRawModel_WhenModelHasOneCompilationUnitWithOneClassAndMetrics()
        {
            var solutionModel = new SolutionModel();

            var classModel = new ClassModel
            {
                FilePath = "SomePath",
                Name = "FirstClass",
                Namespace = "SomeNamespace"
            };
            var baseClassMetric = new BaseClassMetric
            {
                InheritanceMetric = new InheritanceMetric {Interfaces = {"Interface1"}, BaseClassName = "SomeParent"}
            };

            classModel.Metrics.Add(baseClassMetric);

            var classModels = new List<ClassModel>
            {
                classModel
            };

            const string expectedString =
                @"{""Namespaces"":[{""Name"":""SomeNamespace"",""ClassModels"":[{""Path"":""SomePath"",""FullName"":""SomeNamespace.FirstClass"",""Metrics"":[{""ExtractorName"":""HoneydewCore.Extractors.Metrics.SemanticMetrics.BaseClassMetric"",""ValueType"":""HoneydewCore.Extractors.Metrics.SemanticMetrics.InheritanceMetric"",""Value"":{""Interfaces"":[""Interface1""],""BaseClassName"":""SomeParent""}}]}]}]}";
            
            foreach (var model in classModels)
            {
                solutionModel.Add(model);
            }

            var exportString = solutionModel.Export(_sut);

            Assert.Equal(expectedString, exportString);
        }
    }
}