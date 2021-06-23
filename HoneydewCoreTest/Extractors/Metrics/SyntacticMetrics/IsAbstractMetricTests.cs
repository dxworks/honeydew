using System;
using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Extractors.Metrics.SyntacticMetrics;
using Xunit;

namespace HoneydewCoreTest.Extractors.Metrics.SyntacticMetrics
{
    public class IsAbstractMetricTests
    {
        private readonly CSharpMetricExtractor _sut;
        private IFactExtractor _factExtractor;

        public IsAbstractMetricTests()
        {
            _sut = new IsAbstractMetric();
        }

        [Fact]
        public void GetMetricType_ShouldReturnSyntactic()
        {
            Assert.True(_sut is ISyntacticMetric);
        }
        
        [Fact]
        public void PrettyPrint_ShouldReturnIsAbstract()
        {
            Assert.Equal("Is Abstract", _sut.PrettyPrint());
        }

        [Fact]
        public void Extract_ShouldReturnFalse_WhenGivenClassWithNoBaseClass()
        {
            const string fileContent = @"using System;
                                    using System.Collections.Generic;
                                    using System.Linq;
                                    using System.Text;

                                    namespace TopLevel
                                    {
                                        public class Foo { int a; public void f(){} }                                        
                                    }";


            var metrics = new List<Type>()
            {
                _sut.GetType()
            };

            _factExtractor = new CSharpClassFactExtractor(metrics);

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            var optional = classModels[0].GetMetric<IsAbstractMetric>();
            Assert.True(optional.HasValue);
            Assert.False((bool) optional.Value);
        }
        
        [Fact]
        public void Extract_ShouldReturnFalse_WhenGivenClassWithBaseClassAndInterface()
        {
            const string fileContent = @"
                                    namespace Bar
                                    {
                                        public class Foo : SomeClass, ISomeInterface { int a; public void f(){} }                                        
                                    }";


            var metrics = new List<Type>()
            {
                _sut.GetType()
            };

            _factExtractor = new CSharpClassFactExtractor(metrics);

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);
            var optional = classModels[0].GetMetric<IsAbstractMetric>();
            Assert.True(optional.HasValue);
            Assert.False((bool) optional.Value);
        }
        
        [Fact]
        public void Extract_ShouldReturnTrue_WhenGivenInterface()
        {
            const string fileContent = @"
                                    namespace Bar
                                    {
                                        public interface Foo { public void f(); }                                        
                                    }";


            var metrics = new List<Type>()
            {
                _sut.GetType()
            };
            
            _factExtractor = new CSharpClassFactExtractor(metrics);

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);
            var optional = classModels[0].GetMetric<IsAbstractMetric>();
            Assert.True(optional.HasValue);
            Assert.True((bool) optional.Value);
        }
        
        [Fact]
        public void Extract_ShouldReturnTrue_WhenGivenAbstractClass()
        {
            const string fileContent = @"
                                    namespace Bar
                                    {
                                        public abstract class Foo { public void g(); }                                        
                                    }";


            var metrics = new List<Type>()
            {
                _sut.GetType()
            };

            _factExtractor = new CSharpClassFactExtractor(metrics);

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);
            var optional = classModels[0].GetMetric<IsAbstractMetric>();
            Assert.True(optional.HasValue);
            Assert.True((bool) optional.Value);
        }
    }
}