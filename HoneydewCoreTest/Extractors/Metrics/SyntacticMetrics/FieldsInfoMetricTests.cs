using System;
using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Extractors.Metrics.SemanticMetrics;
using HoneydewCore.Extractors.Metrics.SyntacticMetrics;
using HoneydewCore.Models;
using Xunit;

namespace HoneydewCoreTest.Extractors.Metrics.SyntacticMetrics
{
    public class FieldsInfoMetricTests
    {
        private readonly CSharpMetricExtractor _sut;
        private readonly IFactExtractor _factExtractor;

        public FieldsInfoMetricTests()
        {
            _sut = new FieldsInfoMetric();

            var metrics = new List<Type>
            {
                _sut.GetType()
            };

            _factExtractor = new CSharpClassFactExtractor(metrics);
        }

        [Fact]
        public void GetMetricType_ShouldReturnSemantic()
        {
            Assert.True(_sut is ISemanticMetric);
        }

        [Fact]
        public void PrettyPrint_ShouldReturnFieldsInfo()
        {
            Assert.Equal("Fields Info", _sut.PrettyPrint());
        }


        [Fact]
        public void Extract_ShouldHaveNoFields_WhenGivenAnInterface()
        {
            const string fileContent = @"using System;
    
                                     namespace TopLevel
                                     {
                                         public interface Foo { public void f(); string g(int a); }

                                         public interface Bar { public void f(int a); string g(int a, float b); }                                        
                                     }";


            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(2, classModels.Count);

            foreach (var classModel in classModels)
            {
                var optional = classModel.GetMetricValue<FieldsInfoMetric>();
                Assert.True(optional.HasValue);

                var fieldInfos = (IList<FieldModel>) optional.Value;

                Assert.Empty(fieldInfos);
            }
        }

        [Fact]
        public void Extract_ShouldHavePrivateFieldsWithModifiers_WhenGivenClassWithFieldsAndModifiersWithDefaultAccess()
        {
            const string fileContent = @"using System;
                                     using HoneydewCore.Extractors;
                                     namespace TopLevel
                                     {
                                         public class Foo { readonly int A = 12; volatile float X; static string Y = """";
                                                          void f() {}
                                                          public string g(int a) {return ""Value"";}                                            
                                                          }                                        
                                     }";


            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            var optional = classModels[0].GetMetricValue<FieldsInfoMetric>();
            Assert.True(optional.HasValue);

            var fieldInfos = (IList<FieldModel>) optional.Value;

            Assert.Equal(3, fieldInfos.Count);

            Assert.Equal("A", fieldInfos[0].Name);
            Assert.Equal("int", fieldInfos[0].Type);
            Assert.Equal("readonly", fieldInfos[0].Modifier);
            Assert.Equal("private", fieldInfos[0].AccessModifier);
            Assert.False(fieldInfos[0].IsEvent);

            Assert.Equal("X", fieldInfos[1].Name);
            Assert.Equal("float", fieldInfos[1].Type);
            Assert.Equal("volatile", fieldInfos[1].Modifier);
            Assert.Equal("private", fieldInfos[1].AccessModifier);
            Assert.False(fieldInfos[1].IsEvent);

            Assert.Equal("Y", fieldInfos[2].Name);
            Assert.Equal("string", fieldInfos[2].Type);
            Assert.Equal("static", fieldInfos[2].Modifier);
            Assert.Equal("private", fieldInfos[2].AccessModifier);
            Assert.False(fieldInfos[2].IsEvent);
        }

        [Theory]
        [InlineData("public")]
        [InlineData("private")]
        [InlineData("protected")]
        [InlineData("internal")]
        [InlineData("protected internal")]
        [InlineData("private protected")]
        public void Extract_ShouldHaveFieldsWithNoOtherModifiers_WhenGivenClassWithOnlyFieldsAndTheirModifier(
            string modifier)
        {
            var fileContent = $@"using System;
                                     using HoneydewCore.Extractors;
                                     namespace TopLevel
                                     {{
                                         public class Foo {{ {modifier} int AnimalNest; {modifier} float X,Yaz_fafa; {modifier} string _zxy; {modifier} CSharpMetricExtractor extractor;}}                                        
                                     }}";


            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            var optional = classModels[0].GetMetricValue<FieldsInfoMetric>();
            Assert.True(optional.HasValue);

            var fieldInfos = (IList<FieldModel>) optional.Value;

            Assert.Equal(5, fieldInfos.Count);

            Assert.Equal("AnimalNest", fieldInfos[0].Name);
            Assert.Equal("int", fieldInfos[0].Type);
            Assert.Equal("", fieldInfos[0].Modifier);
            Assert.Equal(modifier, fieldInfos[0].AccessModifier);
            Assert.False(fieldInfos[0].IsEvent);

            Assert.Equal("X", fieldInfos[1].Name);
            Assert.Equal("float", fieldInfos[1].Type);
            Assert.Equal("", fieldInfos[1].Modifier);
            Assert.Equal(modifier, fieldInfos[1].AccessModifier);
            Assert.False(fieldInfos[1].IsEvent);

            Assert.Equal("Yaz_fafa", fieldInfos[2].Name);
            Assert.Equal("float", fieldInfos[2].Type);
            Assert.Equal("", fieldInfos[2].Modifier);
            Assert.Equal(modifier, fieldInfos[2].AccessModifier);
            Assert.False(fieldInfos[2].IsEvent);

            Assert.Equal("_zxy", fieldInfos[3].Name);
            Assert.Equal("string", fieldInfos[3].Type);
            Assert.Equal("", fieldInfos[3].Modifier);
            Assert.Equal(modifier, fieldInfos[3].AccessModifier);
            Assert.False(fieldInfos[3].IsEvent);

            Assert.Equal("extractor", fieldInfos[4].Name);
            Assert.Equal("CSharpMetricExtractor", fieldInfos[4].Type);
            Assert.Equal("", fieldInfos[4].Modifier);
            Assert.Equal(modifier, fieldInfos[4].AccessModifier);
            Assert.False(fieldInfos[4].IsEvent);
        }

        [Theory]
        [InlineData("public")]
        [InlineData("private")]
        [InlineData("protected")]
        [InlineData("internal")]
        [InlineData("protected internal")]
        [InlineData("private protected")]
        public void Extract_ShouldHaveFieldsWithNoOtherModifiers_WhenGivenClassWithOnlyEventFieldsAndTheirModifier(
            string visibility)
        {
            var fileContent = $@"using System;
                                     using HoneydewCore.Extractors;
                                     namespace SomeNamespace
                                     {{
                                         public class Foo {{ {visibility} event CSharpMetricExtractor extractor; {visibility} event int _some_event; {visibility} event Action MyAction1,MyAction2;}}                                        
                                     }}";


            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            var optional = classModels[0].GetMetricValue<FieldsInfoMetric>();
            Assert.True(optional.HasValue);

            var fieldInfos = (IList<FieldModel>) optional.Value;

            Assert.Equal(4, fieldInfos.Count);

            Assert.Equal("extractor", fieldInfos[0].Name);
            Assert.Equal("CSharpMetricExtractor", fieldInfos[0].Type);
            Assert.Equal("", fieldInfos[0].Modifier);
            Assert.Equal(visibility, fieldInfos[0].AccessModifier);
            Assert.True(fieldInfos[0].IsEvent);

            Assert.Equal("_some_event", fieldInfos[1].Name);
            Assert.Equal("int", fieldInfos[1].Type);
            Assert.Equal("", fieldInfos[1].Modifier);
            Assert.Equal(visibility, fieldInfos[1].AccessModifier);
            Assert.True(fieldInfos[1].IsEvent);

            Assert.Equal("MyAction1", fieldInfos[2].Name);
            Assert.Equal("System.Action", fieldInfos[2].Type);
            Assert.Equal("", fieldInfos[2].Modifier);
            Assert.Equal(visibility, fieldInfos[2].AccessModifier);
            Assert.True(fieldInfos[2].IsEvent);

            Assert.Equal("MyAction2", fieldInfos[3].Name);
            Assert.Equal("System.Action", fieldInfos[3].Type);
            Assert.Equal("", fieldInfos[3].Modifier);
            Assert.Equal(visibility, fieldInfos[3].AccessModifier);
            Assert.True(fieldInfos[3].IsEvent);
        }

        [Theory]
        [InlineData("static")]
        [InlineData("volatile")]
        [InlineData("readonly")]
        public void Extract_ShouldHaveFieldsWithNoModifiers_WhenGivenClassWithFieldsAndTheirVisibilityAndMethods(
            string modifier)
        {
            var fileContent = $@"using System;
                                     using HoneydewCore.Extractors;
                                     namespace TopLevel
                                     {{
                                         public class Foo {{ {modifier} public int AnimalNest; protected {modifier} float X,Yaz_fafa; {modifier} string _zxy; {modifier} CSharpMetricExtractor extractor;
                                             void f() {{ AnimalNest=0;}}
                                             }}                                        
                                     }}";


            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            var optional = classModels[0].GetMetricValue<FieldsInfoMetric>();
            Assert.True(optional.HasValue);

            var fieldInfos = (IList<FieldModel>) optional.Value;

            Assert.Equal(5, fieldInfos.Count);

            Assert.Equal("AnimalNest", fieldInfos[0].Name);
            Assert.Equal("int", fieldInfos[0].Type);
            Assert.Equal(modifier, fieldInfos[0].Modifier);
            Assert.Equal("public", fieldInfos[0].AccessModifier);
            Assert.False(fieldInfos[0].IsEvent);

            Assert.Equal("X", fieldInfos[1].Name);
            Assert.Equal("float", fieldInfos[1].Type);
            Assert.Equal(modifier, fieldInfos[1].Modifier);
            Assert.Equal("protected", fieldInfos[1].AccessModifier);
            Assert.False(fieldInfos[1].IsEvent);

            Assert.Equal("Yaz_fafa", fieldInfos[2].Name);
            Assert.Equal("float", fieldInfos[2].Type);
            Assert.Equal(modifier, fieldInfos[2].Modifier);
            Assert.Equal("protected", fieldInfos[2].AccessModifier);
            Assert.False(fieldInfos[2].IsEvent);

            Assert.Equal("_zxy", fieldInfos[3].Name);
            Assert.Equal("string", fieldInfos[3].Type);
            Assert.Equal(modifier, fieldInfos[3].Modifier);
            Assert.Equal("private", fieldInfos[3].AccessModifier);
            Assert.False(fieldInfos[3].IsEvent);

            Assert.Equal("extractor", fieldInfos[4].Name);
            Assert.Equal("CSharpMetricExtractor", fieldInfos[4].Type);
            Assert.Equal(modifier, fieldInfos[4].Modifier);
            Assert.Equal("private", fieldInfos[4].AccessModifier);
            Assert.False(fieldInfos[4].IsEvent);
        }
    }
}