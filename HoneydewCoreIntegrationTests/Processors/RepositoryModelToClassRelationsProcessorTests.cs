using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Processors;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewCoreIntegrationTests.Processors
{
    public class RepositoryModelToClassRelationsProcessorTests
    {
        private readonly RepositoryModelToClassRelationsProcessor _sut;

        private readonly Mock<IMetricRelationsProvider> _metricRelationsProviderMock = new();
        private readonly Mock<IMetricPrettier> _metricPrettierMock = new();

        public RepositoryModelToClassRelationsProcessorTests()
        {
            _sut = new RepositoryModelToClassRelationsProcessor(_metricRelationsProviderMock.Object,
                _metricPrettierMock.Object,
                false);
        }

        [Fact]
        public void GetFunction_ShouldReturnEmptyRepresentation_WhenSolutionModelIsNull()
        {
            var classRelationsRepresentation = _sut.Process(null);
            Assert.Empty(classRelationsRepresentation.ClassRelations);
        }

        [Fact]
        public void GetFunction_ShouldReturnEmptyRepresentation_WhenSolutionModelIsEmpty()
        {
            var classRelationsRepresentation = _sut.Process(new RepositoryModel());
            Assert.Empty(classRelationsRepresentation.ClassRelations);
        }

        [Fact]
        public void GetFunction_ShouldReturnEmptyRepresentation_WhenSolutionModelHasProjectsWithoutFiles()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            solutionModel.Projects.Add(new ProjectModel());
            solutionModel.Projects.Add(new ProjectModel());
            solutionModel.Projects.Add(new ProjectModel());

            repositoryModel.Solutions.Add(solutionModel);

            var classRelationsRepresentation = _sut.Process(repositoryModel);
            Assert.Empty(classRelationsRepresentation.ClassRelations);
        }

        [Fact]
        public void GetFunction_ShouldReturnTotalCount0_WhenGivenInvalidSourceName()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            projectModel.Add(new ClassModel
            {
                Name = "Models.Class", FilePath = "path/Model/Class.cs"
            });

            solutionModel.Projects.Add(projectModel);
            repositoryModel.Solutions.Add(solutionModel);

            var classRelationsRepresentation = _sut.Process(repositoryModel);

            Assert.Equal(0, classRelationsRepresentation.TotalRelationsCount("InvalidClass", "InvalidTarget"));
        }

        [Fact]
        public void
            GetFunction_ShouldReturnRepresentationsWithNoRelations_WhenSolutionModelHasProjectWithOneClassWithNoRelations()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            projectModel.Add(new ClassModel
            {
                Name = "Models.Class", FilePath = "path/Model/Class.cs"
            });

            solutionModel.Projects.Add(projectModel);
            repositoryModel.Solutions.Add(solutionModel);

            var classRelationsRepresentation = _sut.Process(repositoryModel);
            Assert.Equal(1, classRelationsRepresentation.ClassRelations.Count);

            Assert.True(
                classRelationsRepresentation.ClassRelations.TryGetValue("Models.Class", out var targetDictionary));
            Assert.Empty(targetDictionary);
        }

        [Fact]
        public void
            GetFunction_ShouldReturnRepresentationsWithNoRelations_WhenSolutionModelHasProjectWithMultipleClassesWithNoRelations()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            const int classCount = 5;
            for (var i = 0; i < classCount; i++)
            {
                projectModel.Add(new ClassModel
                {
                    Name = "Items.Item" + i,
                    FilePath = "path/Items/Item" + i + ".cs"
                });
            }

            solutionModel.Projects.Add(projectModel);
            repositoryModel.Solutions.Add(solutionModel);

            var classRelationsRepresentation = _sut.Process(repositoryModel);
            Assert.Equal(classCount, classRelationsRepresentation.ClassRelations.Count);
            for (var i = 0; i < classCount; i++)
            {
                Assert.True(
                    classRelationsRepresentation.ClassRelations.TryGetValue("Items.Item" + i,
                        out var targetDictionary));
                Assert.Empty(targetDictionary);
            }

            foreach (var (key, _) in classRelationsRepresentation.ClassRelations)
            {
                Assert.Equal(0, classRelationsRepresentation.TotalRelationsCount(key, "invalidDependency"));
            }
        }

        [Fact]
        public void
            GetFunction_ShouldReturnRepresentationsWithRelations_WhenSolutionModelHasProjectWithTwoClassesAndRelationsBetweenThem()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            projectModel.Add(new ClassModel
            {
                Name = "Models.Class1", FilePath = "path/Model/Class1.cs"
            });

            var extractorName = typeof(CSharpParameterRelationMetric).FullName;
            var classMetric = new MetricModel
            {
                ExtractorName = extractorName,
                ValueType = typeof(IDictionary<string, int>).FullName,
                Value = new Dictionary<string, int>
                {
                    {"Models.Class1", 2}
                }
            };
            projectModel.Add(new ClassModel
            {
                Name = "Models.Class2", FilePath = "path/Model/Class2.cs",
                Metrics =
                {
                    classMetric
                }
            });


            solutionModel.Projects.Add(projectModel);
            repositoryModel.Solutions.Add(solutionModel);

            _metricRelationsProviderMock.Setup(provider => provider.GetFileRelations(classMetric)).Returns(
                new List<FileRelation>
                {
                    new()
                    {
                        FileTarget = "Models.Class1",
                        RelationCount = 2,
                        FileSource = extractorName
                    }
                });


            var classRelationsRepresentation = _sut.Process(repositoryModel);

            Assert.Equal(2, classRelationsRepresentation.ClassRelations.Count);

            Assert.True(
                classRelationsRepresentation.ClassRelations.TryGetValue("Models.Class1", out var targetDictionary1));
            Assert.Empty(targetDictionary1);

            Assert.True(
                classRelationsRepresentation.ClassRelations.TryGetValue("Models.Class2", out var targetDictionary2));
            Assert.NotEmpty(targetDictionary2);
            Assert.Equal(1, targetDictionary2.Count);

            Assert.True(targetDictionary2.TryGetValue("Models.Class1", out var dependencyDictionary));
            Assert.Equal(1, dependencyDictionary.Count);
            Assert.True(dependencyDictionary.TryGetValue(extractorName!, out var count));
            Assert.Equal(2, count);

            Assert.Equal(2, classRelationsRepresentation.TotalRelationsCount("Models.Class2", "Models.Class1"));
        }

        [Fact]
        public void
            GetFunction_ShouldReturnRepresentationsWithNoRelations_WhenSolutionModelHasProjectWithInvalidRelationMetric()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            const string extractorName = "some_string";
            var classMetric = new MetricModel
            {
                ExtractorName = extractorName,
                ValueType = typeof(string).FullName,
                Value = ""
            };
            projectModel.Add(new ClassModel
            {
                Name = "Models.Class2", FilePath = "path/Model/Class2.cs",
                Metrics =
                {
                    classMetric
                }
            });

            _metricRelationsProviderMock.Setup(provider => provider.GetFileRelations(classMetric)).Returns(
                new List<FileRelation>());

            solutionModel.Projects.Add(projectModel);
            repositoryModel.Solutions.Add(solutionModel);

            var classRelationsRepresentation = _sut.Process(repositoryModel);

            Assert.Equal(0, classRelationsRepresentation.ClassRelations.Count);
            Assert.Equal(0, classRelationsRepresentation.TotalRelationsCount("Models.Class2", "Models.Class1"));
        }
    }
}
