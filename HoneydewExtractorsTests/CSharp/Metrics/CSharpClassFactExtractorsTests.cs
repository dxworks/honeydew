using System.Collections.Generic;
using HoneydewExtractors.Core;
using HoneydewExtractors.Core.Metrics.Extraction.Class;
using HoneydewExtractors.Core.Metrics.Extraction.Common;
using HoneydewExtractors.Core.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.Core.Metrics.Extraction.Constructor;
using HoneydewExtractors.Core.Metrics.Extraction.Field;
using HoneydewExtractors.Core.Metrics.Extraction.Method;
using HoneydewExtractors.Core.Metrics.Extraction.MethodCall;
using HoneydewExtractors.Core.Metrics.Extraction.Property;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Fields;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.CSharp;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics
{
    public class CSharpClassFactExtractorsTests
    {
        private readonly CSharpFactExtractor _sut;

        public CSharpClassFactExtractorsTests()
        {
            var compositeVisitor = new CompositeVisitor();
            
            var calledMethodSetterVisitor = new CalledMethodSetterVisitor(new List<ICSharpMethodSignatureVisitor>
            {
                new MethodCallInfoVisitor()
            });
            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new MethodSetterClassVisitor(new List<ICSharpMethodVisitor>
                {
                    new MethodInfoVisitor(),
                    calledMethodSetterVisitor
                }),
                new ConstructorSetterClassVisitor(new List<ICSharpConstructorVisitor>
                {
                    new ConstructorInfoVisitor(),
                    calledMethodSetterVisitor
                }),
                new FieldSetterClassVisitor(new List<ICSharpFieldVisitor>
                {
                    new FieldInfoVisitor()
                }),
                new PropertySetterClassVisitor(new List<ICSharpPropertyVisitor>
                {
                    new PropertyInfoVisitor()
                })
            }));

            _sut = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker()), compositeVisitor);
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
            var extractionException = Assert.Throws<ExtractionException>(() => _sut.Extract(emptyContent));
            Assert.Equal("Empty Content", extractionException.Message);
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
            Assert.Throws<ExtractionException>(() => _sut.Extract(fileContent).ClassTypes);
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
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            foreach (var classType in classTypes)
            {
                var classModel = (ClassModel)classType;
                Assert.Equal(entityType, classModel.ClassType);
                Assert.Equal("Models.Main.Items", classModel.Namespace);
                Assert.Equal("Models.Main.Items.MainItem", classModel.Name);
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
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            foreach (var classType in classTypes)
            {
                var classModel = (ClassModel)classType;
                Assert.Equal("Models.Main.Items", classModel.ContainingTypeName);
                Assert.Equal("Models.Main.Items", classModel.Namespace);
                Assert.Equal("Models.Main.Items.MainItem", classModel.Name);
                Assert.Equal(accessModifier, classModel.AccessModifier);
                Assert.Equal(modifier, classModel.Modifier);
            }
        }

        [Theory]
        [InlineData("in")]
        [InlineData("out")]
        [InlineData("ref")]
        public void Extract_ShouldSetParameters_WhenParsingTextWithOneClassWithMethodWithParameterWithModifiers(
            string parameterModifier)
        {
            var fileContent = $@"        
                                     namespace Models.Main.Items
                                     {{
                                       public class MainItem
                                       {{
                                             public void Method({parameterModifier} int a) {{}}

                                             public MainItem({parameterModifier} int a) {{ }}
                                       }}
                                     }}
                                     ";
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            foreach (var classType in classTypes)
            {
                var classModel = (ClassModel)classType;
                Assert.Equal("Models.Main.Items", classModel.Namespace);
                Assert.Equal("Models.Main.Items.MainItem", classModel.Name);

                Assert.Equal(1, classModel.Methods.Count);
                Assert.Equal(1, classModel.Methods[0].ParameterTypes.Count);
                var parameterModel = (ParameterModel)classModel.Methods[0].ParameterTypes[0];
                Assert.Equal("int", parameterModel.Type.Name);
                Assert.Equal(parameterModifier, parameterModel.Modifier);
                Assert.Null(parameterModel.DefaultValue);

                Assert.Equal(1, classModel.Constructors.Count);
                Assert.Equal(1, classModel.Constructors[0].ParameterTypes.Count);
                var parameterModelConstructor = (ParameterModel)classModel.Constructors[0].ParameterTypes[0];
                Assert.Equal("int", parameterModelConstructor.Type.Name);
                Assert.Equal(parameterModifier, parameterModelConstructor.Modifier);
                Assert.Null(parameterModelConstructor.DefaultValue);
            }
        }

        [Fact]
        public void Extract_ShouldSetParameters_WhenParsingTextWithOneClassWithExtensionMethod()
        {
            const string fileContent = @"        
                                     namespace Models.Main.Items
                                     {
                                       public class MainItem
                                       {
                                             public void Method(this int a) {}
                                       }
                                     }
                                     ";
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            foreach (var classType in classTypes)
            {
                var classModel = (ClassModel)classType;
                Assert.Equal("Models.Main.Items", classModel.Namespace);
                Assert.Equal("Models.Main.Items.MainItem", classModel.Name);
                Assert.Equal(1, classModel.Methods.Count);
                Assert.Equal(1, classModel.Methods[0].ParameterTypes.Count);
                var parameterModel = (ParameterModel)classModel.Methods[0].ParameterTypes[0];
                Assert.Equal("int", parameterModel.Type.Name);
                Assert.Equal("this", parameterModel.Modifier);
                Assert.Null(parameterModel.DefaultValue);
            }
        }

        [Fact]
        public void Extract_ShouldSetParameters_WhenParsingTextWithOneClassWithMethodWithParameterWithParamsModifiers()
        {
            var fileContent = @"        
                                     namespace Models.Main.Items
                                     {
                                       public class MainItem
                                       {
                                             public void Method(params int[] a) {}

                                             public MainItem(params int[] a) {}
                                       }
                                     }
                                     ";
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            foreach (var classType in classTypes)
            {
                var classModel = (ClassModel)classType;
                Assert.Equal("Models.Main.Items", classModel.Namespace);
                Assert.Equal("Models.Main.Items.MainItem", classModel.Name);

                Assert.Equal(1, classModel.Methods.Count);
                Assert.Equal(1, classModel.Methods[0].ParameterTypes.Count);
                var parameterModel = (ParameterModel)classModel.Methods[0].ParameterTypes[0];
                Assert.Equal("int[]", parameterModel.Type.Name);
                Assert.Equal("params", parameterModel.Modifier);
                Assert.Null(parameterModel.DefaultValue);

                Assert.Equal(1, classModel.Constructors.Count);
                Assert.Equal(1, classModel.Constructors[0].ParameterTypes.Count);
                var parameterModelConstructor = (ParameterModel)classModel.Constructors[0].ParameterTypes[0];
                Assert.Equal("int[]", parameterModelConstructor.Type.Name);
                Assert.Equal("params", parameterModelConstructor.Modifier);
                Assert.Null(parameterModelConstructor.DefaultValue);
            }
        }

        [Fact]
        public void Extract_ShouldSetParameters_WhenParsingTextWithOneClassWithMethodWithParameterWithDefaultValues()
        {
            var fileContent = @"        
                                     namespace Models.Main.Items
                                     {
                                       public class MainItem
                                       {
                                             public void Method() {}
                                             public void Method1(object a=null) {}
                                             public void Method2(int a=15) {}
                                             public void Method3(int a, int b) {}
                                             public void Method4(int a,in int b=15, string c="""") {}
                                             public void Method5(string c=""null"") {}
                                       }
                                     }
                                     ";
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            foreach (var classType in classTypes)
            {
                var classModel = (ClassModel)classType;
                Assert.Equal(6, classModel.Methods.Count);

                Assert.Empty(classModel.Methods[0].ParameterTypes);

                Assert.Equal(1, classModel.Methods[1].ParameterTypes.Count);
                var method1Parameter = (ParameterModel)classModel.Methods[1].ParameterTypes[0];
                Assert.Equal("object", method1Parameter.Type.Name);
                Assert.Equal("", method1Parameter.Modifier);
                Assert.Equal("null", method1Parameter.DefaultValue);

                Assert.Equal(1, classModel.Methods[2].ParameterTypes.Count);
                var method2Parameter = (ParameterModel)classModel.Methods[2].ParameterTypes[0];
                Assert.Equal("int", method2Parameter.Type.Name);
                Assert.Equal("", method2Parameter.Modifier);
                Assert.Equal("15", method2Parameter.DefaultValue);

                Assert.Equal(2, classModel.Methods[3].ParameterTypes.Count);
                foreach (var parameterType in classModel.Methods[3].ParameterTypes)
                {
                    var parameterModel = (ParameterModel)parameterType;
                    Assert.Equal("int", parameterModel.Type.Name);
                    Assert.Equal("", parameterModel.Modifier);
                    Assert.Null(parameterModel.DefaultValue);
                }

                Assert.Equal(3, classModel.Methods[4].ParameterTypes.Count);
                var method4Parameter0 = (ParameterModel)classModel.Methods[4].ParameterTypes[0];
                Assert.Equal("int", method4Parameter0.Type.Name);
                Assert.Equal("", method4Parameter0.Modifier);
                Assert.Null(method4Parameter0.DefaultValue);

                var method4Parameter1 = (ParameterModel)classModel.Methods[4].ParameterTypes[1];
                Assert.Equal("int", method4Parameter1.Type.Name);
                Assert.Equal("in", method4Parameter1.Modifier);
                Assert.Equal("15", method4Parameter1.DefaultValue);

                var method4Parameter2 = (ParameterModel)classModel.Methods[4].ParameterTypes[2];
                Assert.Equal("string", method4Parameter2.Type.Name);
                Assert.Equal("", method4Parameter2.Modifier);
                Assert.Equal("\"\"", method4Parameter2.DefaultValue);

                Assert.Equal(1, classModel.Methods[5].ParameterTypes.Count);
                var method5Parameter = (ParameterModel)classModel.Methods[5].ParameterTypes[0];
                Assert.Equal("string", method5Parameter.Type.Name);
                Assert.Equal("", method5Parameter.Modifier);
                Assert.Equal("\"null\"", method5Parameter.DefaultValue);
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
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            foreach (var classType in classTypes)
            {
                var classModel = (ClassModel)classType;
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
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(2, classTypes.Count);

            foreach (var classType in classTypes)
            {
                var classModel = (ClassModel)classType;
                Assert.Equal(typeof(ClassModel), classModel.GetType());
            }

            Assert.Equal("Models.Main.Items", ((ClassModel)classTypes[0]).Namespace);
            Assert.Equal("Models.Main.Items.MainItem", classTypes[0].Name);

            Assert.Equal("Models.Main.Items", ((ClassModel)classTypes[1]).Namespace);
            Assert.Equal("Models.Main.Items.IInterface", classTypes[1].Name);
        }

        [Fact]
        public void Extract_ShouldHaveNoFields_WhenGivenClassWithMethodsOnly()
        {
            const string fileContent = @"using System;
     
                                      namespace TopLevel
                                      {
                                          public class Foo { public void f() {} }                                        
                                      }";


            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            Assert.Empty(((ClassModel)classTypes[0]).Fields);
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


            var classTypes = _sut.Extract(fileContent).ClassTypes;

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


            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var fieldInfos = ((ClassModel)classTypes[0]).Fields;

            Assert.Equal(3, fieldInfos.Count);

            Assert.Equal("A", fieldInfos[0].Name);
            Assert.Equal("int", fieldInfos[0].Type.Name);
            Assert.Equal("readonly", fieldInfos[0].Modifier);
            Assert.Equal("private", fieldInfos[0].AccessModifier);
            Assert.False(fieldInfos[0].IsEvent);

            Assert.Equal("X", fieldInfos[1].Name);
            Assert.Equal("float", fieldInfos[1].Type.Name);
            Assert.Equal("volatile", fieldInfos[1].Modifier);
            Assert.Equal("private", fieldInfos[1].AccessModifier);
            Assert.False(fieldInfos[1].IsEvent);

            Assert.Equal("Y", fieldInfos[2].Name);
            Assert.Equal("string", fieldInfos[2].Type.Name);
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


            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var fieldInfos = ((ClassModel)classTypes[0]).Fields;

            Assert.Equal(5, fieldInfos.Count);

            Assert.Equal("AnimalNest", fieldInfos[0].Name);
            Assert.Equal("int", fieldInfos[0].Type.Name);
            Assert.Equal("", fieldInfos[0].Modifier);
            Assert.Equal(modifier, fieldInfos[0].AccessModifier);
            Assert.False(fieldInfos[0].IsEvent);

            Assert.Equal("X", fieldInfos[1].Name);
            Assert.Equal("float", fieldInfos[1].Type.Name);
            Assert.Equal("", fieldInfos[1].Modifier);
            Assert.Equal(modifier, fieldInfos[1].AccessModifier);
            Assert.False(fieldInfos[1].IsEvent);

            Assert.Equal("Yaz_fafa", fieldInfos[2].Name);
            Assert.Equal("float", fieldInfos[2].Type.Name);
            Assert.Equal("", fieldInfos[2].Modifier);
            Assert.Equal(modifier, fieldInfos[2].AccessModifier);
            Assert.False(fieldInfos[2].IsEvent);

            Assert.Equal("_zxy", fieldInfos[3].Name);
            Assert.Equal("string", fieldInfos[3].Type.Name);
            Assert.Equal("", fieldInfos[3].Modifier);
            Assert.Equal(modifier, fieldInfos[3].AccessModifier);
            Assert.False(fieldInfos[3].IsEvent);

            Assert.Equal("extractor", fieldInfos[4].Name);
            Assert.Equal("CSharpMetricExtractor", fieldInfos[4].Type.Name);
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


            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var fieldInfos = ((ClassModel)classTypes[0]).Fields;

            Assert.Equal(4, fieldInfos.Count);

            Assert.Equal("extractor", fieldInfos[0].Name);
            Assert.Equal("CSharpMetricExtractor", fieldInfos[0].Type.Name);
            Assert.Equal("", fieldInfos[0].Modifier);
            Assert.Equal(visibility, fieldInfos[0].AccessModifier);
            Assert.True(fieldInfos[0].IsEvent);

            Assert.Equal("_some_event", fieldInfos[1].Name);
            Assert.Equal("int", fieldInfos[1].Type.Name);
            Assert.Equal("", fieldInfos[1].Modifier);
            Assert.Equal(visibility, fieldInfos[1].AccessModifier);
            Assert.True(fieldInfos[1].IsEvent);

            Assert.Equal("MyAction1", fieldInfos[2].Name);
            Assert.Equal("System.Action", fieldInfos[2].Type.Name);
            Assert.Equal("", fieldInfos[2].Modifier);
            Assert.Equal(visibility, fieldInfos[2].AccessModifier);
            Assert.True(fieldInfos[2].IsEvent);

            Assert.Equal("MyAction2", fieldInfos[3].Name);
            Assert.Equal("System.Action", fieldInfos[3].Type.Name);
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


            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var fieldInfos = ((ClassModel)classTypes[0]).Fields;

            Assert.Equal(5, fieldInfos.Count);

            Assert.Equal("AnimalNest", fieldInfos[0].Name);
            Assert.Equal("int", fieldInfos[0].Type.Name);
            Assert.Equal(modifier, fieldInfos[0].Modifier);
            Assert.Equal("public", fieldInfos[0].AccessModifier);
            Assert.False(fieldInfos[0].IsEvent);

            Assert.Equal("X", fieldInfos[1].Name);
            Assert.Equal("float", fieldInfos[1].Type.Name);
            Assert.Equal(modifier, fieldInfos[1].Modifier);
            Assert.Equal("protected", fieldInfos[1].AccessModifier);
            Assert.False(fieldInfos[1].IsEvent);

            Assert.Equal("Yaz_fafa", fieldInfos[2].Name);
            Assert.Equal("float", fieldInfos[2].Type.Name);
            Assert.Equal(modifier, fieldInfos[2].Modifier);
            Assert.Equal("protected", fieldInfos[2].AccessModifier);
            Assert.False(fieldInfos[2].IsEvent);

            Assert.Equal("_zxy", fieldInfos[3].Name);
            Assert.Equal("string", fieldInfos[3].Type.Name);
            Assert.Equal(modifier, fieldInfos[3].Modifier);
            Assert.Equal("private", fieldInfos[3].AccessModifier);
            Assert.False(fieldInfos[3].IsEvent);

            Assert.Equal("extractor", fieldInfos[4].Name);
            Assert.Equal("CSharpMetricExtractor", fieldInfos[4].Type.Name);
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
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);
            Assert.Empty(((ClassModel)classTypes[0]).Methods);
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
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);
            Assert.Empty(((ClassModel)classTypes[0]).Methods);
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
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);
            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(3, classModel.Methods.Count);

            var methodF = classModel.Methods[0];
            Assert.Equal("f", methodF.Name);
            Assert.Equal("CSharpExtractor", methodF.ReturnValue.Type.Name);
            Assert.Equal("", ((ReturnValueModel)methodF.ReturnValue).Modifier);
            Assert.Equal(1, methodF.ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)methodF.ParameterTypes[0];
            Assert.Equal("int", parameterModel1.Type.Name);
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Null(parameterModel1.DefaultValue);
            Assert.Equal("TopLevel.Foo", methodF.ContainingTypeName);
            Assert.Equal("public", methodF.AccessModifier);
            Assert.Equal("abstract", methodF.Modifier);
            Assert.Empty(methodF.CalledMethods);

            var methodG = classModel.Methods[1];
            Assert.Equal("g", methodG.Name);
            Assert.Equal("int", methodG.ReturnValue.Type.Name);
            Assert.Equal("", ((ReturnValueModel)methodG.ReturnValue).Modifier);
            Assert.Equal(1, methodG.ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)methodG.ParameterTypes[0];
            Assert.Equal("CSharpExtractor", parameterModel2.Type.Name);
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Null(parameterModel2.DefaultValue);
            Assert.Equal("TopLevel.Foo", methodG.ContainingTypeName);
            Assert.Equal("public", methodG.AccessModifier);
            Assert.Equal("abstract", methodG.Modifier);
            Assert.Empty(methodG.CalledMethods);

            var methodH = classModel.Methods[2];
            Assert.Equal("h", methodH.Name);
            Assert.Equal("string", methodH.ReturnValue.Type.Name);
            Assert.Equal("", ((ReturnValueModel)methodH.ReturnValue).Modifier);
            Assert.Equal(2, methodH.ParameterTypes.Count);
            var parameterModel3 = (ParameterModel)methodH.ParameterTypes[0];
            Assert.Equal("float", parameterModel3.Type.Name);
            Assert.Equal("", parameterModel3.Modifier);
            Assert.Null(parameterModel3.DefaultValue);
            var parameterModel4 = (ParameterModel)methodH.ParameterTypes[1];
            Assert.Equal("CSharpExtractor", parameterModel4.Type.Name);
            Assert.Equal("", parameterModel4.Modifier);
            Assert.Null(parameterModel4.DefaultValue);
            Assert.Equal("TopLevel.Foo", methodH.ContainingTypeName);
            Assert.Equal("public", methodH.AccessModifier);
            Assert.Equal("abstract", methodH.Modifier);
            Assert.Empty(methodH.CalledMethods);
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
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);
            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(3, classModel.Methods.Count);

            Assert.Equal("f", classModel.Methods[0].Name);
            Assert.Equal("void", classModel.Methods[0].ReturnValue.Type.Name);
            Assert.Equal("", ((ReturnValueModel)classModel.Methods[0].ReturnValue).Modifier);
            Assert.Equal(1, classModel.Methods[0].ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)classModel.Methods[0].ParameterTypes[0];
            Assert.Equal("int", parameterModel1.Type.Name);
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Null(parameterModel1.DefaultValue);
            Assert.Equal("TopLevel.Foo", classModel.Methods[0].ContainingTypeName);
            Assert.Equal("public", classModel.Methods[0].AccessModifier);
            Assert.Equal("static", classModel.Methods[0].Modifier);
            Assert.Empty(classModel.Methods[0].CalledMethods);

            Assert.Equal("g", classModel.Methods[1].Name);
            Assert.Equal("int", classModel.Methods[1].ReturnValue.Type.Name);
            Assert.Equal("", ((ReturnValueModel)classModel.Methods[1].ReturnValue).Modifier);
            Assert.Equal(1, classModel.Methods[1].ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)classModel.Methods[1].ParameterTypes[0];
            Assert.Equal("CSharpExtractor", parameterModel2.Type.Name);
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Null(parameterModel2.DefaultValue);
            Assert.Equal("TopLevel.Foo", classModel.Methods[1].ContainingTypeName);
            Assert.Equal("private", classModel.Methods[1].AccessModifier);
            Assert.Equal("", classModel.Methods[1].Modifier);
            Assert.Equal(1, classModel.Methods[1].CalledMethods.Count);
            Assert.Equal("f", classModel.Methods[1].CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", classModel.Methods[1].CalledMethods[0].ContainingTypeName);

            Assert.Equal("h", classModel.Methods[2].Name);
            Assert.Equal("string", classModel.Methods[2].ReturnValue.Type.Name);
            Assert.Equal("", ((ReturnValueModel)classModel.Methods[2].ReturnValue).Modifier);
            Assert.Equal(2, classModel.Methods[2].ParameterTypes.Count);
            var parameterModel3 = (ParameterModel)classModel.Methods[2].ParameterTypes[0];
            Assert.Equal("float", parameterModel3.Type.Name);
            Assert.Equal("", parameterModel3.Modifier);
            Assert.Null(parameterModel3.DefaultValue);
            var parameterModel4 = (ParameterModel)classModel.Methods[2].ParameterTypes[1];
            Assert.Equal("CSharpExtractor", parameterModel4.Type.Name);
            Assert.Equal("", parameterModel4.Modifier);
            Assert.Null(parameterModel4.DefaultValue);
            Assert.Equal("TopLevel.Foo", classModel.Methods[2].ContainingTypeName);
            Assert.Equal("protected", classModel.Methods[2].AccessModifier);
            Assert.Equal("", classModel.Methods[2].Modifier);
            Assert.Equal(2, classModel.Methods[2].CalledMethods.Count);
            Assert.Equal("g", classModel.Methods[2].CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", classModel.Methods[2].CalledMethods[0].ContainingTypeName);
            Assert.Equal("f", classModel.Methods[2].CalledMethods[1].Name);
            Assert.Equal("TopLevel.Foo", classModel.Methods[2].CalledMethods[1].ContainingTypeName);
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/CSharpClassFactExtractor/ReadonlyStructs.txt")]
        public void Extract_ShouldHaveReadonlyStructs_WhenGivenPathToAFileWithReadonlyStructs(string fileContent)
        {
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(2, classTypes.Count);
            Assert.Equal("Points.ReadonlyPoint3D", classTypes[0].Name);
            Assert.Equal("Points.ReadonlyPoint2D", classTypes[1].Name);

            foreach (var classType in classTypes)
            {
                var classModel = (ClassModel)classType;
                Assert.Equal("struct", classModel.ClassType);
                Assert.Equal("public", classModel.AccessModifier);
                Assert.Equal("readonly", classModel.Modifier);
            }
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/CSharpClassFactExtractor/MutableStructWithReadonlyMembers.txt")]
        public void Extract_ShouldHaveReadonlyStructMembers_WhenGivenPathToAFileWithMutableStructWithReadonlyMembers(
            string fileContent)
        {
            var classModels = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classModels.Count);
            var classModel = (ClassModel)classModels[0];
            Assert.Equal("Geometry.Point2D", classModel.Name);

            Assert.Equal("struct", classModel.ClassType);
            Assert.Equal("public", classModel.AccessModifier);
            Assert.Equal("", classModel.Modifier);

            Assert.Equal(1, classModel.Methods.Count);
            Assert.Equal("ToString", classModel.Methods[0].Name);
            Assert.Equal("string", classModel.Methods[0].ReturnValue.Type.Name);
            Assert.Equal("", ((ReturnValueModel)classModel.Methods[0].ReturnValue).Modifier);
            Assert.Equal("public", classModel.Methods[0].AccessModifier);
            Assert.Equal("readonly override", classModel.Methods[0].Modifier);

            Assert.Equal(3, classModel.Properties.Count);

            foreach (var propertyModel in classModel.Properties)
            {
                Assert.Equal("double", propertyModel.Type.Name);
                Assert.Equal("public", propertyModel.AccessModifier);
            }

            Assert.Equal("X", classModel.Properties[0].Name);
            Assert.Equal("", classModel.Properties[0].Modifier);
            Assert.Equal(2, classModel.Properties[0].Accessors.Count);
            Assert.Equal("readonly get", classModel.Properties[0].Accessors[0]);
            Assert.Equal("set", classModel.Properties[0].Accessors[1]);

            Assert.Equal("Y", classModel.Properties[1].Name);
            Assert.Equal("", classModel.Properties[1].Modifier);
            Assert.Equal(2, classModel.Properties[1].Accessors.Count);
            Assert.Equal("readonly get", classModel.Properties[1].Accessors[0]);
            Assert.Equal("set", classModel.Properties[1].Accessors[1]);

            Assert.Equal("Distance", classModel.Properties[2].Name);
            Assert.Equal("readonly", classModel.Properties[2].Modifier);
            Assert.Empty(classModel.Properties[2].Accessors);
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/CSharpClassFactExtractor/StructWithRefReadonlyStaticMembers.txt")]
        public void
            Extract_ShouldHaveReadonlyStaticStructMembers_WhenGivenPathToAFileWithStructWithStaticReadonlyMembers(
                string fileContent)
        {
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);
            var classModel = (ClassModel)classTypes[0];

            Assert.Equal("Geometry.Point", classModel.Name);
            Assert.Equal("struct", classModel.ClassType);
            Assert.Equal("public", classModel.AccessModifier);
            Assert.Equal("", classModel.Modifier);

            Assert.Equal(2, classModel.Properties.Count);

            foreach (var propertyModel in classModel.Properties)
            {
                Assert.Equal("Geometry.Point", propertyModel.Type.Name);
                Assert.Equal("public", propertyModel.AccessModifier);
                Assert.Empty(propertyModel.Accessors);
            }

            Assert.Equal("Origin", classModel.Properties[0].Name);
            Assert.Equal("ref", classModel.Properties[0].Modifier);

            Assert.Equal("Origin2", classModel.Properties[1].Name);
            Assert.Equal("static ref readonly", classModel.Properties[1].Modifier);
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/CSharpClassFactExtractor/RefStructs.txt")]
        public void Extract_ShouldHaveRefStructs_WhenGivenPathToAFileWithRefStructs(string fileContent)
        {
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(2, classTypes.Count);

            var classModel1 = (ClassModel)classTypes[0];
            Assert.Equal("Namespace1.CustomRef", classModel1.Name);
            Assert.Equal("struct", classModel1.ClassType);
            Assert.Equal("public", classModel1.AccessModifier);
            Assert.Equal("ref", classModel1.Modifier);

            Assert.Equal(2, classModel1.Fields.Count);

            foreach (var fieldModel in classModel1.Fields)
            {
                Assert.Equal("public", fieldModel.AccessModifier);
                Assert.Equal("System.Span<int>", fieldModel.Type.Name);
            }

            var classModel2 = (ClassModel)classTypes[1];
            Assert.Equal("Namespace1.ConversionRequest", classModel2.Name);
            Assert.Equal("struct", classModel2.ClassType);
            Assert.Equal("public", classModel2.AccessModifier);
            Assert.Equal("readonly ref", classModel2.Modifier);

            Assert.Equal(2, classModel2.Properties.Count);

            foreach (var propertyModel in classModel2.Properties)
            {
                Assert.Equal("public", propertyModel.AccessModifier);
                Assert.Equal("", propertyModel.Modifier);
                Assert.Equal(1, propertyModel.Accessors.Count);
                Assert.Equal("get", propertyModel.Accessors[0]);
            }

            Assert.Equal("Rate", classModel2.Properties[0].Name);
            Assert.Equal("double", classModel2.Properties[0].Type.Name);

            Assert.Equal("Values", classModel2.Properties[1].Name);
            Assert.Equal("System.ReadOnlySpan<double>", classModel2.Properties[1].Type.Name);
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/CSharpClassFactExtractor/RefReturnMethods.txt")]
        public void Extract_ShouldHaveRefReturnMethod_WhenGivenPathToAFileWithRefStructs(string fileContent)
        {
            var classTypes = _sut.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal("Namespace1.Class1", classModel.Name);

            Assert.Equal(2, classModel.Methods.Count);

            foreach (var methodModel in classModel.Methods)
            {
                Assert.Equal("Find", methodModel.Name);
                Assert.Equal("static", methodModel.Modifier);
                Assert.Equal("public", methodModel.AccessModifier);
                Assert.Equal(2, methodModel.ParameterTypes.Count);
                Assert.Equal("int[*,*]", methodModel.ParameterTypes[0].Type.Name);
                Assert.Equal("int", methodModel.ReturnValue.Type.Name);
            }

            Assert.Equal("ref", ((ReturnValueModel)classModel.Methods[0].ReturnValue).Modifier);
            Assert.Equal("System.Func<int, bool>", classModel.Methods[0].ParameterTypes[1].Type.Name);

            Assert.Equal("ref readonly", ((ReturnValueModel)classModel.Methods[1].ReturnValue).Modifier);
            Assert.Equal("bool", classModel.Methods[1].ParameterTypes[1].Type.Name);
        }
    }
}
