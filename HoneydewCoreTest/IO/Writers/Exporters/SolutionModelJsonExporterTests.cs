﻿using System.Collections.Generic;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.IO.Writers.Exporters;
using HoneydewCore.Models;
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
            const string expectedString = "{\"Projects\":[]}";

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
                    FullName = "SomeNamespace.FirstClass",
                    ClassType = "class",
                    AccessModifier = "public",
                    Modifier = "sealed"
                }
            };

            const string expectedString =
                @"{""Projects"":[{""Name"":""A Project"",""Namespaces"":{""SomeNamespace"":{""Name"":""SomeNamespace"",""ClassModels"":[{""ClassType"":""class"",""FilePath"":""pathToClass"",""FullName"":""SomeNamespace.FirstClass"",""AccessModifier"":""public"",""Modifier"":""sealed"",""BaseClassFullName"":""object"",""BaseInterfaces"":[],""Fields"":[],""Methods"":[],""Metrics"":[],""Namespace"":""SomeNamespace""}]}}}]}";

            var projectModel = new ProjectModel("A Project");
            foreach (var classModel in classModels)
            {
                projectModel.Add(classModel);
            }

            solutionModel.Projects.Add(projectModel);

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
                FullName = "SomeNamespace.FirstClass",
                ClassType = "class",
                AccessModifier = "private",
                Modifier = "static"
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
                @"{""Projects"":[{""Name"":""ProjectName"",""Namespaces"":{""SomeNamespace"":{""Name"":""SomeNamespace"",""ClassModels"":[{""ClassType"":""class"",""FilePath"":""SomePath"",""FullName"":""SomeNamespace.FirstClass"",""AccessModifier"":""private"",""Modifier"":""static"",""BaseClassFullName"":""object"",""BaseInterfaces"":[],""Fields"":[],""Methods"":[],""Metrics"":[{""ExtractorName"":""HoneydewCore.Extractors.Metrics.SemanticMetrics.BaseClassMetric"",""ValueType"":""HoneydewCore.Extractors.Metrics.SemanticMetrics.InheritanceMetric"",""Value"":{""Interfaces"":[""Interface1""],""BaseClassName"":""SomeParent""}}],""Namespace"":""SomeNamespace""}]}}}]}";

            var projectModel = new ProjectModel("ProjectName");
            foreach (var model in classModels)
            {
                projectModel.Add(model);
            }

            solutionModel.Projects.Add(projectModel);

            var exportString = solutionModel.Export(_sut);

            Assert.Equal(expectedString, exportString);
        }

        [Fact]
        public void Export_ShouldReturnRawModel_WhenModelHasOneCompilationUnitWithOneClassAndMethodCalls()
        {
            var solutionModel = new SolutionModel();
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
                                    ParameterTypes = {"string"},
                                }
                            },
                            ParameterTypes = { "string"}
                        }
                    }
                }
            };

            const string expectedString =
                @"{""Projects"":[{""Name"":""A Project"",""Namespaces"":{""SomeNamespace"":{""Name"":""SomeNamespace"",""ClassModels"":[{""ClassType"":""class"",""FilePath"":""pathToClass"",""FullName"":""SomeNamespace.FirstClass"",""AccessModifier"":""protected"",""Modifier"":"""",""BaseClassFullName"":""object"",""BaseInterfaces"":[],""Fields"":[],""Methods"":[{""Name"":""Method1"",""ReturnType"":""int"",""Modifier"":""static"",""AccessModifier"":""public"",""ParameterTypes"":[""string""],""ContainingClassName"":""SomeNamespace.FirstClass"",""CalledMethods"":[{""MethodName"":""Parse"",""ContainingClassName"":""int"",""ParameterTypes"":[""string""]}]}],""Metrics"":[],""Namespace"":""SomeNamespace""}]}}}]}";

            var projectModel = new ProjectModel("A Project");
            foreach (var classModel in classModels)
            {
                projectModel.Add(classModel);
            }

            solutionModel.Projects.Add(projectModel);

            var exportString = solutionModel.Export(_sut);

            Assert.Equal(expectedString, exportString);
        }
    }
}