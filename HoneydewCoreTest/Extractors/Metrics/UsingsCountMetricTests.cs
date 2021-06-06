using HoneydewCore.Extractors.Metrics;
using Xunit;

namespace HoneydewCoreTest.Extractors.Metrics
{
    public class UsingsCountMetricTests
    {
        private readonly IMetricExtractor _sut;

        public UsingsCountMetricTests()
        {
            _sut = new UsingsCountMetric();
        }

        [Fact]
        public void Name_ShouldReturn_UsingsCount()
        {
            Assert.Equal("Usings Count", _sut.GetName());
        }
    }
}