using HoneydewExtractors.Core;
using Xunit;

namespace HoneydewExtractorsTests.Core
{
    public class MetricPrettierTests
    {
        private readonly MetricPrettier _sut;

        public MetricPrettierTests()
        {
            _sut = new MetricPrettier();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("\t")]
        public void Pretty_ShouldReturnEmptyString_WhenProvidedWithInvalidName(string name)
        {
            Assert.Equal("", _sut.Pretty(name));
        }

        [Theory]
        [InlineData("string")]
        [InlineData("object")]
        [InlineData("HoneydewExtractorsTests.Core.MetricPrettierTests")]
        [InlineData("SomeNamespace.SomeClass")]
        public void Pretty_ShouldReturnTheSameName_WhenProvidedWithClassNamesThatAreNotMetrics(string name)
        {
            Assert.Equal(name, _sut.Pretty(name));
        }
        
        [Theory]
        [InlineData("HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.CSharpBaseClassMetric", "Inherits Class")]
        [InlineData("HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.CSharpLocalVariablesDependencyMetric", "Local Variables Dependency")]
        [InlineData("HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.CSharpFieldsInfoMetric", "Fields Info")]
        [InlineData("HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnitLevel.CSharpUsingsCountMetric", "Usings Count")]
        public void Pretty_ShouldReturnThePrettyName_WhenProvidedWithClassNamesThatAreMetrics(string name, string prettyName)
        {
            Assert.Equal(prettyName, _sut.Pretty(name));
        }
    }
}
