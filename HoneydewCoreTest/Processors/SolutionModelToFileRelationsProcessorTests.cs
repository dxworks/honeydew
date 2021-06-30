﻿using System.Collections.Generic;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.Models;
using HoneydewCore.Processors;
using Xunit;

namespace HoneydewCoreTest.Processors
{
    public class SolutionModelToFileRelationsProcessorTests
    {
        private readonly SolutionModelToFileRelationsProcessor _sut;

        public SolutionModelToFileRelationsProcessorTests()
        {
            _sut = new SolutionModelToFileRelationsProcessor();
        }

        [Fact]
        public void GetFunction_ShouldReturnEmptyRepresentation_WhenSolutionModelIsNull()
        {
            var processable = _sut.GetFunction().Invoke(new Processable<SolutionModel>(null));
            Assert.Empty(processable.Value.FileRelations);
        }

        [Fact]
        public void GetFunction_ShouldReturnEmptyRepresentation_WhenSolutionModelIsEmpty()
        {
            var processable = _sut.GetFunction().Invoke(new Processable<SolutionModel>(new SolutionModel()));
            Assert.Empty(processable.Value.FileRelations);
        }

        [Fact]
        public void GetFunction_ShouldReturnEmptyRepresentation_WhenSolutionModelHasProjectsWithoutFiles()
        {
            var solutionModel = new SolutionModel();
            solutionModel.Projects.Add(new ProjectModel());
            solutionModel.Projects.Add(new ProjectModel());
            solutionModel.Projects.Add(new ProjectModel());

            var processable = _sut.GetFunction().Invoke(new Processable<SolutionModel>(solutionModel));
            Assert.Empty(processable.Value.FileRelations);
        }

        [Fact]
        public void GetFunction_ShouldReturnTotalCount0_WhenGivenInvalidSourceName()
        {
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            projectModel.Add(new ClassModel
            {
                FullName = "Models.Class", FilePath = "path/Model/Class.cs"
            });

            solutionModel.Projects.Add(projectModel);

            var processable = _sut.GetFunction().Invoke(new Processable<SolutionModel>(solutionModel));

            Assert.Equal(0, processable.Value.TotalRelationsCount("InvalidClass", "InvalidTarget"));
        }

        [Fact]
        public void
            GetFunction_ShouldReturnRepresentationsWithNoRelations_WhenSolutionModelHasProjectWithOneClassWithNoRelations()
        {
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            projectModel.Add(new ClassModel
            {
                FullName = "Models.Class", FilePath = "path/Model/Class.cs"
            });

            solutionModel.Projects.Add(projectModel);

            var processable = _sut.GetFunction().Invoke(new Processable<SolutionModel>(solutionModel));
            Assert.Equal(1, processable.Value.FileRelations.Count);

            Assert.True(processable.Value.FileRelations.TryGetValue("Models.Class", out var targetDictionary));
            Assert.Empty(targetDictionary);
        }

        [Fact]
        public void
            GetFunction_ShouldReturnRepresentationsWithNoRelations_WhenSolutionModelHasProjectWithMultipleClassesWithNoRelations()
        {
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            const int classCount = 5;
            for (var i = 0; i < classCount; i++)
            {
                projectModel.Add(new ClassModel
                {
                    FullName = "Items.Item" + i,
                    FilePath = "path/Items/Item" + i + ".cs"
                });
            }

            solutionModel.Projects.Add(projectModel);

            var processable = _sut.GetFunction().Invoke(new Processable<SolutionModel>(solutionModel));
            Assert.Equal(classCount, processable.Value.FileRelations.Count);
            for (var i = 0; i < classCount; i++)
            {
                Assert.True(processable.Value.FileRelations.TryGetValue("Items.Item" + i, out var targetDictionary));
                Assert.Empty(targetDictionary);
            }

            foreach (var (key, _) in processable.Value.FileRelations)
            {
                Assert.Equal(0, processable.Value.TotalRelationsCount(key, "invalidDependency"));
            }
        }

        [Fact]
        public void
            GetFunction_ShouldReturnRepresentationsWithRelations_WhenSolutionModelHasProjectWithTwoClassesAndRelationsBetweenThem()
        {
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            projectModel.Add(new ClassModel
            {
                FullName = "Models.Class1", FilePath = "path/Model/Class1.cs"
            });

            var extractorName = typeof(ParameterDependencyMetric).FullName;
            projectModel.Add(new ClassModel
            {
                FullName = "Models.Class2", FilePath = "path/Model/Class2.cs",
                Metrics =
                {
                    new ClassMetric
                    {
                        ExtractorName = extractorName,
                        ValueType = typeof(DependencyDataMetric).FullName,
                        Value = new DependencyDataMetric
                        {
                            Dependencies = new Dictionary<string, int>()
                            {
                                {"Models.Class1", 2}
                            }
                        }
                    }
                }
            });

            solutionModel.Projects.Add(projectModel);

            var processable = _sut.GetFunction().Invoke(new Processable<SolutionModel>(solutionModel));

            Assert.Equal(2, processable.Value.FileRelations.Count);

            Assert.True(processable.Value.FileRelations.TryGetValue("Models.Class1", out var targetDictionary1));
            Assert.Empty(targetDictionary1);

            Assert.True(processable.Value.FileRelations.TryGetValue("Models.Class2", out var targetDictionary2));
            Assert.NotEmpty(targetDictionary2);
            Assert.Equal(1, targetDictionary2.Count);

            Assert.True(targetDictionary2.TryGetValue("Models.Class1", out var dependencyDictionary));
            Assert.Equal(1, dependencyDictionary.Count);
            Assert.True(dependencyDictionary.TryGetValue(extractorName!, out var count));
            Assert.Equal(2, count);

            Assert.Equal(2, processable.Value.TotalRelationsCount("Models.Class2", "Models.Class1"));
        }
        
         [Fact]
        public void
            GetFunction_ShouldReturnRepresentationsWithNoRelations_WhenSolutionModelHasProjectWithInvalidRelationMetric()
        {
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            const string extractorName = "some_string";
            projectModel.Add(new ClassModel
            {
                FullName = "Models.Class2", FilePath = "path/Model/Class2.cs",
                Metrics =
                {
                    new ClassMetric
                    {
                        ExtractorName = extractorName,
                        ValueType = typeof(string).FullName,
                        Value = ""
                    }
                }
            });

            solutionModel.Projects.Add(projectModel);

            var processable = _sut.GetFunction().Invoke(new Processable<SolutionModel>(solutionModel));

            Assert.Equal(0, processable.Value.FileRelations.Count);
            Assert.Equal(0, processable.Value.TotalRelationsCount("Models.Class2", "Models.Class1"));
        }
    }
}