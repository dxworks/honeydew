using System.Collections.Generic;
using HoneydewCore.IO.Writers.Exporters;
using HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp;
using HoneydewModels;
using Xunit;

namespace HoneydewCoreTest.IO.Writers.Exporters
{
    public class SolutionModelJsonExporterTests
    {
        private readonly ISolutionModelExporter _sut;

        public SolutionModelJsonExporterTests()
        {
            _sut = new JsonModelExporter();
        }

        [Fact]
        public void Export_ShouldReturnRawModel_WhenModelHasNoCompilationUnits()
        {
            var solutionModel = new SolutionModel();
            const string expectedString = @"{""FilePath"":null,""Projects"":[]}";

            var exportString = _sut.Export(solutionModel);

            Assert.Equal(expectedString, exportString);
        }

        [Fact]
        public void Export_ShouldReturnRawModel_WhenModelHasOneCompilationUnitWithOneClassAndNoMetrics()
        {
            var solutionModel = new SolutionModel {FilePath = "path_to_solution"};
            var classModels = new List<ClassModel>
            {
                new()
                {
                    FilePath = "pathToClass",
                    FullName = "SomeNamespace.FirstClass",
                    ClassType = "class",
                    AccessModifier = "public",
                    Modifier = "sealed"
                }
            };

            const string expectedString =
                @"{""FilePath"":""path_to_solution"",""Projects"":[{""Name"":""A Project"",""FilePath"":""some_other_path"",""ProjectReferences"":[],""Namespaces"":[{""Name"":""SomeNamespace"",""ClassModels"":[{""ClassType"":""class"",""FilePath"":""pathToClass"",""FullName"":""SomeNamespace.FirstClass"",""AccessModifier"":""public"",""Modifier"":""sealed"",""BaseClassFullName"":""object"",""BaseInterfaces"":[],""Fields"":[],""Constructors"":[],""Methods"":[],""Metrics"":[],""Namespace"":""SomeNamespace""}]}]}]}";

            var projectModel = new ProjectModel("A Project")
            {
                FilePath = "some_other_path"
            };
            foreach (var classModel in classModels)
            {
                projectModel.Add(classModel);
            }

            solutionModel.Projects.Add(projectModel);

            var exportString = _sut.Export(solutionModel);

            Assert.Equal(expectedString, exportString);
        }

        [Fact]
        public void Export_ShouldReturnRawModel_WhenModelHasOneCompilationUnitWithOneClassAndMetrics()
        {
            var solutionModel = new SolutionModel {FilePath = "path_to_solution"};

            var classModel = new ClassModel
            {
                FilePath = "SomePath",
                FullName = "SomeNamespace.FirstClass",
                ClassType = "class",
                AccessModifier = "private",
                Modifier = "static"
            };

            classModel.Metrics.Add(new ClassMetric()
            {
                ExtractorName = typeof(CSharpBaseClassMetric).FullName,
                ValueType = typeof(CSharpInheritanceMetric).FullName,
                Value = new CSharpInheritanceMetric {Interfaces = {"Interface1"}, BaseClassName = "SomeParent"}
            });

            var classModels = new List<ClassModel>
            {
                classModel
            };

            const string expectedString =
                @"{""FilePath"":""path_to_solution"",""Projects"":[{""Name"":""ProjectName"",""FilePath"":""some_path"",""ProjectReferences"":[""HoneydewCore""],""Namespaces"":[{""Name"":""SomeNamespace"",""ClassModels"":[{""ClassType"":""class"",""FilePath"":""SomePath"",""FullName"":""SomeNamespace.FirstClass"",""AccessModifier"":""private"",""Modifier"":""static"",""BaseClassFullName"":""object"",""BaseInterfaces"":[],""Fields"":[],""Constructors"":[],""Methods"":[],""Metrics"":[{""ExtractorName"":""HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp.CSharpBaseClassMetric"",""ValueType"":""HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp.CSharpInheritanceMetric"",""Value"":{""Interfaces"":[""Interface1""],""BaseClassName"":""SomeParent""}}],""Namespace"":""SomeNamespace""}]}]}]}";

            var projectModel = new ProjectModel("ProjectName")
            {
                FilePath = "some_path",
                ProjectReferences = {"HoneydewCore"}
            };
            foreach (var model in classModels)
            {
                projectModel.Add(model);
            }

            solutionModel.Projects.Add(projectModel);

            var exportString = _sut.Export(solutionModel);

            Assert.Equal(expectedString, exportString);
        }

        [Fact]
        public void Export_ShouldReturnRawModel_WhenModelHasOneCompilationUnitWithOneClassAndMethodCalls()
        {
            var solutionModel = new SolutionModel {FilePath = "path_to_solution"};
            var classModels = new List<ClassModel>
            {
                new()
                {
                    FilePath = "pathToClass",
                    FullName = "SomeNamespace.FirstClass",
                    ClassType = "class",
                    AccessModifier = "protected",

                    Methods = new List<MethodModel>
                    {
                        new()
                        {
                            Name = "Method1",
                            ReturnType = "int",
                            Modifier = "static",
                            AccessModifier = "public",
                            ContainingClassName = "SomeNamespace.FirstClass",
                            CalledMethods =
                            {
                                new MethodCallModel
                                {
                                    MethodName = "Parse",
                                    ContainingClassName = "int",
                                    ParameterTypes =
                                    {
                                        new ParameterModel
                                        {
                                            Type = "string"
                                        }
                                    },
                                }
                            },
                            ParameterTypes =
                            {
                                new ParameterModel
                                {
                                    Type = "string"
                                }
                            }
                        }
                    }
                }
            };

            const string expectedString =
                @"{""FilePath"":""path_to_solution"",""Projects"":[{""Name"":""A Project"",""FilePath"":""some_path"",""ProjectReferences"":[],""Namespaces"":[{""Name"":""SomeNamespace"",""ClassModels"":[{""ClassType"":""class"",""FilePath"":""pathToClass"",""FullName"":""SomeNamespace.FirstClass"",""AccessModifier"":""protected"",""Modifier"":"""",""BaseClassFullName"":""object"",""BaseInterfaces"":[],""Fields"":[],""Constructors"":[],""Methods"":[{""Name"":""Method1"",""IsConstructor"":false,""ReturnType"":""int"",""Modifier"":""static"",""AccessModifier"":""public"",""ParameterTypes"":[{""Type"":""string"",""Modifier"":"""",""DefaultValue"":null}],""ContainingClassName"":""SomeNamespace.FirstClass"",""CalledMethods"":[{""MethodName"":""Parse"",""ContainingClassName"":""int"",""ParameterTypes"":[{""Type"":""string"",""Modifier"":"""",""DefaultValue"":null}]}]}],""Metrics"":[],""Namespace"":""SomeNamespace""}]}]}]}";

            var projectModel = new ProjectModel("A Project")
            {
                FilePath = "some_path"
            };
            foreach (var classModel in classModels)
            {
                projectModel.Add(classModel);
            }

            solutionModel.Projects.Add(projectModel);

            var exportString = _sut.Export(solutionModel);

            Assert.Equal(expectedString, exportString);
        }
    }
}
