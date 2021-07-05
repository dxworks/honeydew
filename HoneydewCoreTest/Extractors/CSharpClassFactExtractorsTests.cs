using HoneydewCore.Extractors;
using HoneydewCore.Models;
using Xunit;

namespace HoneydewCoreTest.Extractors
{
    public class CSharpClassFactExtractorsTests
    {
        private readonly IFactExtractor _sut;

        public CSharpClassFactExtractorsTests()
        {
            _sut = new CSharpClassFactExtractor();
        }

        [Fact]
        public void FileType_ShouldReturnCS()
        {
            Assert.Equal(".cs", _sut.FileType());
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("  ")]
        [InlineData("\t")]
        [InlineData(null)]
        public void Extract_ShouldThrowEmptyContentException_WhenTryingToExtractFromEmptyString(string emptyContent)
        {
            Assert.Throws<EmptyContentException>(() => _sut.Extract(emptyContent));
        }

        [Theory]
        [InlineData(@"namespace Models
                                    {
                                      public class Item
                                      
                                      }
                                    }
                                    ")]
        [InlineData(@"namespace Models
                                    {
                                      publizzc class Item
                                      {
                                            void a(){ }
                                      }
                                    }
                                    ")]
        [InlineData(@"namespace Models
                                    {
                                      public class Item
                                      {
                                            void a(){ int c}
                                      }
                                    }
                                    ")]
        public void Extract_ShouldThrowExtractionException_WhenParsingTextWithParsingErrors(string fileContent)
        {
            Assert.Throws<ExtractionException>(() => _sut.Extract(fileContent));
        }

        [Theory]
        [InlineData("class")]
        [InlineData("interface")]
        [InlineData("record")]
        [InlineData("struct")]
        [InlineData("enum")]
        public void Extract_ShouldSetClassNameAndNamespace_WhenParsingTextWithOneEntity(string entityType)
        {
            var fileContent = $@"        
                                    namespace Models.Main.Items
                                    {{
                                      public {entityType} MainItem
                                      {{
                                      }}
                                    }}
                                    ";
            var classModels = _sut.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            foreach (var classModel in classModels)
            {
                Assert.Equal(entityType, classModel.ClassType);
                Assert.Equal("Models.Main.Items", classModel.Namespace);
                Assert.Equal("Models.Main.Items.MainItem", classModel.FullName);
            }
        }

        [Theory]
        [InlineData("public", "static")]
        [InlineData("private protected", "sealed")]
        [InlineData("protected internal", "")]
        [InlineData("private", "")]
        [InlineData("protected", "abstract")]
        [InlineData("internal", "new")]
        public void Extract_ShouldSetClassModifiers_WhenParsingTextWithOneEntity(string accessModifier, string modifier)
        {
            var fileContent = $@"        
                                    namespace Models.Main.Items
                                    {{
                                      {accessModifier} {modifier} class MainItem
                                      {{
                                      }}
                                    }}
                                    ";
            var classModels = _sut.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            foreach (var classModel in classModels)
            {
                Assert.Equal("Models.Main.Items", classModel.Namespace);
                Assert.Equal("Models.Main.Items.MainItem", classModel.FullName);
                Assert.Equal(accessModifier, classModel.AccessModifier);
                Assert.Equal(modifier, classModel.Modifier);
            }
        }

        [Fact]
        public void Extract_ShouldNotHaveMetrics_WhenGivenAnEmptyListOfMetrics_ForOneClass()
        {
            const string fileContent = @"    

                                    using System;
                                    using System.Collections.Generic;    
                                    namespace Models
                                    {
                                      public class Item
                                      {
                                      }
                                    }
                                    ";
            var classModels = _sut.Extract(fileContent);

            foreach (var classModel in classModels)
            {
                Assert.Equal(typeof(ClassModel), classModel.GetType());

                Assert.Empty(classModel.Metrics);
            }
        }

        [Fact]
        public void Extract_ShouldSetClassNameAndInterfaceAndNamespace_WhenParsingTextWithOneClassAndOneInterface()
        {
            const string fileContent = @"        
                                    namespace Models.Main.Items
                                    {
                                      public class MainItem
                                      {
                                      }
                                      public interface IInterface
                                      {
                                         void f ();
                                      }
                                    }
                                    ";
            var classModels = _sut.Extract(fileContent);

            Assert.Equal(2, classModels.Count);

            foreach (var classModel in classModels)
            {
                Assert.Equal(typeof(ClassModel), classModel.GetType());
            }

            Assert.Equal("Models.Main.Items", classModels[0].Namespace);
            Assert.Equal("Models.Main.Items.MainItem", classModels[0].FullName);

            Assert.Equal("Models.Main.Items", classModels[1].Namespace);
            Assert.Equal("Models.Main.Items.IInterface", classModels[1].FullName);
        }

        [Fact]
        public void Extract_ShouldHaveNoFields_WhenGivenClassWithMethodsOnly()
        {
            const string fileContent = @"using System;
    
                                     namespace TopLevel
                                     {
                                         public class Foo { public void f() {} }                                        
                                     }";


            var classModels = _sut.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            Assert.Empty(classModels[0].Fields);
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


            var classModels = _sut.Extract(fileContent);

            Assert.Equal(2, classModels.Count);

            foreach (var classModel in classModels)
            {
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


            var classModels = _sut.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            var fieldInfos = classModels[0].Fields;

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


            var classModels = _sut.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            var fieldInfos = classModels[0].Fields;

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


            var classModels = _sut.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            var fieldInfos = classModels[0].Fields;

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


            var classModels = _sut.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            var fieldInfos = classModels[0].Fields;

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

        [Fact]
        public void Extract_ShouldHaveNoMethods_WhenGivenARecordsWithNoMethods()
        {
            const string fileContent = @"using System;
                                     using HoneydewCore.Extractors;
                                     namespace TopLevel
                                     {
                                         public record Foo { readonly int A = 12; volatile float X; static string Y = """";                                                                                                     
                                                          }                                        
                                     }";
            var classModels = _sut.Extract(fileContent);

            Assert.Equal(1, classModels.Count);
            Assert.Empty(classModels[0].Methods);
        }

        [Fact]
        public void Extract_ShouldHaveNoMethods_WhenGivenAStructWithNoMethods()
        {
            const string fileContent = @"using System;
                                     using HoneydewCore.Extractors;
                                     namespace TopLevel
                                     {
                                         public struct Foo {  int A = 12;  float X;  string Y = """";                                                                                                     
                                                          }                                        
                                     }";
            var classModels = _sut.Extract(fileContent);

            Assert.Equal(1, classModels.Count);
            Assert.Empty(classModels[0].Methods);
        }


        [Fact]
        public void Extract_ShouldHaveMethods_WhenGivenAnInterfaceWithMethods()
        {
            const string fileContent = @"using System;
                                     using HoneydewCore.Extractors;
                                     namespace TopLevel
                                     {
                                         public interface Foo { 
                                                           CSharpExtractor f(int a);
                                                            int g(CSharpExtractor a);
                                                            string h(float a,  CSharpExtractor b);                                                                                                    
                                                          }                                        
                                     }";
            var classModels = _sut.Extract(fileContent);

            Assert.Equal(1, classModels.Count);
            Assert.Equal(3, classModels[0].Methods.Count);

            Assert.Equal("f", classModels[0].Methods[0].Name);
            Assert.Equal("CSharpExtractor", classModels[0].Methods[0].ReturnType);
            Assert.Equal(1, classModels[0].Methods[0].ParameterTypes.Count);
            Assert.Equal("int", classModels[0].Methods[0].ParameterTypes[0]);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[0].ContainingClassName);
            Assert.Equal("public", classModels[0].Methods[0].AccessModifier);
            Assert.Equal("abstract", classModels[0].Methods[0].Modifier);
            Assert.Empty(classModels[0].Methods[0].CalledMethods);

            Assert.Equal("g", classModels[0].Methods[1].Name);
            Assert.Equal("int", classModels[0].Methods[1].ReturnType);
            Assert.Equal(1, classModels[0].Methods[1].ParameterTypes.Count);
            Assert.Equal("CSharpExtractor", classModels[0].Methods[1].ParameterTypes[0]);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[1].ContainingClassName);
            Assert.Equal("public", classModels[0].Methods[1].AccessModifier);
            Assert.Equal("abstract", classModels[0].Methods[1].Modifier);
            Assert.Empty(classModels[0].Methods[1].CalledMethods);

            Assert.Equal("h", classModels[0].Methods[2].Name);
            Assert.Equal("string", classModels[0].Methods[2].ReturnType);
            Assert.Equal(2, classModels[0].Methods[2].ParameterTypes.Count);
            Assert.Equal("float", classModels[0].Methods[2].ParameterTypes[0]);
            Assert.Equal("CSharpExtractor", classModels[0].Methods[2].ParameterTypes[1]);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[2].ContainingClassName);
            Assert.Equal("public", classModels[0].Methods[2].AccessModifier);
            Assert.Equal("abstract", classModels[0].Methods[2].Modifier);
            Assert.Empty(classModels[0].Methods[2].CalledMethods);
        }

        [Fact]
        public void Extract_ShouldHaveMethods_WhenGivenAClassWithMethods()
        {
            const string fileContent = @"using System;
                                     using HoneydewCore.Extractors;
                                     namespace TopLevel
                                     {
                                         public class Foo { readonly int A = 12;
                                                           public static void f(int a) {}
                                                            private int g(CSharpExtractor a) { f(0); return 0; }
                                                            protected string h(float a,  CSharpExtractor b) { g(b); f(4); return ""A"";}                                                                                                    
                                                          }                                        
                                     }";
            var classModels = _sut.Extract(fileContent);

            Assert.Equal(1, classModels.Count);
            Assert.Equal(3, classModels[0].Methods.Count);

            Assert.Equal("f", classModels[0].Methods[0].Name);
            Assert.Equal("void", classModels[0].Methods[0].ReturnType);
            Assert.Equal(1, classModels[0].Methods[0].ParameterTypes.Count);
            Assert.Equal("int", classModels[0].Methods[0].ParameterTypes[0]);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[0].ContainingClassName);
            Assert.Equal("public", classModels[0].Methods[0].AccessModifier);
            Assert.Equal("static", classModels[0].Methods[0].Modifier);
            Assert.Empty(classModels[0].Methods[0].CalledMethods);

            Assert.Equal("g", classModels[0].Methods[1].Name);
            Assert.Equal("int", classModels[0].Methods[1].ReturnType);
            Assert.Equal(1, classModels[0].Methods[1].ParameterTypes.Count);
            Assert.Equal("CSharpExtractor", classModels[0].Methods[1].ParameterTypes[0]);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[1].ContainingClassName);
            Assert.Equal("private", classModels[0].Methods[1].AccessModifier);
            Assert.Equal("", classModels[0].Methods[1].Modifier);
            Assert.Equal(1, classModels[0].Methods[1].CalledMethods.Count);
            Assert.Equal("f", classModels[0].Methods[1].CalledMethods[0].MethodName);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[1].CalledMethods[0].ContainingClassName);

            Assert.Equal("h", classModels[0].Methods[2].Name);
            Assert.Equal("string", classModels[0].Methods[2].ReturnType);
            Assert.Equal(2, classModels[0].Methods[2].ParameterTypes.Count);
            Assert.Equal("float", classModels[0].Methods[2].ParameterTypes[0]);
            Assert.Equal("CSharpExtractor", classModels[0].Methods[2].ParameterTypes[1]);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[2].ContainingClassName);
            Assert.Equal("protected", classModels[0].Methods[2].AccessModifier);
            Assert.Equal("", classModels[0].Methods[2].Modifier);
            Assert.Equal(2, classModels[0].Methods[2].CalledMethods.Count);
            Assert.Equal("g", classModels[0].Methods[2].CalledMethods[0].MethodName);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[2].CalledMethods[0].ContainingClassName);
            Assert.Equal("f", classModels[0].Methods[2].CalledMethods[1].MethodName);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[2].CalledMethods[1].ContainingClassName);
        }
    }
}