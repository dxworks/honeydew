using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpParameterDependencyMetricTests
    {
        private readonly CSharpParameterDependencyMetric _sut;
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpParameterDependencyMetricTests()
        {
            _sut = new CSharpParameterDependencyMetric();
            _factExtractor = new CSharpFactExtractor();
            _factExtractor.AddMetric<CSharpParameterDependencyMetric>();
        }

        [Fact]
        public void GetMetricType_ShouldReturnClassLevel()
        {
            Assert.Equal(ExtractionMetricType.ClassLevel, _sut.GetMetricType());
        }

        [Fact]
        public void PrettyPrint_ShouldReturnParameterDependency()
        {
            Assert.Equal("Parameter Dependency", _sut.PrettyPrint());
        }

        [Fact]
        public void Extract_ShouldHaveNoParameters_WhenClassHasMethodsWithNoParameters()
        {
            const string fileContent = @"
                                     namespace App
                                     {                                       

                                         class MyClass
                                         {                                           
                                             public void Foo() { }

                                             public void Foo() { }
                                         }
                                     }";

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpParameterDependencyMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Empty(dependencies);
        }

        [Fact]
        public void Extract_ShouldHaveNoParameters_WhenClassHasConstructorWithNoParameters()
        {
            const string fileContent = @"
                                     namespace App
                                     {                                       
                                         class MyClass
                                         {                                           
                                             public MyClass() { }
                                         }
                                     }";

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpParameterDependencyMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Empty(dependencies);
        }

        [Fact]
        public void Extract_ShouldHavePrimitiveParameters_WhenClassHasMethodsWithPrimitiveParameters()
        {
            const string fileContent = @"using System;

                                     namespace App
                                     {                                       
                                         class MyClass
                                         {                                           
                                             public void Foo(int a, float b, string c) { }

                                             public void Bar(float a, int b) { }

                                             public void Zoo(int a) { }

                                             public void Goo() { }
                                         }
                                     }";


            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpParameterDependencyMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(3, dependencies["int"]);
            Assert.Equal(2, dependencies["float"]);
            Assert.Equal(1, dependencies["string"]);
        }

        [Fact]
        public void Extract_ShouldHavePrimitiveParameters_WhenInterfaceHasMethodsWithPrimitiveParameters()
        {
            const string fileContent = @"using System;

                                     namespace App
                                     {                                       
                                         public interface IInterface
                                         {                                           
                                             public void Foo(int a, float b, string c);

                                             public void Bar(float a, int b);

                                             public string Zoo(int a);

                                             public int Goo();
                                         }
                                     }";

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpParameterDependencyMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(3, dependencies["int"]);
            Assert.Equal(2, dependencies["float"]);
            Assert.Equal(1, dependencies["string"]);
        }

        [Fact]
        public void Extract_ShouldHaveDependenciesParameters_WhenInterfaceHasMethodsWithDependenciesParameters()
        {
            const string fileContent = @"using System;
                                     using HoneydewCore.Extractors;
                                     using HoneydewCore.Extractors.Metrics;
                                     using HoneydewCore.Extractors.Metrics.SemanticMetrics;
                                     namespace App
                                     {                                       
                                         public interface IInterface
                                         {                                           
                                             public void Foo(int a, CSharpMetricExtractor extractor) ;

                                             public void Bar(IFactExtractor factExtractor,  CSharpMetricExtractor extractor) ;
                                         }
                                     }";
            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpParameterDependencyMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(2, dependencies["CSharpMetricExtractor"]);
            Assert.Equal(1, dependencies["IFactExtractor"]);
            Assert.Equal(1, dependencies["int"]);
        }

        [Fact]
        public void Extract_ShouldHaveDependenciesParameters_WhenClassHasMethodsWithDependenciesParameters()
        {
            const string fileContent = @"using System;
                                     using HoneydewCore.Extractors;
                                     using HoneydewCore.Extractors.Metrics;
                                     namespace App
                                     {                                       
                                         public class Class1
                                         {                                           
                                             public void Foo(int a, CSharpMetricExtractor extractor, string name) { }

                                             public void Bar(IFactExtractor factExtractor,  CSharpMetricExtractor extractor, int b) { }
                                         }
                                     }";

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpParameterDependencyMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(4, dependencies.Count);
            Assert.Equal(2, dependencies["CSharpMetricExtractor"]);
            Assert.Equal(1, dependencies["IFactExtractor"]);
            Assert.Equal(2, dependencies["int"]);
            Assert.Equal(1, dependencies["string"]);
        }

        [Fact]
        public void Extract_ShouldHaveDependenciesParameters_WhenClassHasConstructorWithDependenciesParameters()
        {
            const string fileContent = @"using System;
                                     using HoneydewCore.Extractors;
                                     using HoneydewCore.Extractors.Metrics;
                                     namespace App
                                     {                                       
                                         public class Class1
                                         {                                           
                                             public Class1(int a, CSharpMetricExtractor extractor, string name) { }

                                             public Class1(IFactExtractor factExtractor,  CSharpMetricExtractor extractor, int b) { }
                                         }
                                     }";

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpParameterDependencyMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(4, dependencies.Count);
            Assert.Equal(2, dependencies["CSharpMetricExtractor"]);
            Assert.Equal(1, dependencies["IFactExtractor"]);
            Assert.Equal(2, dependencies["int"]);
            Assert.Equal(1, dependencies["string"]);
        }

        [Fact]
        public void GetRelations_ShouldHaveNoRelations_WhenClassHasMethodsWithNoParameters()
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
            Assert.Equal(typeof(CSharpParameterDependencyMetric).FullName, fileRelation1.RelationType);
            Assert.Equal(2, fileRelation1.RelationCount);

            var fileRelation2 = fileRelations[1];
            Assert.Equal("CSharpMetricExtractor", fileRelation2.FileTarget);
            Assert.Equal(typeof(CSharpParameterDependencyMetric).FullName, fileRelation2.RelationType);
            Assert.Equal(1, fileRelation2.RelationCount);
        }
    }
}
