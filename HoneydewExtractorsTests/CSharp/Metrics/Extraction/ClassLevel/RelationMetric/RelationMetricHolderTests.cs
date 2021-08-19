using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Extraction.Class.Relations;
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
            var fileRelations = _sut.GetRelations(new Dictionary<string, IDictionary<string, int>>());

            Assert.Empty(fileRelations);
        }

        [Fact]
        public void GetRelations_ShouldHaveNoRelations_WhenDependenciesAreOnlyPrimitiveTypes()
        {
            var fileRelations = _sut.GetRelations(new Dictionary<string, IDictionary<string, int>>
            {
                {
                    "Class1", new Dictionary<string, int>
                    {
                        { "int", 3 },
                        { "float", 2 },
                        { "string", 1 }
                    }
                },
                {
                    "Class2", new Dictionary<string, int>
                    {
                        { "System.Int32", 6 }
                    }
                }
            });

            Assert.Empty(fileRelations);
        }

        [Fact]
        public void GetRelations_Extract_ShouldHaveRelations_WhenThereAreNonPrimitiveDependencies()
        {
            var fileRelations = _sut.GetRelations(new Dictionary<string, IDictionary<string, int>>
            {
                {
                    "Class1", new Dictionary<string, int>
                    {
                        { "int", 3 },
                        { "IFactExtractor", 2 },
                        { "CSharpMetricExtractor", 1 }
                    }
                },
                {
                    "Class2", new Dictionary<string, int>
                    {
                        { "IMetric", 5 }
                    }
                }
            });

            Assert.NotEmpty(fileRelations);
            Assert.Equal(3, fileRelations.Count);

            var fileRelation1 = fileRelations[0];
            Assert.Equal("Class1", fileRelation1.FileSource);
            Assert.Equal("IFactExtractor", fileRelation1.FileTarget);
            Assert.Equal(2, fileRelation1.RelationCount);

            var fileRelation2 = fileRelations[1];
            Assert.Equal("Class1", fileRelation2.FileSource);
            Assert.Equal("CSharpMetricExtractor", fileRelation2.FileTarget);
            Assert.Equal(1, fileRelation2.RelationCount);

            var fileRelation3 = fileRelations[2];
            Assert.Equal("Class2", fileRelation3.FileSource);
            Assert.Equal("IMetric", fileRelation3.FileTarget);
            Assert.Equal(5, fileRelation3.RelationCount);
        }
    }
}
