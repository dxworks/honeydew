using System.Collections.Generic;
using HoneydewModels;
using HoneydewModels.CSharp;
using HoneydewModels.Exporters;
using HoneydewModels.Types;
using Xunit;

namespace HoneydewCoreIntegrationTests.IO.Writers.Exporters
{
    public class SolutionModelJsonExporterTests
    {
        private readonly JsonSolutionModelExporter _sut;

        public SolutionModelJsonExporterTests()
        {
            _sut = new JsonSolutionModelExporter(new ConverterList());
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
            var solutionModel = new SolutionModel { FilePath = "path_to_solution" };
            var classModels = new List<ClassModel>
            {
                new()
                {
                    FilePath = "pathToClass",
                    Name = "SomeNamespace.FirstClass",
                    ClassType = "class",
                    AccessModifier = "public",
                    Modifier = "sealed"
                }
            };

            const string expectedString =
                @"{""FilePath"":""path_to_solution"",""Projects"":[{""Name"":""A Project"",""FilePath"":""some_other_path"",""ProjectReferences"":[],""Namespaces"":[{""Name"":""SomeNamespace"",""ClassModels"":[{""ClassType"":""class"",""Name"":""SomeNamespace.FirstClass"",""FilePath"":""pathToClass"",""Loc"":{""SourceLines"":0,""CommentedLines"":0,""EmptyLines"":0},""AccessModifier"":""public"",""Modifier"":""sealed"",""ContainingTypeName"":""SomeNamespace"",""BaseTypes"":[],""Imports"":[],""Fields"":[],""Properties"":[],""Constructors"":[],""Methods"":[],""Metrics"":[],""Attributes"":[],""Namespace"":""SomeNamespace""}]}]}]}";

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
            var solutionModel = new SolutionModel { FilePath = "path_to_solution" };

            var classModel = new ClassModel
            {
                FilePath = "SomePath",
                Name = "SomeNamespace.FirstClass",
                ClassType = "class",
                AccessModifier = "private",
                Modifier = "static",
                BaseTypes = new List<IBaseType>
                {
                    new BaseTypeModel
                    {
                        Name = "object",
                        ClassType = "class"
                    }
                }
            };

            classModel.Metrics.Add(new MetricModel
            {
                ExtractorName = "BaseTypeExtractor",
                ValueType = typeof(List<IBaseType>).FullName,
                Value = new List<IBaseType>
                {
                    new BaseTypeModel
                    {
                        Name = "SomeParent",
                        ClassType = "class"
                    },
                    new BaseTypeModel
                    {
                        Name = "Interface1",
                        ClassType = "interface"
                    }
                }
            });

            var classModels = new List<ClassModel>
            {
                classModel
            };

            const string expectedString =
                @"{""FilePath"":""path_to_solution"",""Projects"":[{""Name"":""ProjectName"",""FilePath"":""some_path"",""ProjectReferences"":[""HoneydewCore""],""Namespaces"":[{""Name"":""SomeNamespace"",""ClassModels"":[{""ClassType"":""class"",""Name"":""SomeNamespace.FirstClass"",""FilePath"":""SomePath"",""Loc"":{""SourceLines"":0,""CommentedLines"":0,""EmptyLines"":0},""AccessModifier"":""private"",""Modifier"":""static"",""ContainingTypeName"":""SomeNamespace"",""BaseTypes"":[{""Name"":""object"",""ClassType"":""class""}],""Imports"":[],""Fields"":[],""Properties"":[],""Constructors"":[],""Methods"":[],""Metrics"":[{""ExtractorName"":""BaseTypeExtractor"",""ValueType"":""System.Collections.Generic.List\u00601[[HoneydewModels.Types.IBaseType, HoneydewModels, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"",""Value"":[{""Name"":""SomeParent"",""ClassType"":""class""},{""Name"":""Interface1"",""ClassType"":""interface""}]}],""Attributes"":[],""Namespace"":""SomeNamespace""}]}]}]}";

            var projectModel = new ProjectModel("ProjectName")
            {
                FilePath = "some_path",
                ProjectReferences = { "HoneydewCore" }
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
            var solutionModel = new SolutionModel { FilePath = "path_to_solution" };
            var classModels = new List<ClassModel>
            {
                new()
                {
                    FilePath = "pathToClass",
                    Name = "SomeNamespace.FirstClass",
                    ClassType = "class",
                    AccessModifier = "protected",
                    Loc = new LinesOfCode
                    {
                        SourceLines = 20,
                        CommentedLines = 5,
                        EmptyLines = 30
                    },
                    Imports =
                    {
                        new UsingModel
                        {
                            Name = "System",
                            Alias = "Sys",
                            AliasType = EAliasType.Namespace
                        },
                        new UsingModel
                        {
                            Name = "System.Collections",
                            IsStatic = true
                        }
                    },
                    Methods = new List<IMethodType>
                    {
                        new MethodModel
                        {
                            Name = "Method1",
                            ReturnType = new ReturnTypeModel
                            {
                                Name = "int",
                                Modifier = "ref"
                            },
                            Modifier = "static",
                            AccessModifier = "public",
                            ContainingTypeName = "SomeNamespace.FirstClass",
                            Loc = new LinesOfCode
                            {
                                CommentedLines = 0,
                                EmptyLines = 4,
                                SourceLines = 6
                            },
                            CyclomaticComplexity = 7,
                            CalledMethods =
                            {
                                new MethodCallModel
                                {
                                    Name = "Parse",
                                    ContainingTypeName = "int",
                                    ParameterTypes =
                                    {
                                        new ParameterModel
                                        {
                                            Name = "string"
                                        }
                                    },
                                }
                            },
                            ParameterTypes = new List<IParameterType>
                            {
                                new ParameterModel
                                {
                                    Name = "string"
                                }
                            }
                        }
                    }
                }
            };

            const string expectedString =
                @"{""FilePath"":""path_to_solution"",""Projects"":[{""Name"":""A Project"",""FilePath"":""some_path"",""ProjectReferences"":[],""Namespaces"":[{""Name"":""SomeNamespace"",""ClassModels"":[{""ClassType"":""class"",""Name"":""SomeNamespace.FirstClass"",""FilePath"":""pathToClass"",""Loc"":{""SourceLines"":20,""CommentedLines"":5,""EmptyLines"":30},""AccessModifier"":""protected"",""Modifier"":"""",""ContainingTypeName"":""SomeNamespace"",""BaseTypes"":[],""Imports"":[{""Name"":""System"",""IsStatic"":false,""Alias"":""Sys"",""AliasType"":1},{""Name"":""System.Collections"",""IsStatic"":true,""Alias"":"""",""AliasType"":0}],""Fields"":[],""Properties"":[],""Constructors"":[],""Methods"":[{""Name"":""Method1"",""ContainingTypeName"":""SomeNamespace.FirstClass"",""Modifier"":""static"",""AccessModifier"":""public"",""ReturnType"":{""Name"":""int"",""Modifier"":""ref"",""Attributes"":[]},""ParameterTypes"":[{""Name"":""string"",""Modifier"":"""",""DefaultValue"":null,""Attributes"":[]}],""CalledMethods"":[{""Name"":""Parse"",""ContainingTypeName"":""int"",""ParameterTypes"":[{""Name"":""string"",""Modifier"":"""",""DefaultValue"":null,""Attributes"":[]}]}],""Attributes"":[],""Loc"":{""SourceLines"":6,""CommentedLines"":0,""EmptyLines"":4},""CyclomaticComplexity"":7,""Metrics"":[]}],""Metrics"":[],""Attributes"":[],""Namespace"":""SomeNamespace""}]}]}]}";

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
