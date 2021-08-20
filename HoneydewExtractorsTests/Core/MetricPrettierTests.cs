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
    }
}
