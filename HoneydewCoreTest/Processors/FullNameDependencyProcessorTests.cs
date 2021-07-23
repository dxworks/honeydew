using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewCore.Processors;
using HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp;
using HoneydewModels;
using Moq;
using Xunit;

namespace HoneydewCoreTest.Processors
{
    public class FullNameDependencyProcessorTests
    {
        private readonly FullNameModelProcessor _sut;
        private readonly Mock<IProgressLogger> _progressLoggerMock = new();

        public FullNameDependencyProcessorTests()
        {
            _sut = new FullNameModelProcessor(_progressLoggerMock.Object);
        }

        [Fact]
        public void GetFunction_ShouldReturnTheSameClassNames_WhenGivenClassNamesThatCouldNotBeLocatedInSolution()
        {
            var solutionModel = new SolutionModel();

            ClassModel classModel1 = new()
            {
                FullName = "Models.Class1", FilePath = "path/Models/Class1.cs"
            };
            classModel1.Metrics.Add(new ClassMetric
            {
                ExtractorName = typeof(CSharpParameterDependencyMetric).FullName,
                ValueType = typeof(CSharpDependencyDataMetric).FullName,
                Value = new CSharpDependencyDataMetric
                {
                    Dependencies = new Dictionary<string, int>()
                    {
                        {"Dependency1", 1}
                    },
                    Usings = {"System"}
                }
            });

            ClassModel classModel2 = new()
            {
                FullName = "Services.Class2", FilePath = "path/Services/Class2.cs"
            };
            classModel2.Metrics.Add(new ClassMetric
            {
                ExtractorName = typeof(CSharpParameterDependencyMetric).FullName,
                ValueType = typeof(CSharpDependencyDataMetric).FullName,
                Value = new CSharpDependencyDataMetric
                {
                    Dependencies = new Dictionary<string, int>()
                    {
                        {"Dependency1", 2}
                    },
                    Usings = {"System", "Xunit"}
                }
            });

            ClassModel classModel3 = new()
            {
                FullName = "Controllers.Class3", FilePath = "path/Controllers/Class3.cs"
            };
            classModel3.Metrics.Add(new ClassMetric
            {
                ExtractorName = typeof(CSharpParameterDependencyMetric).FullName,
                ValueType = typeof(CSharpDependencyDataMetric).FullName,
                Value = new CSharpDependencyDataMetric
                {
                    Dependencies = new Dictionary<string, int>()
                    {
                        {"Dependency1", 6},
                        {"Dependency2", 2}
                    },
                    Usings = {"Xunit"}
                }
            });

            ClassModel classModel4 = new()
            {
                FullName = "Domain.Data.Class4", FilePath = "path/Domain/Data/Class4.cs"
            };
            classModel4.Metrics.Add(new ClassMetric
            {
                ExtractorName = typeof(CSharpParameterDependencyMetric).FullName,
                ValueType = typeof(CSharpDependencyDataMetric).FullName,
                Value = new CSharpDependencyDataMetric
                {
                    Dependencies = new Dictionary<string, int>()
                    {
                        {"Dependency2", 2}
                    },
                    Usings = {"System", "Xunit"}
                }
            });

            ClassModel classModel5 = new()
            {
                FullName = "Controllers.Class5", FilePath = "path/Controllers/Class5.cs"
            };
            classModel5.Metrics.Add(new ClassMetric
            {
                ExtractorName = typeof(CSharpParameterDependencyMetric).FullName,
                ValueType = typeof(CSharpDependencyDataMetric).FullName,
                Value = new CSharpDependencyDataMetric
                {
                    Dependencies = new Dictionary<string, int>()
                }
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

            var processedRepositoryModel = _sut.Process(repositoryModel);

            var processedProjectModel = processedRepositoryModel.Solutions[0].Projects[0];

            Assert.False(
                ((CSharpDependencyDataMetric) processedProjectModel.Namespaces[0].ClassModels[0].Metrics[0].Value)
                .Dependencies.TryGetValue("Full.Path.Dependency1", out _));
            Assert.False(
                ((CSharpDependencyDataMetric) processedProjectModel.Namespaces[1].ClassModels[0].Metrics[0].Value)
                .Dependencies.TryGetValue("Full.Path.Dependency1", out _));
            Assert.False(
                ((CSharpDependencyDataMetric) processedProjectModel.Namespaces[2].ClassModels[0].Metrics[0]
                    .Value).Dependencies.TryGetValue("Full.Path.Dependency1", out _));
            Assert.False(
                ((CSharpDependencyDataMetric) processedProjectModel.Namespaces[2].ClassModels[0].Metrics[0]
                    .Value).Dependencies.TryGetValue("Full.Path.Dependency2", out _));
            Assert.False(
                ((CSharpDependencyDataMetric) processedProjectModel.Namespaces[3].ClassModels[0].Metrics[0]
                    .Value).Dependencies.TryGetValue("Full.Path.Dependency2", out _));
        }

        [Fact]
        public void GetFunction_ShouldReturnTheFullClassNames_WhenGivenClassNamesThatCanBeLocatedInSolution()
        {
            var solutionModel = new SolutionModel();

            ClassModel classModel1 = new()
            {
                FullName = "Models.Class1", FilePath = "path/Models/Class1.cs"
            };
            classModel1.Metrics.Add(new ClassMetric
            {
                ExtractorName = typeof(CSharpParameterDependencyMetric).FullName,
                ValueType = typeof(CSharpDependencyDataMetric).FullName,
                Value = new CSharpDependencyDataMetric
                {
                    Dependencies = new Dictionary<string, int>(),
                    Usings = {"System"}
                }
            });

            ClassModel classModel2 = new()
            {
                FullName = "Services.Class2", FilePath = "path/Services/Class2.cs"
            };
            classModel2.Metrics.Add(new ClassMetric
            {
                ExtractorName = typeof(CSharpParameterDependencyMetric).FullName,
                ValueType = typeof(CSharpDependencyDataMetric).FullName,
                Value = new CSharpDependencyDataMetric
                {
                    Dependencies = new Dictionary<string, int>()
                    {
                        {"Class1", 2}
                    },
                    Usings = {"System", "Models"}
                }
            });

            ClassModel classModel3 = new()
            {
                FullName = "Controllers.Class3", FilePath = "path/Controllers/Class3.cs"
            };
            classModel3.Metrics.Add(new ClassMetric
            {
                ExtractorName = typeof(CSharpParameterDependencyMetric).FullName,
                ValueType = typeof(CSharpDependencyDataMetric).FullName,
                Value = new CSharpDependencyDataMetric
                {
                    Dependencies = new Dictionary<string, int>()
                    {
                        {"Class1", 6},
                        {"Class2", 2}
                    },
                    Usings = {"Models", "Services"}
                }
            });

            ClassModel classModel4 = new()
            {
                FullName = "Domain.Data.Class4", FilePath = "path/Domain/Data/Class4.cs"
            };
            classModel4.Metrics.Add(new ClassMetric
            {
                ExtractorName = typeof(CSharpParameterDependencyMetric).FullName,
                ValueType = typeof(CSharpDependencyDataMetric).FullName,
                Value = new CSharpDependencyDataMetric
                {
                    Dependencies = new Dictionary<string, int>()
                    {
                        {"Class3", 4},
                        {"Class5", 1},
                    },
                    Usings = {"Controllers"}
                }
            });

            ClassModel classModel5 = new()
            {
                FullName = "Controllers.Class5", FilePath = "path/Controllers/Class5.cs"
            };
            classModel5.Metrics.Add(new ClassMetric
            {
                ExtractorName = typeof(CSharpParameterDependencyMetric).FullName,
                ValueType = typeof(CSharpDependencyDataMetric).FullName,
                Value = new CSharpDependencyDataMetric
                {
                    Dependencies = new Dictionary<string, int>()
                }
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
            
            
            var processedRepositoryModel = _sut.Process(repositoryModel);

            var processedProjectModel = processedRepositoryModel.Solutions[0].Projects[0];

            Assert.Empty(
                ((CSharpDependencyDataMetric) processedProjectModel.Namespaces[0].ClassModels[0].Metrics[0].Value)
                .Dependencies);

            Assert.True(
                ((CSharpDependencyDataMetric) processedProjectModel.Namespaces[1].ClassModels[0].Metrics[0].Value)
                .Dependencies.TryGetValue("Models.Class1", out var depCount1));
            Assert.Equal(2, depCount1);

            Assert.True(
                ((CSharpDependencyDataMetric) processedProjectModel.Namespaces[2].ClassModels[0].Metrics[0]
                    .Value).Dependencies.TryGetValue("Models.Class1", out var depCount2));
            Assert.Equal(6, depCount2);
            Assert.True(
                ((CSharpDependencyDataMetric) processedProjectModel.Namespaces[2].ClassModels[0].Metrics[0]
                    .Value).Dependencies.TryGetValue("Services.Class2", out var depCount3));
            Assert.Equal(2, depCount3);

            Assert.True(
                ((CSharpDependencyDataMetric) processedProjectModel.Namespaces[3].ClassModels[0].Metrics[0]
                    .Value).Dependencies.TryGetValue("Controllers.Class3", out var depCount4));
            Assert.Equal(4, depCount4);
            Assert.True(
                ((CSharpDependencyDataMetric) processedProjectModel.Namespaces[3].ClassModels[0].Metrics[0]
                    .Value).Dependencies.TryGetValue("Controllers.Class5", out var depCount5));
            Assert.Equal(1, depCount5);
        }
    }
}
