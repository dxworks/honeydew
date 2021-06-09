using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Models;
using Xunit;

namespace HoneydewCoreTest.Models
{
    public class MetricsSetTests
    {
        private readonly MetricsSet _sut;

        public MetricsSetTests()
        {
            _sut = new MetricsSet();
        }

        [Fact]
        public void HasMetrics_ShouldReturnFalse_WhenNoMetricsAreAdded()
        {
            Assert.False(_sut.HasMetrics());
        }

        [Fact]
        public void HasMetrics_ShouldReturnTrue_WhenOneMetricIsAdded()
        {
            _sut.Add(new Metric<string>(""));

            Assert.True(_sut.HasMetrics());
        }

        [Fact]
        public void HasMetrics_ShouldReturnTrue_WhenMultipleMetricsAreAdded()
        {
            _sut.Add(new Metric<string>(""));
            _sut.Add(new Metric<int>(0));
            _sut.Add(new Metric<float>(0.0f));

            Assert.True(_sut.HasMetrics());
        }

        [Fact]
        public void Add_ShouldAddTheCorrectMetric_WhenANewMetricIsAdded()
        {
            var metric = new Metric<string>("Value");
            _sut.Add(metric);

            var optional = _sut.Get<string>();
            Assert.True(optional.HasValue);

            Assert.Equal(metric, optional.Value);
            Assert.Equal("Value", optional.Value.Value);
        }

        [Fact]
        public void Get_ShouldGetEmptyOptional_WhenNoMetricIsAdded()
        {
            var optional = _sut.Get<int>();
            Assert.False(optional.HasValue);
        }

        [Fact]
        public void Get_ShouldGetEmptyOptional_WhenMetricIsAddedButOtherMetricIsRequested()
        {
            _sut.Add(new Metric<string>(""));

            var optional = _sut.Get<int>();
            Assert.False(optional.HasValue);
        }

        [Fact]
        public void Get_ShouldGetCorrectMetrics_WhenMultipleMetricsAreAdded()
        {
            var metric1 = new Metric<string>("Some");
            var metric2 = new Metric<int>(10);
            var metric3 = new Metric<object>(null);
            _sut.Add(metric1);
            _sut.Add(metric2);
            _sut.Add(metric3);

            var optional1 = _sut.Get<string>();
            Assert.True(optional1.HasValue);
            Assert.Equal(metric1, optional1.Value);
            Assert.Equal("Some", optional1.Value.Value);

            var optional2 = _sut.Get<int>();
            Assert.True(optional2.HasValue);
            Assert.Equal(metric2, optional2.Value);
            Assert.Equal(10, optional2.Value.Value);

            var optional3 = _sut.Get<object>();
            Assert.True(optional3.HasValue);
            Assert.Equal(metric3, optional3.Value);
            Assert.Null(optional3.Value.Value);
        }
        
        [Fact]
        public void Get_ShouldGetTheFirstMetric_WhenMultipleMetricsOfTheSameTypeIsGiven()
        {
            var metric1 = new Metric<string>("Some");
            var metric2 = new Metric<string>("Other Value");
            _sut.Add(metric1);
            _sut.Add(metric2);

            var optional = _sut.Get<string>();
            Assert.True(optional.HasValue);
            Assert.Equal(metric1, optional.Value);
            Assert.Equal("Some", optional.Value.Value);
        }
    }
}