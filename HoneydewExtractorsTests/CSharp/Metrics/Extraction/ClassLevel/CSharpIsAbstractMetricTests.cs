using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpIsAbstractMetricTests
    {
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpIsAbstractMetricTests()
        {
            _factExtractor = new CSharpFactExtractor();
            _factExtractor.AddMetric<CSharpIsAbstractMetric>();
        }

        [Fact]
        public void GetMetricType_ShouldReturnClassLevel()
        {
            Assert.Equal(ExtractionMetricType.ClassLevel, new CSharpIsAbstractMetric().GetMetricType());
        }

        [Fact]
        public void PrettyPrint_ShouldReturnIsAbstract()
        {
            Assert.Equal("Is Abstract", new CSharpIsAbstractMetric().PrettyPrint());
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

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            var optional = classModels[0].GetMetricValue<CSharpIsAbstractMetric>();
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

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);
            var optional = classModels[0].GetMetricValue<CSharpIsAbstractMetric>();
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

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);
            var optional = classModels[0].GetMetricValue<CSharpIsAbstractMetric>();
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

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);
            var optional = classModels[0].GetMetricValue<CSharpIsAbstractMetric>();
            Assert.True(optional.HasValue);
            Assert.True((bool) optional.Value);
        }
    }
}
