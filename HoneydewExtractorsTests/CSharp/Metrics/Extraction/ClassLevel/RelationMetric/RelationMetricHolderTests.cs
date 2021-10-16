using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel.RelationMetric
{
    public class RelationMetricHolderTests
    {
        private readonly RelationMetricHolder _sut;

        public RelationMetricHolderTests()
        {
            _sut = new RelationMetricHolder();
        }

        [Fact]
        public void GetRelations_ShouldHaveNoRelations_WhenClassHasNoFields()
        {
            var fileRelations = _sut.GetRelations();

            Assert.Empty(fileRelations);
        }

        [Fact]
        public void GetRelations_ShouldHaveNoRelations_WhenDependenciesAreOnlyPrimitiveTypes()
        {
            var relationMetricMock1 = new Mock<IRelationVisitor>();
            var relationMetricMock2 = new Mock<IRelationVisitor>();

            _sut.Add("Class1", "int", relationMetricMock1.Object);
            _sut.Add("Class1", "int", relationMetricMock1.Object);
            _sut.Add("Class1", "int", relationMetricMock1.Object);
            _sut.Add("Class1", "float", relationMetricMock1.Object);
            _sut.Add("Class1", "float", relationMetricMock1.Object);
            _sut.Add("Class1", "string", relationMetricMock1.Object);

            for (var i = 0; i < 6; i++)
            {
                _sut.Add("Class2", "System.Int32", relationMetricMock2.Object);
            }

            var fileRelations = _sut.GetRelations();

            Assert.Empty(fileRelations);
        }

        [Fact]
        public void GetRelations_Extract_ShouldHaveRelations_WhenThereAreNonPrimitiveDependencies()
        {
            var relationMetricMock1 = new Mock<IRelationVisitor>();
            var relationMetricMock2 = new Mock<IRelationVisitor>();

            relationMetricMock1.Setup(metric => metric.PrettyPrint()).Returns("Relation 1");
            relationMetricMock2.Setup(metric => metric.PrettyPrint()).Returns("Relation 2");

            _sut.Add("Class1", "int", relationMetricMock1.Object);
            _sut.Add("Class1", "int", relationMetricMock1.Object);
            _sut.Add("Class1", "int", relationMetricMock1.Object);
            _sut.Add("Class1", "IFactExtractor", relationMetricMock1.Object);
            _sut.Add("Class1", "IFactExtractor", relationMetricMock1.Object);
            _sut.Add("Class1", "CSharpMetricExtractor", relationMetricMock1.Object);

            for (var i = 0; i < 5; i++)
            {
                _sut.Add("Class2", "IMetric", relationMetricMock2.Object);
            }

            _sut.Add("Class2", "IFactExtractor", relationMetricMock2.Object);

            var fileRelations = _sut.GetRelations();

            Assert.Equal(4, fileRelations.Count);

            var fileRelation1 = fileRelations[0];
            Assert.Equal("Class1", fileRelation1.Source);
            Assert.Equal("IFactExtractor", fileRelation1.Target);
            Assert.Equal(2, fileRelation1.Strength);
            Assert.Equal("Relation 1", fileRelation1.Type);

            var fileRelation2 = fileRelations[1];
            Assert.Equal("Class1", fileRelation2.Source);
            Assert.Equal("CSharpMetricExtractor", fileRelation2.Target);
            Assert.Equal(1, fileRelation2.Strength);
            Assert.Equal("Relation 1", fileRelation2.Type);

            var fileRelation3 = fileRelations[2];
            Assert.Equal("Class2", fileRelation3.Source);
            Assert.Equal("IMetric", fileRelation3.Target);
            Assert.Equal(5, fileRelation3.Strength);
            Assert.Equal("Relation 2", fileRelation3.Type);

            var fileRelation4 = fileRelations[3];
            Assert.Equal("Class2", fileRelation4.Source);
            Assert.Equal("IFactExtractor", fileRelation4.Target);
            Assert.Equal(1, fileRelation4.Strength);
            Assert.Equal("Relation 2", fileRelation4.Type);
        }
    }
}
