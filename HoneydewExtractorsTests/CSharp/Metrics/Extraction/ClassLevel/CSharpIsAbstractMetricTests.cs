using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpIsAbstractMetricTests
    {
        private readonly CSharpFactExtractor _factExtractor;
        private readonly Mock<ILogger> _loggerMock = new();

        public CSharpIsAbstractMetricTests()
        {
            var compositeVisitor = new CompositeVisitor();
            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new IsAbstractClassVisitor()
            }));

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker(_loggerMock.Object)), compositeVisitor);
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

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);
            Assert.Equal(1, classTypes[0].Metrics.Count);
            var metricModel = classTypes[0].Metrics[0];
            Assert.Equal("System.Boolean", metricModel.ValueType);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.IsAbstractClassVisitor",
                metricModel.ExtractorName);
            Assert.False((bool)metricModel.Value);
        }

        [Fact]
        public void Extract_ShouldReturnFalse_WhenGivenClassWithBaseClassAndInterface()
        {
            const string fileContent = @"
                                     namespace Bar
                                     {
                                         public class Foo : SomeClass, ISomeInterface { int a; public void f(){} }                                        
                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);
            Assert.Equal(1, classTypes[0].Metrics.Count);
            var metricModel = classTypes[0].Metrics[0];
            Assert.Equal("System.Boolean", metricModel.ValueType);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.IsAbstractClassVisitor",
                metricModel.ExtractorName);
            Assert.False((bool)metricModel.Value);
        }

        [Fact]
        public void Extract_ShouldReturnTrue_WhenGivenInterface()
        {
            const string fileContent = @"
                                     namespace Bar
                                     {
                                         public interface Foo { public void f(); }                                        
                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);
            Assert.Equal(1, classTypes[0].Metrics.Count);
            var metricModel = classTypes[0].Metrics[0];
            Assert.Equal("System.Boolean", metricModel.ValueType);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.IsAbstractClassVisitor",
                metricModel.ExtractorName);
            Assert.True((bool)metricModel.Value);
        }

        [Fact]
        public void Extract_ShouldReturnTrue_WhenGivenAbstractClass()
        {
            const string fileContent = @"
                                     namespace Bar
                                     {
                                         public abstract class Foo { public void g(); }                                        
                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);
            Assert.Equal(1, classTypes[0].Metrics.Count);
            var metricModel = classTypes[0].Metrics[0];
            Assert.Equal("System.Boolean", metricModel.ValueType);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.IsAbstractClassVisitor",
                metricModel.ExtractorName);
            Assert.True((bool)metricModel.Value);
        }
    }
}
