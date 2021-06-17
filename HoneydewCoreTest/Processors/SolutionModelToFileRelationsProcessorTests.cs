using System.Collections.Generic;
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

            var fileRelation = processable.Value.FileRelations[0];

            Assert.Equal("Models.Class", fileRelation.FileSource);
            Assert.Equal("", fileRelation.FileTarget);
            Assert.Equal("", fileRelation.RelationType);
            Assert.Equal(0,fileRelation.RelationCount);
            Assert.Equal(0, processable.Value.TotalRelationsCount(fileRelation.FileSource));
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
                var fileRelation = processable.Value.FileRelations[i];

                Assert.Equal("Items.Item" + i, fileRelation.FileSource);
                Assert.Equal("", fileRelation.FileTarget);
                Assert.Equal("",fileRelation.RelationType);
                Assert.Equal(0, fileRelation.RelationCount);
            }

            foreach (var fileRelation in processable.Value.FileRelations)
            {
                Assert.Equal(0, processable.Value.TotalRelationsCount(fileRelation.FileSource));

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

            var extractorName = typeof(ParameterDependenciesMetric).FullName;
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

            var fileRelation1 = processable.Value.FileRelations[0];
            Assert.Equal("Models.Class1", fileRelation1.FileSource);
            Assert.Equal("", fileRelation1.FileTarget);
            Assert.Equal(0, fileRelation1.RelationCount);

            var fileRelation2 = processable.Value.FileRelations[1];
            Assert.Equal("Models.Class2", fileRelation2.FileSource);
            Assert.Equal("Models.Class1", fileRelation2.FileTarget);
            Assert.Equal(extractorName, fileRelation2.RelationType);
            Assert.Equal(2,fileRelation2.RelationCount);
            
            Assert.Equal(0, processable.Value.TotalRelationsCount(fileRelation1.FileSource));
            Assert.Equal(2, processable.Value.TotalRelationsCount(fileRelation2.FileSource));
        }
    }
}