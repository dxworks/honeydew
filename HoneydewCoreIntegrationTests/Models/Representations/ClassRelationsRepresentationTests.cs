using System.Linq;
using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel;
using Moq;
using Xunit;

namespace HoneydewCoreIntegrationTests.Models.Representations
{
    public class ClassRelationsRepresentationTests
    {
        private readonly ClassRelationsRepresentation _sut;
        private readonly Mock<IMetricPrettier> _metricPrettierMock = new();

        public ClassRelationsRepresentationTests()
        {
            _sut = new ClassRelationsRepresentation(_metricPrettierMock.Object);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void Add_ShouldHaveNoRelations_WhenAddedEmptySource(string sourceName)
        {
            _sut.Add(sourceName, "", "", 0);

            Assert.Empty(_sut.ClassRelations);
            Assert.Empty(_sut.DependenciesType);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void Add_WithOneParameter_ShouldHaveNoRelations_WhenAddedEmptySource(string sourceName)
        {
            _sut.Add(sourceName);

            Assert.Empty(_sut.ClassRelations);
            Assert.Empty(_sut.DependenciesType);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void Add_ShouldHaveNoRelations_WhenAddedEmptyTarget(string targetName)
        {
            _sut.Add("sourceName", targetName, "", 0);

            Assert.NotEmpty(_sut.ClassRelations);
            Assert.Equal(1, _sut.ClassRelations.Count);
            Assert.Empty(_sut.DependenciesType);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void Add_ShouldHaveNoRelations_WhenAddedEmptyDependency(string dependencyName)
        {
            _sut.Add("sourceName", "targetName", dependencyName, 0);

            Assert.Empty(_sut.ClassRelations);
            Assert.Empty(_sut.DependenciesType);
        }

        [Fact]
        public void Add_ShouldHaveRelation_WhenAddedOneDependency()
        {
            _sut.Add("source", "target", "dependency", 2);

            Assert.Equal(1, _sut.ClassRelations.Count);
            Assert.True(_sut.ClassRelations.TryGetValue("source", out var targetDictionary));
            Assert.Equal(1, targetDictionary.Count);
            Assert.True(targetDictionary.TryGetValue("target", out var dependenciesDictionary));
            Assert.Equal(1, dependenciesDictionary.Count);
            Assert.True(dependenciesDictionary.TryGetValue("dependency", out var count));
            Assert.Equal(2, count);

            Assert.Equal(1, _sut.DependenciesType.Count);
            Assert.True(_sut.DependenciesType.Contains("dependency"));
        }

        [Fact]
        public void Add_ShouldHaveRelation_WhenAddedTwoDependenciesForTheSameSourceAndTarget()
        {
            _sut.Add("source", "target", "dependency1", 2);
            _sut.Add("source", "target", "dependency2", 5);

            Assert.Equal(1, _sut.ClassRelations.Count);
            Assert.True(_sut.ClassRelations.TryGetValue("source", out var targetDictionary));
            Assert.Equal(1, targetDictionary.Count);
            Assert.True(targetDictionary.TryGetValue("target", out var dependenciesDictionary));
            Assert.Equal(2, dependenciesDictionary.Count);
            Assert.True(dependenciesDictionary.TryGetValue("dependency1", out var count1));
            Assert.Equal(2, count1);
            Assert.True(dependenciesDictionary.TryGetValue("dependency2", out var count2));
            Assert.Equal(5, count2);

            Assert.Equal(2, _sut.DependenciesType.Count);
            Assert.True(_sut.DependenciesType.Contains("dependency1"));
            Assert.True(_sut.DependenciesType.Contains("dependency2"));
        }

        [Fact]
        public void Add_ShouldHaveRelation_WhenAddedMultipleDependenciesForMultipleSourceAndTargets()
        {
            _sut.Add("source1", "target1", "dependency1", 2);
            _sut.Add("source1", "target1", "dependency2", 3);
            _sut.Add("source1", "target2", "dependency1", 8);
            _sut.Add("source1", "target1", "dependency3", 4);
            _sut.Add("source1", "target1", "dependency1", 2);
            _sut.Add("source1", "target1", "dependency1", 2);
            _sut.Add("source1", "target2", "dependency2", 2);

            _sut.Add("source2", "target2", "dependency1", 7);
            _sut.Add("source2", "target2", "dependency1", 12);
            _sut.Add("source2", "target2", "dependency4", 4);

            Assert.Equal(2, _sut.ClassRelations.Count);


            Assert.True(_sut.ClassRelations.TryGetValue("source1", out var targetDictionary1));
            Assert.Equal(2, targetDictionary1.Count);

            Assert.True(targetDictionary1.TryGetValue("target1", out var dependenciesDictionary1));
            Assert.Equal(3, dependenciesDictionary1.Count);
            Assert.True(dependenciesDictionary1.TryGetValue("dependency1", out var count1));
            Assert.Equal(2, count1);
            Assert.True(dependenciesDictionary1.TryGetValue("dependency2", out var count2));
            Assert.Equal(3, count2);
            Assert.True(dependenciesDictionary1.TryGetValue("dependency3", out var count3));
            Assert.Equal(4, count3);

            Assert.True(targetDictionary1.TryGetValue("target2", out var dependenciesDictionary2));
            Assert.Equal(2, dependenciesDictionary2.Count);
            Assert.True(dependenciesDictionary2.TryGetValue("dependency1", out var count4));
            Assert.Equal(8, count4);
            Assert.True(dependenciesDictionary2.TryGetValue("dependency2", out var count5));
            Assert.Equal(2, count5);


            Assert.True(_sut.ClassRelations.TryGetValue("source2", out var targetDictionary2));
            Assert.Equal(1, targetDictionary2.Count);

            Assert.True(targetDictionary2.TryGetValue("target2", out var dependenciesDictionary3));
            Assert.Equal(2, dependenciesDictionary3.Count);
            Assert.True(dependenciesDictionary3.TryGetValue("dependency1", out var count6));
            Assert.Equal(7, count6);
            Assert.True(dependenciesDictionary3.TryGetValue("dependency4", out var count7));
            Assert.Equal(4, count7);

            Assert.Equal(4, _sut.DependenciesType.Count);
            Assert.True(_sut.DependenciesType.Contains("dependency1"));
            Assert.True(_sut.DependenciesType.Contains("dependency2"));
            Assert.True(_sut.DependenciesType.Contains("dependency3"));
            Assert.True(_sut.DependenciesType.Contains("dependency4"));
        }

        [Fact]
        public void GetDependenciesTypePretty_ShouldReturnTheDependencies_WhenUsePrettyIsFalse()
        {
            _sut.Add("source", "target", typeof(CSharpParameterDependencyMetric).FullName, 4);

            var dependenciesTypePretty = _sut.DependenciesType;

            Assert.Single(dependenciesTypePretty);
            Assert.Equal(typeof(CSharpParameterDependencyMetric).FullName, dependenciesTypePretty.First());
        }

        [Fact]
        public void GetDependenciesTypePretty_ShouldReturnTheDependenciesPrettyPrint_WhenUsePrettyIsTrue()
        {
            var dependencyType = typeof(CSharpParameterDependencyMetric).FullName;

            _metricPrettierMock.Setup(prettier => prettier.Pretty(dependencyType)).Returns("Parameter Dependency");

            _sut.UsePrettyPrint = true;
            _sut.Add("source", "target", dependencyType, 4);

            Assert.Single(_sut.DependenciesType);
            Assert.Equal("Parameter Dependency", _sut.DependenciesType.First());
        }

        [Fact]
        public void GetDependenciesTypePretty_ShouldReturnTheDependencies_WhenUsePrettyIsTrueAndTheClassIsNotAMetric()
        {
            _metricPrettierMock.Setup(prettier => prettier.Pretty("string")).Returns("string");

            _sut.UsePrettyPrint = true;
            _sut.Add("source", "target", "string", 4);

            Assert.Single(_sut.DependenciesType);
            Assert.Equal("string", _sut.DependenciesType.First());
        }
    }
}
