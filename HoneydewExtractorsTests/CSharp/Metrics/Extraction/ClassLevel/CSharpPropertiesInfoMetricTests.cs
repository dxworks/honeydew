using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Extraction.Class;
using HoneydewExtractors.Core.Metrics.Extraction.Common;
using HoneydewExtractors.Core.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.Core.Metrics.Extraction.MethodCall;
using HoneydewExtractors.Core.Metrics.Extraction.Property;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.CSharp;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpPropertiesInfoMetricTests
    {
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpPropertiesInfoMetricTests()
        {
            var compositeVisitor = new CompositeVisitor();

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new PropertySetterClassVisitor(new List<ICSharpPropertyVisitor>
                {
                    new PropertyInfoVisitor(),
                    new CalledMethodSetterVisitor(new List<ICSharpMethodSignatureVisitor>
                    {
                        new MethodCallInfoVisitor()
                    })
                })
            }));
            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker()), compositeVisitor);
        }

        [Fact]
        public void Extract_ShouldHaveProperties_WhenGivenAnInterface()
        {
            const string fileContent = @"using System;
     
                                      namespace TopLevel
                                      {
                                          public interface Foo { public int Value {get;set;} }                                       
                                      }";


            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var propertyModel = ((ClassModel)classTypes[0]).Properties[0];
            Assert.Equal("Value", propertyModel.Name);
            Assert.Equal("", propertyModel.Modifier);
            Assert.Equal("public", propertyModel.AccessModifier);
            Assert.Equal("int", propertyModel.Type);
            Assert.Equal("TopLevel.Foo", propertyModel.ContainingTypeName);
            Assert.False(propertyModel.IsEvent);
            Assert.Empty(propertyModel.CalledMethods);
        }

        [Fact]
        public void
            Extract_ShouldHavePrivatePropertiesWithModifiers_WhenGivenClassWithPropertiesAndModifiersWithDefaultAccess()
        {
            const string fileContent = @"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {
                                          public class Foo { static int A {get;set;} = 12; private float X {get;set;}
                                                           void f() {}
                                                           public string g(int a) {return ""Value"";}                              
                                                           }                                        
                                      }";


            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var propertyTypes = ((ClassModel)classTypes[0]).Properties;

            Assert.Equal(2, propertyTypes.Count);

            Assert.Equal("A", propertyTypes[0].Name);
            Assert.Equal("int", propertyTypes[0].Type);
            Assert.Equal("static", propertyTypes[0].Modifier);
            Assert.Equal("private", propertyTypes[0].AccessModifier);
            Assert.False(propertyTypes[0].IsEvent);
            Assert.Equal("TopLevel.Foo", propertyTypes[0].ContainingTypeName);
            Assert.Empty(propertyTypes[0].CalledMethods);

            Assert.Equal("X", propertyTypes[1].Name);
            Assert.Equal("float", propertyTypes[1].Type);
            Assert.Equal("", propertyTypes[1].Modifier);
            Assert.Equal("private", propertyTypes[1].AccessModifier);
            Assert.False(propertyTypes[1].IsEvent);
            Assert.Equal("TopLevel.Foo", propertyTypes[1].ContainingTypeName);
            Assert.Empty(propertyTypes[1].CalledMethods);
        }

        [Theory]
        [InlineData("public")]
        [InlineData("private")]
        [InlineData("protected")]
        [InlineData("internal")]
        [InlineData("protected internal")]
        [InlineData("private protected")]
        public void Extract_ShouldHavePropertiesWithNoOtherModifiers_WhenGivenClassWithOnlyPropertiesAndTheirModifier(
            string modifier)
        {
            var fileContent = $@"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {{
                                          public class Foo {{ {modifier} int AnimalNest{{get;set;}} {modifier} float X {{get;set;}} {modifier} CSharpMetricExtractor extractor{{get;init;}}}}                                        
                                      }}";


            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var propertyModels = ((ClassModel)classTypes[0]).Properties;

            Assert.Equal(3, propertyModels.Count);

            Assert.Equal("AnimalNest", propertyModels[0].Name);
            Assert.Equal("int", propertyModels[0].Type);
            Assert.Equal("", propertyModels[0].Modifier);
            Assert.Equal(modifier, propertyModels[0].AccessModifier);
            Assert.Equal("TopLevel.Foo", propertyModels[0].ContainingTypeName);
            Assert.False(propertyModels[0].IsEvent);
            Assert.Empty(propertyModels[0].CalledMethods);

            Assert.Equal("X", propertyModels[1].Name);
            Assert.Equal("float", propertyModels[1].Type);
            Assert.Equal("", propertyModels[1].Modifier);
            Assert.Equal(modifier, propertyModels[1].AccessModifier);
            Assert.Equal("TopLevel.Foo", propertyModels[1].ContainingTypeName);
            Assert.False(propertyModels[1].IsEvent);
            Assert.Empty(propertyModels[1].CalledMethods);

            Assert.Equal("extractor", propertyModels[2].Name);
            Assert.Equal("CSharpMetricExtractor", propertyModels[2].Type);
            Assert.Equal("", propertyModels[2].Modifier);
            Assert.Equal(modifier, propertyModels[2].AccessModifier);
            Assert.Equal("TopLevel.Foo", propertyModels[2].ContainingTypeName);
            Assert.False(propertyModels[2].IsEvent);
            Assert.Empty(propertyModels[2].CalledMethods);
        }

        [Theory]
        [InlineData("public")]
        [InlineData("private")]
        [InlineData("protected")]
        [InlineData("internal")]
        [InlineData("protected internal")]
        [InlineData("private protected")]
        public void
            Extract_ShouldHavePropertiesWithNoOtherModifiers_WhenGivenClassWithOnlyEventPropertiesAndTheirModifier(
                string visibility)
        {
            var fileContent = $@"using System;
                                      using HoneydewCore.Extractors;
                                      namespace SomeNamespace
                                      {{
                                          public class Foo {{ {visibility} event CSharpMetricExtractor extractor {{add{{}} remove{{}} }} {visibility} event int _some_event{{add{{}} remove{{}} }} {visibility} event Action MyAction1{{add{{}} remove{{}} }}}}                                        
                                      }}";


            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var propertyModels = ((ClassModel)classTypes[0]).Properties;

            Assert.Equal(3, propertyModels.Count);

            Assert.Equal("extractor", propertyModels[0].Name);
            Assert.Equal("CSharpMetricExtractor", propertyModels[0].Type);
            Assert.Equal("", propertyModels[0].Modifier);
            Assert.Equal(visibility, propertyModels[0].AccessModifier);
            Assert.True(propertyModels[0].IsEvent);
            Assert.Empty(propertyModels[0].CalledMethods);

            Assert.Equal("_some_event", propertyModels[1].Name);
            Assert.Equal("int", propertyModels[1].Type);
            Assert.Equal("", propertyModels[1].Modifier);
            Assert.Equal(visibility, propertyModels[1].AccessModifier);
            Assert.True(propertyModels[1].IsEvent);
            Assert.Empty(propertyModels[1].CalledMethods);

            Assert.Equal("MyAction1", propertyModels[2].Name);
            Assert.Equal("System.Action", propertyModels[2].Type);
            Assert.Equal("", propertyModels[2].Modifier);
            Assert.Equal(visibility, propertyModels[2].AccessModifier);
            Assert.True(propertyModels[2].IsEvent);
            Assert.Empty(propertyModels[1].CalledMethods);
        }

        [Fact]
        public void Extract_ShouldHavePropertiesWithAccessors_WhenGivenClassWithPropertiesWithDifferentAccessors()
        {
            const string fileContent = @"using System;
                                      namespace TopLevel
                                      {
                                        public class Foo
                                        {
                                            protected int Value
                                            {
                                                get => throw new System.NotImplementedException();
                                                private set => throw new System.NotImplementedException();
                                            }

                                            public string Name { get; protected init; }

                                            public string FullName { get; }

                                            public bool IsHere
                                            {
                                                set { throw new System.NotImplementedException(); }
                                            }

                                            public event Func<int> IntEvent { add{} remove{} }
                                        }                                    
                                      }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var propertyModels = ((ClassModel)classTypes[0]).Properties;

            Assert.Equal(5, propertyModels.Count);

            var propertyModel0 = propertyModels[0];
            Assert.Equal("Value", propertyModel0.Name);
            Assert.False(propertyModel0.IsEvent);
            Assert.Equal(2, propertyModel0.Accessors.Count);
            Assert.Equal("get", propertyModel0.Accessors[0]);
            Assert.Equal("private set", propertyModel0.Accessors[1]);

            var propertyModel1 = propertyModels[1];
            Assert.Equal("Name", propertyModel1.Name);
            Assert.False(propertyModel1.IsEvent);
            Assert.Equal(2, propertyModel1.Accessors.Count);
            Assert.Equal("get", propertyModel1.Accessors[0]);
            Assert.Equal("protected init", propertyModel1.Accessors[1]);

            var propertyModel2 = propertyModels[2];
            Assert.Equal("FullName", propertyModel2.Name);
            Assert.False(propertyModel2.IsEvent);
            Assert.Equal(1, propertyModel2.Accessors.Count);
            Assert.Equal("get", propertyModel2.Accessors[0]);

            var propertyModel3 = propertyModels[3];
            Assert.Equal("IsHere", propertyModel3.Name);
            Assert.False(propertyModel3.IsEvent);
            Assert.Equal(1, propertyModel3.Accessors.Count);
            Assert.Equal("set", propertyModel3.Accessors[0]);

            var propertyModel4 = propertyModels[4];
            Assert.Equal("IntEvent", propertyModel4.Name);
            Assert.True(propertyModel4.IsEvent);
            Assert.Equal(2, propertyModel4.Accessors.Count);
            Assert.Equal("add", propertyModel4.Accessors[0]);
            Assert.Equal("remove", propertyModel4.Accessors[1]);
        }

        [Fact]
        public void
            Extract_ShouldHaveProperties_WhenGivenClassWithComputedEmptyProperties()
        {
            const string fileContent = @"using System;
                                      namespace TopLevel
                                      {
                                        class Bar {}
                                        public class Foo 
                                        { 
                                            protected Bar Value
                                            {
                                                get => throw new System.NotImplementedException();
                                                set => throw new System.NotImplementedException();
                                            }
                                        }                                        
                                      }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(2, classTypes.Count);

            var propertyModels = ((ClassModel)classTypes[1]).Properties;

            Assert.Equal(1, propertyModels.Count);

            Assert.Equal("Value", propertyModels[0].Name);
            Assert.Equal("TopLevel.Bar", propertyModels[0].Type);
            Assert.Equal("", propertyModels[0].Modifier);
            Assert.Equal("protected", propertyModels[0].AccessModifier);
            Assert.False(propertyModels[0].IsEvent);
            Assert.Equal("TopLevel.Foo", propertyModels[0].ContainingTypeName);
            Assert.Empty(propertyModels[0].CalledMethods);
        }

        [Fact]
        public void
            Extract_ShouldHaveProperties_WhenGivenClassWithPropertiesAndBackingFields()
        {
            const string fileContent = @"using System;
                                      namespace TopLevel
                                      {
                                        public class Foo 
                                        { 
                                            private int _value;
                                            public int Value
                                            {
                                                set => _value = value;
                                                get => _value;
                                            }
                                        }                                        
                                      }";


            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var propertyModels = ((ClassModel)classTypes[0]).Properties;

            Assert.Equal(1, propertyModels.Count);

            Assert.Equal("Value", propertyModels[0].Name);
            Assert.Equal("int", propertyModels[0].Type);
            Assert.Equal("", propertyModels[0].Modifier);
            Assert.Equal("public", propertyModels[0].AccessModifier);
            Assert.False(propertyModels[0].IsEvent);
            Assert.Equal("TopLevel.Foo", propertyModels[0].ContainingTypeName);
            Assert.Empty(propertyModels[0].CalledMethods);
        }

        [Fact]
        public void
            Extract_ShouldHaveProperties_WhenGivenClassWithComputedPropertyThatCallsInnerMethods()
        {
            const string fileContent = @"using System;
                                      namespace TopLevel
                                      {
                                        public class Foo 
                                        { 
                                            private int _value;
                                            public int Value
                                            {
                                                get => Triple(_value);
                                                set
                                                {
                                                    _value = value;
                                                    _value = Double(_value);
                                                }
                                            }

                                            protected int Triple(int a)
                                            {
                                                return a * 3;
                                            }

                                            private static int Double(int a)
                                            {
                                                return a * 2;
                                            }
                                        }                                        
                                      }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var propertyModels = ((ClassModel)classTypes[0]).Properties;

            Assert.Equal(1, propertyModels.Count);

            Assert.Equal("Value", propertyModels[0].Name);
            Assert.Equal("int", propertyModels[0].Type);
            Assert.Equal("", propertyModels[0].Modifier);
            Assert.Equal("public", propertyModels[0].AccessModifier);
            Assert.False(propertyModels[0].IsEvent);
            Assert.Equal("TopLevel.Foo", propertyModels[0].ContainingTypeName);
            Assert.Equal(2, propertyModels[0].CalledMethods.Count);

            Assert.Equal("Triple", propertyModels[0].CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", propertyModels[0].CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, propertyModels[0].CalledMethods[0].ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)propertyModels[0].CalledMethods[0].ParameterTypes[0];
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Equal("int", parameterModel1.Name);
            Assert.Null(parameterModel1.DefaultValue);

            Assert.Equal("Double", propertyModels[0].CalledMethods[1].Name);
            Assert.Equal("TopLevel.Foo", propertyModels[0].CalledMethods[1].ContainingTypeName);
            Assert.Equal(1, propertyModels[0].CalledMethods[1].ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)propertyModels[0].CalledMethods[1].ParameterTypes[0];
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Equal("int", parameterModel2.Name);
            Assert.Null(parameterModel2.DefaultValue);
        }

        [Fact]
        public void
            Extract_ShouldHaveProperties_WhenGivenClassWithComputedPropertyThatCallsMethodsFromOtherClassFromTheSameNamespace()
        {
            const string fileContent =
                @"using System;                                                                         
                                    namespace TopLevel
                                    {
                                        class Bar
                                        {
                                            public int Triple(int a)
                                            {
                                                return a * 3;
                                            }

                                            public static int Double(int a)
                                            {
                                                return a * 2;
                                            }
                                        }

                                        public class Foo
                                        {
                                            private Bar _bar=new();
                                            private int _value;

                                            public int Value
                                            {
                                                get
                                                {
                                                    var temp = _value;
                                                    return _bar.Triple(temp);
                                                }
                                                set => _value = Bar.Double(value);
                                            }
                                        }
                                    }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(2, classTypes.Count);

            var propertyModels = ((ClassModel)classTypes[1]).Properties;

            Assert.Equal(1, propertyModels.Count);

            Assert.Equal("Value", propertyModels[0].Name);
            Assert.Equal("int", propertyModels[0].Type);
            Assert.Equal("", propertyModels[0].Modifier);
            Assert.Equal("public", propertyModels[0].AccessModifier);
            Assert.False(propertyModels[0].IsEvent);
            Assert.Equal("TopLevel.Foo", propertyModels[0].ContainingTypeName);
            Assert.Equal(2, propertyModels[0].CalledMethods.Count);

            Assert.Equal("Triple", propertyModels[0].CalledMethods[0].Name);
            Assert.Equal("TopLevel.Bar", propertyModels[0].CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, propertyModels[0].CalledMethods[0].ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)propertyModels[0].CalledMethods[0].ParameterTypes[0];
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Equal("int", parameterModel1.Name);
            Assert.Null(parameterModel1.DefaultValue);

            Assert.Equal("Double", propertyModels[0].CalledMethods[1].Name);
            Assert.Equal("TopLevel.Bar", propertyModels[0].CalledMethods[1].ContainingTypeName);
            Assert.Equal(1, propertyModels[0].CalledMethods[1].ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)propertyModels[0].CalledMethods[1].ParameterTypes[0];
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Equal("int", parameterModel2.Name);
            Assert.Null(parameterModel2.DefaultValue);
        }

        [Fact]
        public void
            Extract_ShouldHaveProperties_WhenGivenClassWithComputedPropertyThatCallsStaticMethodsFromUnknownClass()
        {
            const string fileContent =
                @"using System;                                                                         
                                    namespace TopLevel
                                    {
                                        public class Foo
                                        {
                                            private ExternClass _class=new();
                                            private int _value;

                                            public int Value
                                            {
                                                get
                                                {
                                                    var temp = _value;
                                                    return _class.Triple(temp);
                                                }
                                                set => _value = ExternClass.Double(value);
                                            }
                                        }
                                    }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var propertyModels = ((ClassModel)classTypes[0]).Properties;

            Assert.Equal(1, propertyModels.Count);

            Assert.Equal("Value", propertyModels[0].Name);
            Assert.Equal("int", propertyModels[0].Type);
            Assert.Equal("", propertyModels[0].Modifier);
            Assert.Equal("public", propertyModels[0].AccessModifier);
            Assert.False(propertyModels[0].IsEvent);
            Assert.Equal("TopLevel.Foo", propertyModels[0].ContainingTypeName);
            Assert.Equal(2, propertyModels[0].CalledMethods.Count);

            Assert.Equal("Triple", propertyModels[0].CalledMethods[0].Name);
            Assert.Equal("ExternClass", propertyModels[0].CalledMethods[0].ContainingTypeName);

            Assert.Equal(1, propertyModels[0].CalledMethods[0].ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)propertyModels[0].CalledMethods[0].ParameterTypes[0];
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Equal("int", parameterModel1.Name);
            Assert.Null(parameterModel1.DefaultValue);

            Assert.Equal("Double", propertyModels[0].CalledMethods[1].Name);
            Assert.Equal("ExternClass", propertyModels[0].CalledMethods[1].ContainingTypeName);

            Assert.Equal(1, propertyModels[0].CalledMethods[1].ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)propertyModels[0].CalledMethods[1].ParameterTypes[0];
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Equal("int", parameterModel2.Name);
            Assert.Null(parameterModel2.DefaultValue);
        }

        [Fact]
        public void
            Extract_ShouldHaveEventProperties_WhenGivenClassWithComputedPropertyThatCallsInnerMethods()
        {
            const string fileContent = @"using System;
                                      namespace TopLevel
                                      {
                                        public class Foo 
                                        { 
                                            private double _value;
                                            public event Func<double> Value
                                            {
                                                add => Triple(_value);
                                                remove {
                                                    
                                                    _value = Double(_value);
                                                }
                                            }

                                            protected double Triple(double a)
                                            {
                                                return a * 3;
                                            }

                                            private static double Double(double a)
                                            {
                                                return a * 2;
                                            }
                                        }                                        
                                      }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var propertyModels = ((ClassModel)classTypes[0]).Properties;

            Assert.Equal(1, propertyModels.Count);

            Assert.Equal("Value", propertyModels[0].Name);
            Assert.Equal("System.Func<double>", propertyModels[0].Type);
            Assert.Equal("", propertyModels[0].Modifier);
            Assert.Equal("public", propertyModels[0].AccessModifier);
            Assert.True(propertyModels[0].IsEvent);
            Assert.Equal("TopLevel.Foo", propertyModels[0].ContainingTypeName);
            Assert.Equal(2, propertyModels[0].CalledMethods.Count);

            Assert.Equal("Triple", propertyModels[0].CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", propertyModels[0].CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, propertyModels[0].CalledMethods[0].ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)propertyModels[0].CalledMethods[0].ParameterTypes[0];
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Equal("double", parameterModel1.Name);
            Assert.Null(parameterModel1.DefaultValue);

            Assert.Equal("Double", propertyModels[0].CalledMethods[1].Name);
            Assert.Equal("TopLevel.Foo", propertyModels[0].CalledMethods[1].ContainingTypeName);
            Assert.Equal(1, propertyModels[0].CalledMethods[1].ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)propertyModels[0].CalledMethods[1].ParameterTypes[0];
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Equal("double", parameterModel2.Name);
            Assert.Null(parameterModel2.DefaultValue);
        }

        [Fact]
        public void
            Extract_ShouldHaveEventProperties_WhenGivenClassWithComputedPropertyThatCallsMethodsFromOtherClassFromTheSameNamespace()
        {
            const string fileContent =
                @"using System;                                                                         
                                    namespace TopLevel
                                    {
                                         public class Bar
                                        {
                                            public string Convert(int a)
                                            {
                                                return a.ToString();
                                            }

                                            public static string Cut(Func<string> s)
                                            {
                                                return s.ToString();
                                            }
                                        }

                                        public class Foo
                                        {
                                            private Bar _bar=new();
                                            private string _value;

                                            public event Func<string> Value
                                            {
                                                add
                                                {
                                                    var temp = _value;
                                                    _bar.Convert(temp.Length);
                                                }
                                                remove => _value = Bar.Cut(value);
                                            }
                                        }
                                    }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(2, classTypes.Count);

            var propertyModels = ((ClassModel)classTypes[1]).Properties;

            Assert.Equal(1, propertyModels.Count);

            Assert.Equal("Value", propertyModels[0].Name);
            Assert.Equal("System.Func<string>", propertyModels[0].Type);
            Assert.Equal("", propertyModels[0].Modifier);
            Assert.Equal("public", propertyModels[0].AccessModifier);
            Assert.True(propertyModels[0].IsEvent);
            Assert.Equal("TopLevel.Foo", propertyModels[0].ContainingTypeName);
            Assert.Equal(2, propertyModels[0].CalledMethods.Count);

            Assert.Equal("Convert", propertyModels[0].CalledMethods[0].Name);
            Assert.Equal("TopLevel.Bar", propertyModels[0].CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, propertyModels[0].CalledMethods[0].ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)propertyModels[0].CalledMethods[0].ParameterTypes[0];
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Equal("int", parameterModel1.Name);
            Assert.Null(parameterModel1.DefaultValue);

            Assert.Equal("Cut", propertyModels[0].CalledMethods[1].Name);
            Assert.Equal("TopLevel.Bar", propertyModels[0].CalledMethods[1].ContainingTypeName);
            Assert.Equal(1, propertyModels[0].CalledMethods[1].ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)propertyModels[0].CalledMethods[1].ParameterTypes[0];
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Equal("System.Func<string>", parameterModel2.Name);
            Assert.Null(parameterModel2.DefaultValue);
        }

        [Fact]
        public void
            Extract_ShouldHaveEventProperties_WhenGivenClassWithComputedPropertyThatCallsStaticMethodsFromUnknownClass()
        {
            const string fileContent =
                @"using System;                                                                         
                                    namespace TopLevel
                                    {
                                        public class Foo
                                        {
                                            private ExternClass _class=new();
                                            private int _value;

                                            public event Func<int> Value
                                            {
                                                add
                                                {
                                                    _class.Triple(_value);
                                                }
                                                remove => _value = ExternClass.Double(value);
                                            }
                                        }
                                    }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var propertyModels = ((ClassModel)classTypes[0]).Properties;

            Assert.Equal(1, propertyModels.Count);

            Assert.Equal("Value", propertyModels[0].Name);
            Assert.Equal("System.Func<int>", propertyModels[0].Type);
            Assert.Equal("", propertyModels[0].Modifier);
            Assert.Equal("public", propertyModels[0].AccessModifier);
            Assert.True(propertyModels[0].IsEvent);
            Assert.Equal("TopLevel.Foo", propertyModels[0].ContainingTypeName);
            Assert.Equal(2, propertyModels[0].CalledMethods.Count);

            Assert.Equal("Triple", propertyModels[0].CalledMethods[0].Name);
            Assert.Equal("ExternClass", propertyModels[0].CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, propertyModels[0].CalledMethods[0].ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)propertyModels[0].CalledMethods[0].ParameterTypes[0];
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Equal("int", parameterModel1.Name);
            Assert.Null(parameterModel1.DefaultValue);

            Assert.Equal("Double", propertyModels[0].CalledMethods[1].Name);
            Assert.Equal("ExternClass", propertyModels[0].CalledMethods[1].ContainingTypeName);
            Assert.Equal(1, propertyModels[0].CalledMethods[1].ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)propertyModels[0].CalledMethods[1].ParameterTypes[0];
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Equal("System.Func<int>", parameterModel2.Name);
            Assert.Null(parameterModel2.DefaultValue);
        }

        [Fact]
        public void
            Extract_ShouldHaveEventProperties_WhenGivenClassWithComputedPropertyThatCallsMethodsFromOtherClassFromTheSameNamespaceAsProperty()
        {
            const string fileContent =
                @"using System;                                                                         
                                    namespace TopLevel
                                    {
                                        public class Bar
                                        {
                                            public string Convert(int a)
                                            {
                                                return a.ToString();
                                            }

                                            public static string Cut(string s)
                                            {
                                                return s.Trim();
                                            }
                                        }

                                        public class Foo
                                        {
                                            private Bar _bar {get;set;}
                                            private string _value;

                                            public event Func<string> Value
                                            {
                                                add
                                                {
                                                    var temp = _value;
                                                    _bar.Convert(temp.Length);
                                                }
                                                remove => _value = Bar.Cut(value);
                                            }
                                        }
                                    }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(2, classTypes.Count);

            var propertyModels = ((ClassModel)classTypes[1]).Properties;

            Assert.Equal(2, propertyModels.Count);

            var propertyModel = propertyModels[1];
            Assert.Equal("Value", propertyModel.Name);
            Assert.Equal("System.Func<string>", propertyModel.Type);
            Assert.Equal("", propertyModel.Modifier);
            Assert.Equal("public", propertyModel.AccessModifier);
            Assert.True(propertyModel.IsEvent);
            Assert.Equal("TopLevel.Foo", propertyModel.ContainingTypeName);
            Assert.Equal(2, propertyModel.CalledMethods.Count);

            Assert.Equal("Convert", propertyModel.CalledMethods[0].Name);
            Assert.Equal("TopLevel.Bar", propertyModel.CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, propertyModel.CalledMethods[0].ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)propertyModel.CalledMethods[0].ParameterTypes[0];
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Equal("int", parameterModel1.Name);
            Assert.Null(parameterModel1.DefaultValue);

            Assert.Equal("Cut", propertyModel.CalledMethods[1].Name);
            Assert.Equal("TopLevel.Bar", propertyModel.CalledMethods[1].ContainingTypeName);

            Assert.Equal(1, propertyModel.CalledMethods[1].ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)propertyModel.CalledMethods[1].ParameterTypes[0];
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Equal("System.Func<string>", parameterModel2.Name);
            Assert.Null(parameterModel2.DefaultValue);
        }
    }
}
