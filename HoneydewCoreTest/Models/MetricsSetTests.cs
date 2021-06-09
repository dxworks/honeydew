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
            _sut.Add(new ValueExtractor<string>(""));

            Assert.True(_sut.HasMetrics());
        }

        [Fact]
        public void HasMetrics_ShouldReturnTrue_WhenMultipleMetricsAreAdded()
        {
            _sut.Add(new ValueExtractor<string>(""));
            _sut.Add(new ValueExtractor<int>(0));
            _sut.Add(new ValueExtractor<float>(0.0f));

            Assert.True(_sut.HasMetrics());
        }

        [Fact]
        public void Add_ShouldAddTheCorrectMetric_WhenANewMetricIsAdded()
        {
            var extractor = new ValueExtractor<string>("Value");
            _sut.Add(extractor);

            var optional = _sut.Get<ValueExtractor<string>>();
            Assert.True(optional.HasValue);

            Assert.Equal(extractor.GetMetric(), optional.Value);
            Assert.Equal("Value", ((Metric<string>) optional.Value).Value);
        }

        [Fact]
        public void Get_ShouldGetEmptyOptional_WhenNoMetricIsAdded()
        {
            var optional = _sut.Get<ValueExtractor<int>>();
            Assert.False(optional.HasValue);
        }

        [Fact]
        public void Get_ShouldGetEmptyOptional_WhenMetricIsAddedButOtherMetricIsRequested()
        {
            _sut.Add(new ValueExtractor<string>(""));

            var optional = _sut.Get<ValueExtractor<int>>();
            Assert.False(optional.HasValue);
        }

        [Fact]
        public void Get_ShouldGetCorrectMetrics_WhenMultipleMetricsAreAdded()
        {
            var extractor1 = new ValueExtractor<string>("Some");
            var extractor2 = new ValueExtractor<int>(10);
            var extractor3 = new ValueExtractor<object>(null);
            _sut.Add(extractor1);
            _sut.Add(extractor2);
            _sut.Add(extractor3);

            var optional1 = _sut.Get<ValueExtractor<string>>();
            Assert.True(optional1.HasValue);
            Assert.Equal(extractor1.GetMetric(), optional1.Value);
            Assert.Equal("Some", ((Metric<string>) optional1.Value).Value);

            var optional2 = _sut.Get<ValueExtractor<int>>();
            Assert.True(optional2.HasValue);
            Assert.Equal(extractor2.GetMetric(), optional2.Value);
            Assert.Equal(10, ((Metric<int>)optional2.Value).Value);

            var optional3 = _sut.Get<ValueExtractor<object>>();
            Assert.True(optional3.HasValue);
            Assert.Equal(extractor3.GetMetric(), optional3.Value);
            Assert.Null(((Metric<object>)optional3.Value).Value);
        }

        [Fact]
        public void Get_ShouldGetTheFirstMetric_WhenMultipleMetricsOfTheSameTypeIsGiven()
        {
            var extractor1 = new ValueExtractor<string>("Some");
            var extractor2 = new ValueExtractor<string>("Other Value");
            _sut.Add(extractor1);
            _sut.Add(extractor2);

            var optional = _sut.Get<ValueExtractor<string>>();
            Assert.True(optional.HasValue);
            Assert.Equal(extractor1.GetMetric(), optional.Value);
            Assert.Equal("Some", ((Metric<string>)optional.Value).Value);
        }
    }

    internal class ValueExtractor<T> : IMetricExtractor
    {
        private readonly T _value;
        private readonly MetricType _metricType;

        public ValueExtractor(T value, MetricType metricType = MetricType.Syntactic)
        {
            _value = value;
            _metricType = metricType;
        }

        public MetricType GetMetricType()
        {
            return _metricType;
        }

        public IMetric GetMetric()
        {
            return new Metric<T>(_value);
        }
    }
}