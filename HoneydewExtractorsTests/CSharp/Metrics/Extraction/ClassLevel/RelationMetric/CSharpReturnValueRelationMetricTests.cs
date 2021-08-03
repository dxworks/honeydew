using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel.RelationMetric
{
    public class CSharpReturnValueRelationMetricTests
    {
        private readonly CSharpReturnValueRelationMetric _sut;
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpReturnValueRelationMetricTests()
        {
            _sut = new CSharpReturnValueRelationMetric();
            _factExtractor = new CSharpFactExtractor();
            _factExtractor.AddMetric<CSharpReturnValueRelationMetric>();
        }

        [Fact]
        public void GetMetricType_ShouldReturnClassLevel()
        {
            Assert.Equal(ExtractionMetricType.ClassLevel, _sut.GetMetricType());
        }

        [Fact]
        public void PrettyPrint_ShouldReturnReturnValueDependency()
        {
            Assert.Equal("Return Value Dependency", _sut.PrettyPrint());
        }

        [Fact]
        public void Extract_ShouldHaveVoidReturnValues_WhenClassHasMethodsThatReturnVoid()
        {
            const string fileContent = @"
                                     namespace App
                                     {                                       

                                         class MyClass
                                         {                                           
                                             public void Foo() { }

                                             public void Bar() { }
                                         }
                                     }";

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpReturnValueRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(1, dependencies.Count);
            Assert.Equal(2, dependencies["void"]);
        }

        [Fact]
        public void Extract_ShouldHavePrimitiveReturnValues_WhenClassHasMethodsThatReturnPrimitiveValues()
        {
            const string fileContent = @"using System;

                                     namespace App
                                     {                                       
                                         class MyClass
                                         {                                           
                                             public int Foo(int a, float b, string c) { }

                                             public float Bar(float a, int b) { }

                                             public int Zoo(int a) { }

                                             public string Goo() { }
                                         }
                                     }";

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpReturnValueRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(2, dependencies["int"]);
            Assert.Equal(1, dependencies["float"]);
            Assert.Equal(1, dependencies["string"]);
        }

        [Fact]
        public void Extract_ShouldHavePrimitiveReturnValues_WhenInterfaceHasMethodsWithPrimitiveReturnValues()
        {
            const string fileContent = @"using System;

                                     namespace App
                                     {                                       
                                         public interface IInterface
                                         {                                           
                                             public float Foo(int a, float b, string c);

                                             public void Bar(float a, int b);

                                             public string Zoo(int a);

                                             public int Goo();
                                         }
                                     }";

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpReturnValueRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(4, dependencies.Count);
            Assert.Equal(1, dependencies["int"]);
            Assert.Equal(1, dependencies["float"]);
            Assert.Equal(1, dependencies["string"]);
            Assert.Equal(1, dependencies["void"]);
        }

        [Fact]
        public void Extract_ShouldHaveDependenciesReturnValues_WhenInterfaceHasMethodsWithDependenciesReturnValues()
        {
            const string fileContent = @"using System;
                                     using HoneydewCore.Extractors;
                                     using HoneydewCore.Extractors.Metrics;
                                     using HoneydewCore.Extractors.Metrics.SemanticMetrics;
                                     namespace App
                                     {                                       
                                         public interface IInterface
                                         {                                           
                                             public CSharpMetricExtractor Foo(int a, CSharpMetricExtractor extractor) ;

                                             public CSharpMetricExtractor Foo(int a) ;

                                             public IFactExtractor Bar(CSharpMetricExtractor extractor) ;
                                         }
                                     }";

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpReturnValueRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(2, dependencies.Count);
            Assert.Equal(2, dependencies["CSharpMetricExtractor"]);
            Assert.Equal(1, dependencies["IFactExtractor"]);
        }

        [Fact]
        public void Extract_ShouldHaveDependenciesReturnValues_WhenClassHasMethodsWithDependenciesReturnValues()
        {
            const string fileContent = @"using System;
                                     using HoneydewCore.Extractors;
                                     using HoneydewCore.Extractors.Metrics;
                                     namespace App
                                     {                                       
                                         public class IInterface
                                         {                                           
                                             public CSharpMetricExtractor Foo(int a, string name) { }

                                             public IFactExtractor Bar(CSharpMetricExtractor extractor, int b) { }

                                             public IFactExtractor Goo(CSharpMetricExtractor extractor) { }
                                         }
                                     }";

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpReturnValueRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(2, dependencies.Count);
            Assert.Equal(1, dependencies["CSharpMetricExtractor"]);
            Assert.Equal(2, dependencies["IFactExtractor"]);
        }

        [Fact]
        public void GetRelations_ShouldHaveNoRelations_WhenClassHasMethodsWithNoReturnValues()
        {
            var fileRelations = _sut.GetRelations(new Dictionary<string, int>());

            Assert.Empty(fileRelations);
        }

        [Fact]
        public void GetRelations_ShouldHaveNoRelations_WhenDependenciesAreOnlyPrimitiveTypes()
        {
            var fileRelations = _sut.GetRelations(new Dictionary<string, int>
            {
                {"int", 3},
                {"float", 2},
                {"string", 1}
            });

            Assert.Empty(fileRelations);
        }

        [Fact]
        public void GetRelations_Extract_ShouldHaveRelations_WhenThereAreNonPrimitiveDependencies()
        {
            var fileRelations = _sut.GetRelations(new Dictionary<string, int>
            {
                {"int", 3},
                {"IFactExtractor", 2},
                {"CSharpMetricExtractor", 1}
            });

            Assert.NotEmpty(fileRelations);
            Assert.Equal(2, fileRelations.Count);

            var fileRelation1 = fileRelations[0];
            Assert.Equal("IFactExtractor", fileRelation1.FileTarget);
            Assert.Equal(typeof(CSharpReturnValueRelationMetric).FullName, fileRelation1.RelationType);
            Assert.Equal(2, fileRelation1.RelationCount);

            var fileRelation2 = fileRelations[1];
            Assert.Equal("CSharpMetricExtractor", fileRelation2.FileTarget);
            Assert.Equal(typeof(CSharpReturnValueRelationMetric).FullName, fileRelation2.RelationType);
            Assert.Equal(1, fileRelation2.RelationCount);
        }
    }
}
