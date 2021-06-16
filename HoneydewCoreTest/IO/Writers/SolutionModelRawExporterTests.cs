using System.Collections.Generic;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
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
            const string expectedString = "{\"Namespaces\":{}}";

            var exportString = solutionModel.Export(_sut);

            Assert.Equal(expectedString, exportString);
        }

        [Fact]
        public void Export_ShouldReturnRawModel_WhenModelHasOneCompilationUnitWithOneClassAndNoMetrics()
        {
            var solutionModel = new SolutionModel();
            var classModels = new List<ProjectClassModel>
            {
                new()
                {
                    FilePath = "pathToClass",
                    FullName = "SomeNamespace.FirstClass",
                }
            };

            const string expectedString =
                @"{""Namespaces"":{""SomeNamespace"":{""Name"":""SomeNamespace"",""ClassModels"":[{""FilePath"":""pathToClass"",""FullName"":""SomeNamespace.FirstClass"",""Metrics"":[],""Namespace"":""SomeNamespace""}]}}}";

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

            var classModel = new ProjectClassModel
            {
                FilePath = "SomePath",
                FullName = "SomeNamespace.FirstClass"
            };

            classModel.Metrics.Add(new ClassMetric()
            {
                ExtractorName = typeof(BaseClassMetric).FullName,
                ValueType = typeof(InheritanceMetric).FullName,
                Value = new InheritanceMetric {Interfaces = {"Interface1"}, BaseClassName = "SomeParent"}
            });

            var classModels = new List<ProjectClassModel>
            {
                classModel
            };

            const string expectedString =
                @"{""Namespaces"":{""SomeNamespace"":{""Name"":""SomeNamespace"",""ClassModels"":[{""FilePath"":""SomePath"",""FullName"":""SomeNamespace.FirstClass"",""Metrics"":[{""ExtractorName"":""HoneydewCore.Extractors.Metrics.SemanticMetrics.BaseClassMetric"",""ValueType"":""HoneydewCore.Extractors.Metrics.SemanticMetrics.InheritanceMetric"",""Value"":{""Interfaces"":[""Interface1""],""BaseClassName"":""SomeParent""}}],""Namespace"":""SomeNamespace""}]}}}";
            
            foreach (var model in classModels)
            {
                solutionModel.Add(model);
            }

            var exportString = solutionModel.Export(_sut);

            Assert.Equal(expectedString, exportString);
        }
    }
}