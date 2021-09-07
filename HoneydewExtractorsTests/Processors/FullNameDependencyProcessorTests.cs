using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;
using HoneydewExtractors.Processors;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.Processors
{
    public class FullNameDependencyProcessorTests
    {
        private readonly FullNameModelProcessor _sut;
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly Mock<IProgressLogger> _progressLoggerMock = new();
        private readonly Mock<IProgressLoggerBar> _progressLoggerBarMock = new();

        public FullNameDependencyProcessorTests()
        {
            _sut = new FullNameModelProcessor(_loggerMock.Object, _progressLoggerMock.Object);
        }

        [Fact]
        public void GetFunction_ShouldReturnTheSameClassNames_WhenGivenClassNamesThatCouldNotBeLocatedInSolution()
        {
            var solutionModel = new SolutionModel();

            ClassModel classModel1 = new()
            {
                Name = "Models.Class1", FilePath = "path/Models/Class1.cs"
            };
            classModel1.Metrics.Add(new MetricModel
            {
                ExtractorName = typeof(ParameterRelationVisitor).FullName,
                ValueType = typeof(Dictionary<string, int>).FullName,
                Value = new Dictionary<string, int>
                {
                    { "Dependency1", 1 }
                }
            });

            ClassModel classModel2 = new()
            {
                Name = "Services.Class2", FilePath = "path/Services/Class2.cs"
            };
            classModel2.Metrics.Add(new MetricModel
            {
                ExtractorName = typeof(ParameterRelationVisitor).FullName,
                ValueType = typeof(Dictionary<string, int>).FullName,
                Value = new Dictionary<string, int>
                {
                    { "Dependency1", 2 }
                }
            });

            ClassModel classModel3 = new()
            {
                Name = "Controllers.Class3", FilePath = "path/Controllers/Class3.cs"
            };
            classModel3.Metrics.Add(new MetricModel
            {
                ExtractorName = typeof(ParameterRelationVisitor).FullName,
                ValueType = typeof(Dictionary<string, int>).FullName,
                Value = new Dictionary<string, int>
                {
                    { "Dependency1", 6 },
                    { "Dependency2", 2 }
                }
            });

            ClassModel classModel4 = new()
            {
                Name = "Domain.Data.Class4", FilePath = "path/Domain/Data/Class4.cs"
            };
            classModel4.Metrics.Add(new MetricModel
            {
                ExtractorName = typeof(ParameterRelationVisitor).FullName,
                ValueType = typeof(Dictionary<string, int>).FullName,
                Value = new Dictionary<string, int>
                {
                    { "Dependency2", 2 }
                }
            });

            ClassModel classModel5 = new()
            {
                Name = "Controllers.Class5", FilePath = "path/Controllers/Class5.cs"
            };
            classModel5.Metrics.Add(new MetricModel
            {
                ExtractorName = typeof(ParameterRelationVisitor).FullName,
                ValueType = typeof(Dictionary<string, int>).FullName,
                Value = new Dictionary<string, int>()
            });

            var projectModel = new ProjectModel();

            projectModel.Add(classModel1);
            projectModel.Add(classModel2);
            projectModel.Add(classModel3);
            projectModel.Add(classModel4);
            projectModel.Add(classModel5);

            solutionModel.Projects.Add(projectModel);

            var repositoryModel = new RepositoryModel();
            repositoryModel.Solutions.Add(solutionModel);

            _progressLoggerMock.Setup(logger => logger.CreateProgressLogger(5, "Resolving Class Names"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(5, "Resolving Using Statements for Each Class"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(5, "Resolving Class Elements (Fields, Methods, Properties,...)"))
                .Returns(_progressLoggerBarMock.Object);

            var processedRepositoryModel = _sut.Process(repositoryModel);

            var processedProjectModel = processedRepositoryModel.Solutions[0].Projects[0];

            Assert.False(
                ((Dictionary<string, int>)processedProjectModel.Namespaces[0].ClassModels[0].Metrics[0].Value)
                .TryGetValue("Full.Path.Dependency1", out _));
            Assert.False(
                ((Dictionary<string, int>)processedProjectModel.Namespaces[1].ClassModels[0].Metrics[0].Value)
                .TryGetValue("Full.Path.Dependency1", out _));
            Assert.False(
                ((Dictionary<string, int>)processedProjectModel.Namespaces[2].ClassModels[0].Metrics[0].Value)
                .TryGetValue("Full.Path.Dependency1", out _));
            Assert.False(
                ((Dictionary<string, int>)processedProjectModel.Namespaces[2].ClassModels[0].Metrics[0].Value)
                .TryGetValue("Full.Path.Dependency2", out _));
            Assert.False(
                ((Dictionary<string, int>)processedProjectModel.Namespaces[3].ClassModels[0].Metrics[0].Value)
                .TryGetValue("Full.Path.Dependency2", out _));
        }

        [Fact]
        public void GetFunction_ShouldReturnTheFullClassNames_WhenGivenClassNamesThatCanBeLocatedInSolution()
        {
            var solutionModel = new SolutionModel();

            ClassModel classModel1 = new()
            {
                Name = "Models.Class1", FilePath = "path/Models/Class1.cs"
            };
            classModel1.Metrics.Add(new MetricModel
            {
                ExtractorName = typeof(ParameterRelationVisitor).FullName,
                ValueType = typeof(Dictionary<string, int>).FullName,
                Value = new Dictionary<string, int>()
            });

            ClassModel classModel2 = new()
            {
                Name = "Services.Class2", FilePath = "path/Services/Class2.cs"
            };
            classModel2.Metrics.Add(new MetricModel
            {
                ExtractorName = typeof(ParameterRelationVisitor).FullName,
                ValueType = typeof(Dictionary<string, int>).FullName,
                Value = new Dictionary<string, int>
                {
                    { "Class1", 2 }
                }
            });

            ClassModel classModel3 = new()
            {
                Name = "Controllers.Class3", FilePath = "path/Controllers/Class3.cs"
            };
            classModel3.Metrics.Add(new MetricModel
            {
                ExtractorName = typeof(ParameterRelationVisitor).FullName,
                ValueType = typeof(Dictionary<string, int>).FullName,
                Value = new Dictionary<string, int>
                {
                    { "Class1", 6 },
                    { "Class2", 2 }
                }
            });

            ClassModel classModel4 = new()
            {
                Name = "Domain.Data.Class4", FilePath = "path/Domain/Data/Class4.cs"
            };
            classModel4.Metrics.Add(new MetricModel
            {
                ExtractorName = typeof(ParameterRelationVisitor).FullName,
                ValueType = typeof(Dictionary<string, int>).FullName,
                Value = new Dictionary<string, int>
                {
                    { "Class3", 4 },
                    { "Class5", 1 },
                }
            });

            ClassModel classModel5 = new()
            {
                Name = "Controllers.Class5", FilePath = "path/Controllers/Class5.cs"
            };
            classModel5.Metrics.Add(new MetricModel
            {
                ExtractorName = typeof(ParameterRelationVisitor).FullName,
                ValueType = typeof(Dictionary<string, int>).FullName,
                Value = new Dictionary<string, int>()
            });

            var projectModel = new ProjectModel();

            projectModel.Add(classModel1);
            projectModel.Add(classModel2);
            projectModel.Add(classModel3);
            projectModel.Add(classModel4);
            projectModel.Add(classModel5);

            solutionModel.Projects.Add(projectModel);

            var repositoryModel = new RepositoryModel();
            repositoryModel.Solutions.Add(solutionModel);

            _progressLoggerMock.Setup(logger => logger.CreateProgressLogger(5, "Resolving Class Names"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(5, "Resolving Using Statements for Each Class"))
                .Returns(_progressLoggerBarMock.Object);
            _progressLoggerMock.Setup(logger =>
                    logger.CreateProgressLogger(5, "Resolving Class Elements (Fields, Methods, Properties,...)"))
                .Returns(_progressLoggerBarMock.Object);

            var processedRepositoryModel = _sut.Process(repositoryModel);

            var processedProjectModel = processedRepositoryModel.Solutions[0].Projects[0];

            Assert.Empty(
                (Dictionary<string, int>)processedProjectModel.Namespaces[0].ClassModels[0].Metrics[0].Value);

            Assert.True(
                ((Dictionary<string, int>)processedProjectModel.Namespaces[1].ClassModels[0].Metrics[0].Value)
                .TryGetValue("Models.Class1", out var depCount1));
            Assert.Equal(2, depCount1);

            Assert.True(
                ((Dictionary<string, int>)processedProjectModel.Namespaces[2].ClassModels[0].Metrics[0].Value)
                .TryGetValue("Models.Class1", out var depCount2));
            Assert.Equal(6, depCount2);
            Assert.True(
                ((Dictionary<string, int>)processedProjectModel.Namespaces[2].ClassModels[0].Metrics[0].Value)
                .TryGetValue("Services.Class2", out var depCount3));
            Assert.Equal(2, depCount3);

            Assert.True(
                ((Dictionary<string, int>)processedProjectModel.Namespaces[3].ClassModels[0].Metrics[0].Value)
                .TryGetValue("Controllers.Class3", out var depCount4));
            Assert.Equal(4, depCount4);
            Assert.True(
                ((Dictionary<string, int>)processedProjectModel.Namespaces[3].ClassModels[0].Metrics[0].Value)
                .TryGetValue("Controllers.Class5", out var depCount5));
            Assert.Equal(1, depCount5);
        }
    }
}
