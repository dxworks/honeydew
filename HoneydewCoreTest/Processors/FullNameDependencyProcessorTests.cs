using System.Collections.Generic;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.Models;
using HoneydewCore.Processors;
using Xunit;

namespace HoneydewCoreTest.Processors
{
    public class FullNameDependencyProcessorTests
    {
        private readonly FullNameDependencyProcessor _sut;

        public FullNameDependencyProcessorTests()
        {
            _sut = new FullNameDependencyProcessor();
        }

        [Fact]
        public void GetFunction_ShouldReturnTheSameClassNames_WhenGivenClassNamesThatCouldNotBeLocatedInSolution()
        {
            var solutionModel = new SolutionModel();

            ProjectClassModel classModel1 = new()
            {
                FullName = "Models.Class1", FilePath = "path/Models/Class1.cs"
            };
            classModel1.Metrics.Add(new ClassMetric
            {
                ExtractorName = typeof(ParameterDependenciesMetric).FullName,
                ValueType = typeof(DependencyDataMetric).FullName,
                Value = new DependencyDataMetric
                {
                    Dependencies = new Dictionary<string, int>()
                    {
                        {"Dependency1", 1}
                    },
                    Usings = {"System"}
                }
            });

            ProjectClassModel classModel2 = new()
            {
                FullName = "Services.Class2", FilePath = "path/Services/Class2.cs"
            };
            classModel2.Metrics.Add(new ClassMetric
            {
                ExtractorName = typeof(ParameterDependenciesMetric).FullName,
                ValueType = typeof(DependencyDataMetric).FullName,
                Value = new DependencyDataMetric
                {
                    Dependencies = new Dictionary<string, int>()
                    {
                        {"Dependency1", 2}
                    },
                    Usings = {"System", "Xunit"}
                }
            });

            ProjectClassModel classModel3 = new()
            {
                FullName = "Controllers.Class3", FilePath = "path/Controllers/Class3.cs"
            };
            classModel3.Metrics.Add(new ClassMetric
            {
                ExtractorName = typeof(ParameterDependenciesMetric).FullName,
                ValueType = typeof(DependencyDataMetric).FullName,
                Value = new DependencyDataMetric
                {
                    Dependencies = new Dictionary<string, int>()
                    {
                        {"Dependency1", 6},
                        {"Dependency2", 2}
                    },
                    Usings = {"Xunit"}
                }
            });

            ProjectClassModel classModel4 = new()
            {
                FullName = "Domain.Data.Class4", FilePath = "path/Domain/Data/Class4.cs"
            };
            classModel4.Metrics.Add(new ClassMetric
            {
                ExtractorName = typeof(ParameterDependenciesMetric).FullName,
                ValueType = typeof(DependencyDataMetric).FullName,
                Value = new DependencyDataMetric
                {
                    Dependencies = new Dictionary<string, int>()
                    {
                        {"Dependency2", 2}
                    },
                    Usings = {"System", "Xunit"}
                }
            });

            ProjectClassModel classModel5 = new()
            {
                FullName = "Controllers.Class5", FilePath = "path/Controllers/Class5.cs"
            };
            classModel5.Metrics.Add(new ClassMetric
            {
                ExtractorName = typeof(ParameterDependenciesMetric).FullName,
                ValueType = typeof(DependencyDataMetric).FullName,
                Value = new DependencyDataMetric
                {
                    Dependencies = new Dictionary<string, int>(),
                    Usings = { }
                }
            });

            solutionModel.Add(classModel1);
            solutionModel.Add(classModel2);
            solutionModel.Add(classModel3);
            solutionModel.Add(classModel4);
            solutionModel.Add(classModel5);

            var processable = new ProcessorChain(IProcessable.Of(solutionModel))
                .Process(_sut)
                .Finish<SolutionModel>();

            var processedSolutionModel = processable.Value;

            Assert.False(
                ((DependencyDataMetric) processedSolutionModel.Namespaces["Models"].ClassModels[0].Metrics[0].Value)
                .Dependencies.TryGetValue("Full.Path.Dependency1", out _));
            Assert.False(
                ((DependencyDataMetric) processedSolutionModel.Namespaces["Services"].ClassModels[0].Metrics[0].Value)
                .Dependencies.TryGetValue("Full.Path.Dependency1", out _));
            Assert.False(
                ((DependencyDataMetric) processedSolutionModel.Namespaces["Controllers"].ClassModels[0].Metrics[0]
                    .Value).Dependencies.TryGetValue("Full.Path.Dependency1", out _));
            Assert.False(
                ((DependencyDataMetric) processedSolutionModel.Namespaces["Controllers"].ClassModels[0].Metrics[0]
                    .Value).Dependencies.TryGetValue("Full.Path.Dependency2", out _));
            Assert.False(
                ((DependencyDataMetric) processedSolutionModel.Namespaces["Domain.Data"].ClassModels[0].Metrics[0]
                    .Value).Dependencies.TryGetValue("Full.Path.Dependency2", out _));
        }

        [Fact]
        public void GetFunction_ShouldReturnTheFullClassNames_WhenGivenClassNamesThatCanBeLocatedInSolution()
        {
            var solutionModel = new SolutionModel();

            ProjectClassModel classModel1 = new()
            {
                FullName = "Models.Class1", FilePath = "path/Models/Class1.cs"
            };
            classModel1.Metrics.Add(new ClassMetric
            {
                ExtractorName = typeof(ParameterDependenciesMetric).FullName,
                ValueType = typeof(DependencyDataMetric).FullName,
                Value = new DependencyDataMetric
                {
                    Dependencies = new Dictionary<string, int>(),
                    Usings = {"System"}
                }
            });

            ProjectClassModel classModel2 = new()
            {
                FullName = "Services.Class2", FilePath = "path/Services/Class2.cs"
            };
            classModel2.Metrics.Add(new ClassMetric
            {
                ExtractorName = typeof(ParameterDependenciesMetric).FullName,
                ValueType = typeof(DependencyDataMetric).FullName,
                Value = new DependencyDataMetric
                {
                    Dependencies = new Dictionary<string, int>()
                    {
                        {"Class1", 2}
                    },
                    Usings = {"System", "Models"}
                }
            });

            ProjectClassModel classModel3 = new()
            {
                FullName = "Controllers.Class3", FilePath = "path/Controllers/Class3.cs"
            };
            classModel3.Metrics.Add(new ClassMetric
            {
                ExtractorName = typeof(ParameterDependenciesMetric).FullName,
                ValueType = typeof(DependencyDataMetric).FullName,
                Value = new DependencyDataMetric
                {
                    Dependencies = new Dictionary<string, int>()
                    {
                        {"Class1", 6},
                        {"Class2", 2}
                    },
                    Usings = {"Models", "Services"}
                }
            });

            ProjectClassModel classModel4 = new()
            {
                FullName = "Domain.Data.Class4", FilePath = "path/Domain/Data/Class4.cs"
            };
            classModel4.Metrics.Add(new ClassMetric
            {
                ExtractorName = typeof(ParameterDependenciesMetric).FullName,
                ValueType = typeof(DependencyDataMetric).FullName,
                Value = new DependencyDataMetric
                {
                    Dependencies = new Dictionary<string, int>()
                    {
                        {"Class3", 4},
                        {"Class5", 1},
                    },
                    Usings = {"Controllers"}
                }
            });

            ProjectClassModel classModel5 = new()
            {
                FullName = "Controllers.Class5", FilePath = "path/Controllers/Class5.cs"
            };
            classModel5.Metrics.Add(new ClassMetric
            {
                ExtractorName = typeof(ParameterDependenciesMetric).FullName,
                ValueType = typeof(DependencyDataMetric).FullName,
                Value = new DependencyDataMetric
                {
                    Dependencies = new Dictionary<string, int>(),
                    Usings = { }
                }
            });

            solutionModel.Add(classModel1);
            solutionModel.Add(classModel2);
            solutionModel.Add(classModel3);
            solutionModel.Add(classModel4);
            solutionModel.Add(classModel5);

            var processable = new ProcessorChain(IProcessable.Of(solutionModel))
                .Process(_sut)
                .Finish<SolutionModel>();

            var processedSolutionModel = processable.Value;

            Assert.Empty(
                ((DependencyDataMetric) processedSolutionModel.Namespaces["Models"].ClassModels[0].Metrics[0].Value)
                .Dependencies);

            Assert.True(
                ((DependencyDataMetric) processedSolutionModel.Namespaces["Services"].ClassModels[0].Metrics[0].Value)
                .Dependencies.TryGetValue("Models.Class1", out var depCount1));
            Assert.Equal(2, depCount1);

            Assert.True(
                ((DependencyDataMetric) processedSolutionModel.Namespaces["Controllers"].ClassModels[0].Metrics[0]
                    .Value).Dependencies.TryGetValue("Models.Class1", out var depCount2));
            Assert.Equal(6, depCount2);
            Assert.True(
                ((DependencyDataMetric) processedSolutionModel.Namespaces["Controllers"].ClassModels[0].Metrics[0]
                    .Value).Dependencies.TryGetValue("Services.Class2", out var depCount3));
            Assert.Equal(2, depCount3);
            
            Assert.True(
                ((DependencyDataMetric) processedSolutionModel.Namespaces["Domain.Data"].ClassModels[0].Metrics[0]
                    .Value).Dependencies.TryGetValue("Controllers.Class3", out var depCount4));
            Assert.Equal(4, depCount4);
            Assert.True(
                ((DependencyDataMetric) processedSolutionModel.Namespaces["Domain.Data"].ClassModels[0].Metrics[0]
                    .Value).Dependencies.TryGetValue("Controllers.Class5", out var depCount5));
            Assert.Equal(1, depCount5);
        }
    }
}