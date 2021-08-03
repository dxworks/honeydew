using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel.RelationMetric
{
    public class CSharpFieldsRelationMetricTests
    {
        private readonly CSharpFieldsRelationMetric _sut;
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpFieldsRelationMetricTests()
        {
            _sut = new CSharpFieldsRelationMetric();
            _factExtractor = new CSharpFactExtractor();
            _factExtractor.AddMetric<CSharpFieldsRelationMetric>();
        }

        [Fact]
        public void GetMetricType_ShouldReturnClassLevel()
        {
            Assert.Equal(ExtractionMetricType.ClassLevel, _sut.GetMetricType());
        }

        [Fact]
        public void PrettyPrint_ShouldReturnReturnValueDependency()
        {
            Assert.Equal("Fields Dependency", _sut.PrettyPrint());
        }

        [Theory]
        [InlineData("class")]
        [InlineData("record")]
        [InlineData("struct")]
        public void Extract_ShouldHavePrimitiveFields_WhenClassHasFieldsOfPrimitiveTypes(string classType)
        {
            var fileContent = $@"using System;

                                     namespace App
                                     {{                                       
                                         {classType} MyClass
                                         {{                                           
                                             public int Foo;

                                             private float Bar;

                                             protected int Zoo;

                                             internal string Goo;
                                         }}
                                     }}";

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpFieldsRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(2, dependencies["int"]);
            Assert.Equal(1, dependencies["float"]);
            Assert.Equal(1, dependencies["string"]);
        }
        
        [Theory]
        [InlineData("class")]
        [InlineData("record")]
        [InlineData("struct")]
        public void Extract_ShouldHavePrimitiveFields_WhenClassHasEventFieldsOfPrimitiveTypes(string classType)
        {
            var fileContent = $@"using System;
                                     namespace App
                                     {{                                       
                                        {classType} MyClass
                                        {{
                                            public event Func<int> Foo;
                                            
                                            public event Action<string> Bar;
                                        }}
                                     }}";

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpFieldsRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(2, dependencies.Count);
            Assert.Equal(1, dependencies["System.Func<int>"]);
            Assert.Equal(1, dependencies["System.Action<string>"]);
        }

        [Theory]
        [InlineData("class")]
        [InlineData("record")]
        [InlineData("struct")]
        public void Extract_ShouldHaveDependenciesFields_WhenClassHasFields(string classType)
        {
            var fileContent = $@"using System;
                                     using HoneydewCore.Extractors;
                                     using HoneydewCore.Extractors.Metrics;
                                     using HoneydewCore.Extractors.Metrics.SemanticMetrics;
                                     namespace App
                                     {{                                       
                                         public {classType} IInterface
                                         {{                                           
                                             public CSharpMetricExtractor Foo;

                                             private CSharpMetricExtractor Foo2 ;

                                             protected IFactExtractor Bar;
                                         }}
                                     }}";

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpFieldsRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(2, dependencies.Count);
            Assert.Equal(2, dependencies["CSharpMetricExtractor"]);
            Assert.Equal(1, dependencies["IFactExtractor"]);
        }

        [Theory]
        [InlineData("class")]
        [InlineData("record")]
        [InlineData("struct")]
        public void Extract_ShouldHaveDependenciesEventFields_WhenClassHasEventFields(string classType)
        {
            var fileContent = $@"using System;
                                     using HoneydewCore.Extractors;
                                     using HoneydewCore.Extractors.Metrics;
                                     namespace App
                                     {{                                       
                                         public {classType} IInterface
                                         {{                                           
                                             internal event Func<CSharpMetricExtractor> Foo;

                                             public event Action<IFactExtractor> Bar;

                                             private event Func<IFactExtractor,CSharpMetricExtractor> Goo;
                                         }}
                                     }}";

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpFieldsRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(1, dependencies["System.Func<CSharpMetricExtractor>"]);
            Assert.Equal(1, dependencies["System.Action<IFactExtractor>"]);
            Assert.Equal(1, dependencies["System.Func<IFactExtractor, CSharpMetricExtractor>"]);
        }

        [Fact]
        public void GetRelations_ShouldHaveNoRelations_WhenClassHasNoFields()
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
            Assert.Equal(typeof(CSharpFieldsRelationMetric).FullName, fileRelation1.RelationType);
            Assert.Equal(2, fileRelation1.RelationCount);

            var fileRelation2 = fileRelations[1];
            Assert.Equal("CSharpMetricExtractor", fileRelation2.FileTarget);
            Assert.Equal(typeof(CSharpFieldsRelationMetric).FullName, fileRelation2.RelationType);
            Assert.Equal(1, fileRelation2.RelationCount);
        }
    }
}
