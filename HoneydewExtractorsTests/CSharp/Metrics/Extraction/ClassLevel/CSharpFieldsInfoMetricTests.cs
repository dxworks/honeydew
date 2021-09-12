using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Fields;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Field;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpFieldsInfoMetricTests
    {
        private readonly CSharpFactExtractor _factExtractor;
        private readonly Mock<ILogger> _loggerMock = new();

        public CSharpFieldsInfoMetricTests()
        {
            var compositeVisitor = new CompositeVisitor();

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new FieldSetterClassVisitor(new List<ICSharpFieldVisitor>
                {
                    new FieldInfoVisitor()
                })
            }));

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker(_loggerMock.Object)), compositeVisitor);
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


            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(2, classTypes.Count);

            foreach (var classType in classTypes)
            {
                var classModel = (ClassModel)classType;
                Assert.Empty(classModel.Fields);
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


            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);


            var fieldTypes = ((ClassModel)classTypes[0]).Fields;

            Assert.Equal(3, fieldTypes.Count);

            Assert.Equal("A", fieldTypes[0].Name);
            Assert.Equal("int", fieldTypes[0].Type.Name);
            Assert.Equal("readonly", fieldTypes[0].Modifier);
            Assert.Equal("private", fieldTypes[0].AccessModifier);
            Assert.Equal("TopLevel.Foo", fieldTypes[0].ContainingTypeName);
            Assert.False(fieldTypes[0].IsEvent);

            Assert.Equal("X", fieldTypes[1].Name);
            Assert.Equal("float", fieldTypes[1].Type.Name);
            Assert.Equal("volatile", fieldTypes[1].Modifier);
            Assert.Equal("private", fieldTypes[1].AccessModifier);
            Assert.Equal("TopLevel.Foo", fieldTypes[1].ContainingTypeName);
            Assert.False(fieldTypes[1].IsEvent);

            Assert.Equal("Y", fieldTypes[2].Name);
            Assert.Equal("string", fieldTypes[2].Type.Name);
            Assert.Equal("static", fieldTypes[2].Modifier);
            Assert.Equal("private", fieldTypes[2].AccessModifier);
            Assert.Equal("TopLevel.Foo", fieldTypes[2].ContainingTypeName);
            Assert.False(fieldTypes[2].IsEvent);
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


            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var fieldTypes = ((ClassModel)classTypes[0]).Fields;

            Assert.Equal(5, fieldTypes.Count);

            Assert.Equal("AnimalNest", fieldTypes[0].Name);
            Assert.Equal("int", fieldTypes[0].Type.Name);
            Assert.Equal("", fieldTypes[0].Modifier);
            Assert.Equal(modifier, fieldTypes[0].AccessModifier);
            Assert.False(fieldTypes[0].IsEvent);

            Assert.Equal("X", fieldTypes[1].Name);
            Assert.Equal("float", fieldTypes[1].Type.Name);
            Assert.Equal("", fieldTypes[1].Modifier);
            Assert.Equal(modifier, fieldTypes[1].AccessModifier);
            Assert.False(fieldTypes[1].IsEvent);

            Assert.Equal("Yaz_fafa", fieldTypes[2].Name);
            Assert.Equal("float", fieldTypes[2].Type.Name);
            Assert.Equal("", fieldTypes[2].Modifier);
            Assert.Equal(modifier, fieldTypes[2].AccessModifier);
            Assert.False(fieldTypes[2].IsEvent);

            Assert.Equal("_zxy", fieldTypes[3].Name);
            Assert.Equal("string", fieldTypes[3].Type.Name);
            Assert.Equal("", fieldTypes[3].Modifier);
            Assert.Equal(modifier, fieldTypes[3].AccessModifier);
            Assert.False(fieldTypes[3].IsEvent);

            Assert.Equal("extractor", fieldTypes[4].Name);
            Assert.Equal("CSharpMetricExtractor", fieldTypes[4].Type.Name);
            Assert.Equal("", fieldTypes[4].Modifier);
            Assert.Equal(modifier, fieldTypes[4].AccessModifier);
            Assert.False(fieldTypes[4].IsEvent);
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


            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var fieldTypes = ((ClassModel)classTypes[0]).Fields;

            Assert.Equal(4, fieldTypes.Count);

            Assert.Equal("extractor", fieldTypes[0].Name);
            Assert.Equal("CSharpMetricExtractor", fieldTypes[0].Type.Name);
            Assert.Equal("", fieldTypes[0].Modifier);
            Assert.Equal(visibility, fieldTypes[0].AccessModifier);
            Assert.True(fieldTypes[0].IsEvent);

            Assert.Equal("_some_event", fieldTypes[1].Name);
            Assert.Equal("int", fieldTypes[1].Type.Name);
            Assert.Equal("", fieldTypes[1].Modifier);
            Assert.Equal(visibility, fieldTypes[1].AccessModifier);
            Assert.True(fieldTypes[1].IsEvent);

            Assert.Equal("MyAction1", fieldTypes[2].Name);
            Assert.Equal("System.Action", fieldTypes[2].Type.Name);
            Assert.Equal("", fieldTypes[2].Modifier);
            Assert.Equal(visibility, fieldTypes[2].AccessModifier);
            Assert.True(fieldTypes[2].IsEvent);

            Assert.Equal("MyAction2", fieldTypes[3].Name);
            Assert.Equal("System.Action", fieldTypes[3].Type.Name);
            Assert.Equal("", fieldTypes[3].Modifier);
            Assert.Equal(visibility, fieldTypes[3].AccessModifier);
            Assert.True(fieldTypes[3].IsEvent);
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


            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var fieldTypes = ((ClassModel)classTypes[0]).Fields;

            Assert.Equal(5, fieldTypes.Count);

            Assert.Equal("AnimalNest", fieldTypes[0].Name);
            Assert.Equal("int", fieldTypes[0].Type.Name);
            Assert.Equal(modifier, fieldTypes[0].Modifier);
            Assert.Equal("public", fieldTypes[0].AccessModifier);
            Assert.False(fieldTypes[0].IsEvent);

            Assert.Equal("X", fieldTypes[1].Name);
            Assert.Equal("float", fieldTypes[1].Type.Name);
            Assert.Equal(modifier, fieldTypes[1].Modifier);
            Assert.Equal("protected", fieldTypes[1].AccessModifier);
            Assert.False(fieldTypes[1].IsEvent);

            Assert.Equal("Yaz_fafa", fieldTypes[2].Name);
            Assert.Equal("float", fieldTypes[2].Type.Name);
            Assert.Equal(modifier, fieldTypes[2].Modifier);
            Assert.Equal("protected", fieldTypes[2].AccessModifier);
            Assert.False(fieldTypes[2].IsEvent);

            Assert.Equal("_zxy", fieldTypes[3].Name);
            Assert.Equal("string", fieldTypes[3].Type.Name);
            Assert.Equal(modifier, fieldTypes[3].Modifier);
            Assert.Equal("private", fieldTypes[3].AccessModifier);
            Assert.False(fieldTypes[3].IsEvent);

            Assert.Equal("extractor", fieldTypes[4].Name);
            Assert.Equal("CSharpMetricExtractor", fieldTypes[4].Type.Name);
            Assert.Equal(modifier, fieldTypes[4].Modifier);
            Assert.Equal("private", fieldTypes[4].AccessModifier);
            Assert.False(fieldTypes[4].IsEvent);
        }
    }
}
