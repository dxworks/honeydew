using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Processors;
using Moq;
using Xunit;

namespace HoneydewCoreIntegrationTests.Processors
{
    public class RelationMetricHolderToClassRelationsProcessorTests
    {
        private readonly RelationMetricHolderToClassRelationsProcessor _sut;

        private readonly Mock<IRelationMetricHolder> _relationMetricHolderMock = new();

        public RelationMetricHolderToClassRelationsProcessorTests()
        {
            _sut = new RelationMetricHolderToClassRelationsProcessor();
        }

        [Fact]
        public void GetFunction_ShouldReturnEmptyRepresentation_WhenSolutionModelIsNull()
        {
            var classRelationsRepresentation = _sut.Process(null);

            Assert.Empty(classRelationsRepresentation.ClassRelations);
        }

        [Fact]
        public void GetFunction_ShouldReturnEmptyRepresentation_WhenProvidedWithNullRelationMetricHolder()
        {
            var classRelationsRepresentation = _sut.Process(null);

            Assert.Empty(classRelationsRepresentation.ClassRelations);
        }

        [Fact]
        public void GetFunction_ShouldReturnEmptyRepresentation_WhenProvidedWithEmptyRelationMetricHolder()
        {
            _relationMetricHolderMock.Setup(holder => holder.GetRelations()).Returns(new List<FileRelation>());

            var classRelationsRepresentation = _sut.Process(_relationMetricHolderMock.Object);

            Assert.Empty(classRelationsRepresentation.ClassRelations);
        }

        [Fact]
        public void GetFunction_ShouldReturnTotalCount0_WhenGivenRelationMetricHolderWithData()
        {
            _relationMetricHolderMock.Setup(holder => holder.GetRelations()).Returns(new List<FileRelation>
            {
                new()
                {
                    Type = "Relation Type",
                    FileSource = "Source1",
                    FileTarget = "Target1",
                    RelationCount = 5
                }
            });

            var classRelationsRepresentation = _sut.Process(_relationMetricHolderMock.Object);

            Assert.Equal(0, classRelationsRepresentation.TotalRelationsCount("InvalidClass", "InvalidTarget"));
        }

        [Fact]
        public void
            GetFunction_ShouldReturnRepresentationsWithNoRelations_WhenGivenOneClassWithNoRelations()
        {
            _relationMetricHolderMock.Setup(holder => holder.GetRelations()).Returns(new List<FileRelation>
            {
                new()
                {
                    FileSource = "Models.Class"
                }
            });

            var classRelationsRepresentation = _sut.Process(_relationMetricHolderMock.Object);

            Assert.Equal(1, classRelationsRepresentation.ClassRelations.Count);
            Assert.True(
                classRelationsRepresentation.ClassRelations.TryGetValue("Models.Class", out var targetDictionary));
            Assert.Empty(targetDictionary);
        }

        [Fact]
        public void
            GetFunction_ShouldReturnRepresentationsWithNoRelations_WhenGivenMultipleClassesWithNoRelations()
        {
            const int classCount = 5;

            var fileRelations = new List<FileRelation>();

            for (var i = 0; i < classCount; i++)
            {
                fileRelations.Add(new FileRelation
                {
                    FileSource = "Items.Item" + i
                });
            }

            _relationMetricHolderMock.Setup(holder => holder.GetRelations()).Returns(fileRelations);

            var classRelationsRepresentation = _sut.Process(_relationMetricHolderMock.Object);

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
            GetFunction_ShouldReturnRepresentationsWithRelations_WhenGivenTwoClassesAndRelationsBetweenThem()
        {
            const string extractorName = "extractorName";
            _relationMetricHolderMock.Setup(holder => holder.GetRelations()).Returns(
                new List<FileRelation>
                {
                    new()
                    {
                        FileSource = "Models.Class1"
                    },
                    new()
                    {
                        FileTarget = "Models.Class1",
                        FileSource = "Models.Class2",
                        RelationCount = 2,
                        Type = extractorName
                    }
                });


            var classRelationsRepresentation = _sut.Process(_relationMetricHolderMock.Object);

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
    }
}
