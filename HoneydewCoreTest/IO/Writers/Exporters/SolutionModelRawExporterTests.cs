using System.Collections.Generic;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.IO.Writers.Exporters;
using HoneydewCore.Models;
using Xunit;

namespace HoneydewCoreTest.IO.Writers.Exporters
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
            const string expectedString = "{\"Projects\":[]}";

            var exportString = solutionModel.Export(_sut);

            Assert.Equal(expectedString, exportString);
        }

        [Fact(Skip = "Revise Later")]
        public void Export_ShouldReturnRawModel_WhenModelHasOneCompilationUnitWithOneClassAndNoMetrics()
        {
            var solutionModel = new SolutionModel();
            var classModels = new List<ClassModel>
            {
                new()
                {
                    FilePath = "pathToClass",
                    FullName = "SomeNamespace.FirstClass",
                }
            };

            const string expectedString =
                @"{""Projects"":[{""Name"":""A Project"",""Namespaces"":{""SomeNamespace"":{""Name"":""SomeNamespace"",""ClassModels"":[{""FilePath"":""pathToClass"",""FullName"":""SomeNamespace.FirstClass"",""Metrics"":[],""Namespace"":""SomeNamespace""}]}}}]}";

            var projectModel = new ProjectModel("A Project");
            foreach (var classModel in classModels)
            {
                projectModel.Add(classModel);
            }

            solutionModel.Projects.Add(projectModel);

            var exportString = solutionModel.Export(_sut);

            Assert.Equal(expectedString, exportString);
        }

        [Fact(Skip = "Revise Later")]
        public void Export_ShouldReturnRawModel_WhenModelHasOneCompilationUnitWithOneClassAndMetrics()
        {
            var solutionModel = new SolutionModel();

            var classModel = new ClassModel
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

            var classModels = new List<ClassModel>
            {
                classModel
            };

            const string expectedString =
                @"{""Projects"":[{""Name"":""ProjectName"",""Namespaces"":{""SomeNamespace"":{""Name"":""SomeNamespace"",""ClassModels"":[{""FilePath"":""SomePath"",""FullName"":""SomeNamespace.FirstClass"",""Metrics"":[{""ExtractorName"":""HoneydewCore.Extractors.Metrics.SemanticMetrics.BaseClassMetric"",""ValueType"":""HoneydewCore.Extractors.Metrics.SemanticMetrics.InheritanceMetric"",""Value"":{""Interfaces"":[""Interface1""],""BaseClassName"":""SomeParent""}}],""Namespace"":""SomeNamespace""}]}}}]}";

            var projectModel = new ProjectModel("ProjectName");
            foreach (var model in classModels)
            {
                projectModel.Add(model);
            }

            solutionModel.Projects.Add(projectModel);

            var exportString = solutionModel.Export(_sut);

            Assert.Equal(expectedString, exportString);
        }
    }
}