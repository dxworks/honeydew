using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel;
using HoneydewModels.CSharp;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpMethodInfoMetricTests
    {
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpMethodInfoMetricTests()
        {
            _factExtractor = new CSharpFactExtractor();
            _factExtractor.AddMetric<CSharpMethodInfoMetric>();
        }

        [Fact]
        public void GetMetricType_ShouldReturnClassLevel()
        {
            Assert.Equal(ExtractionMetricType.ClassLevel, new CSharpMethodInfoMetric().GetMetricType());
        }

        [Fact]
        public void PrettyPrint_ShouldReturnFieldsInfo()
        {
            Assert.Equal("Methods Info", new CSharpMethodInfoMetric().PrettyPrint());
        }

        [Fact]
        public void Extract_ShouldHaveNoMethods_WhenGivenClassAndRecordsWithFieldsOnly()
        {
            const string fileContent = @"using System;
     
                                      namespace TopLevel
                                      {
                                          public class Foo { public int f; string g; }

                                          public record Bar { Boolean a; string g; int x; float b; }                                        
                                      }";


            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(2, classModels.Count);

            foreach (var classModel in classModels)
            {
                var optional = classModel.GetMetricValue<CSharpMethodInfoMetric>();
                Assert.True(optional.HasValue);

                var infoDataMetric = (CSharpMethodInfoDataMetric)optional.Value;

                Assert.Empty(infoDataMetric.ConstructorInfos);
                Assert.Empty(infoDataMetric.MethodInfos);
            }
        }

        [Fact]
        public void Extract_ShouldHaveMethods_WhenGivenAClassHierarchy()
        {
            const string fileContent = @"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {
                                             public abstract class Foo
                                             {
                                                 protected int G(float a)
                                                 {
                                                     return (int) a;
                                                 }

                                                 public virtual bool H()
                                                 {
                                                     return false;
                                                 }

                                                 public abstract int X();
                                             }

                                             public class Bar : Foo
                                             {
                                                 public override int X()
                                                 {
                                                     return 1;
                                                 }

                                                 public override bool H()
                                                 {
                                                     return true;
                                                 }
                                             }                                  
                                      }";
            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(2, classModels.Count);

            Assert.Equal(3, classModels[0].Methods.Count);

            Assert.Equal("G", classModels[0].Methods[0].Name);
            Assert.Equal("int", classModels[0].Methods[0].ReturnType.Name);
            Assert.Equal(1, classModels[0].Methods[0].ParameterTypes.Count);
            var parameterModel = (ParameterModel)classModels[0].Methods[0].ParameterTypes[0];
            Assert.Equal("float", parameterModel.Name);
            Assert.Equal("", parameterModel.Modifier);
            Assert.Null(parameterModel.DefaultValue);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[0].ContainingTypeName);
            Assert.Equal("protected", classModels[0].Methods[0].AccessModifier);
            Assert.Equal("", classModels[0].Methods[0].Modifier);
            Assert.Empty(classModels[0].Methods[0].CalledMethods);

            Assert.Equal("H", classModels[0].Methods[1].Name);
            Assert.Equal("bool", classModels[0].Methods[1].ReturnType.Name);
            Assert.Empty(classModels[0].Methods[1].ParameterTypes);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[1].ContainingTypeName);
            Assert.Equal("public", classModels[0].Methods[1].AccessModifier);
            Assert.Equal("virtual", classModels[0].Methods[1].Modifier);
            Assert.Empty(classModels[0].Methods[1].CalledMethods);

            Assert.Equal("X", classModels[0].Methods[2].Name);
            Assert.Equal("int", classModels[0].Methods[2].ReturnType.Name);
            Assert.Empty(classModels[0].Methods[2].ParameterTypes);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[2].ContainingTypeName);
            Assert.Equal("public", classModels[0].Methods[2].AccessModifier);
            Assert.Equal("abstract", classModels[0].Methods[2].Modifier);
            Assert.Empty(classModels[0].Methods[2].CalledMethods);

            Assert.Equal(2, classModels[1].Methods.Count);

            Assert.Equal("X", classModels[1].Methods[0].Name);
            Assert.Equal("int", classModels[1].Methods[0].ReturnType.Name);
            Assert.Empty(classModels[1].Methods[0].ParameterTypes);
            Assert.Equal("TopLevel.Bar", classModels[1].Methods[0].ContainingTypeName);
            Assert.Equal("public", classModels[1].Methods[0].AccessModifier);
            Assert.Equal("override", classModels[1].Methods[0].Modifier);
            Assert.Empty(classModels[1].Methods[0].CalledMethods);

            Assert.Equal("H", classModels[1].Methods[1].Name);
            Assert.Equal("bool", classModels[1].Methods[1].ReturnType.Name);
            Assert.Empty(classModels[0].Methods[1].ParameterTypes);
            Assert.Equal("TopLevel.Bar", classModels[1].Methods[1].ContainingTypeName);
            Assert.Equal("public", classModels[1].Methods[1].AccessModifier);
            Assert.Equal("override", classModels[1].Methods[1].Modifier);
            Assert.Empty(classModels[1].Methods[1].CalledMethods);
        }

        [Fact]
        public void Extract_ShouldHaveMethodsWithMethodCalls_WhenGivenMethodsThatCallOtherMethods()
        {
            const string fileContent = @"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {
                                             public abstract class Foo
                                             {
                                                 protected int G(float a)
                                                 {
                                                     return (int) a;
                                                 }

                                                 public virtual bool H()
                                                 {
                                                     return false;
                                                 }
                                             }

                                             public class Bar : Foo
                                             {
                                                 int M() {return 2;}
                                             
                                                 public override bool H()
                                                 {
                                                     if (G(0.5f) == 0) {
                                                         return base.H();
                                                     }
                                                     int x = M();
                                                     
                                                     return true;
                                                 }
                                             }                                  
                                      }";
            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(2, classModels.Count);

            Assert.Equal(2, classModels[0].Methods.Count);

            Assert.Equal("G", classModels[0].Methods[0].Name);
            Assert.Equal("int", classModels[0].Methods[0].ReturnType.Name);
            Assert.Equal(1, classModels[0].Methods[0].ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)classModels[0].Methods[0].ParameterTypes[0];
            Assert.Equal("float", parameterModel1.Name);
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Null(parameterModel1.DefaultValue);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[0].ContainingTypeName);
            Assert.Equal("protected", classModels[0].Methods[0].AccessModifier);
            Assert.Equal("", classModels[0].Methods[0].Modifier);
            Assert.Empty(classModels[0].Methods[0].CalledMethods);

            Assert.Equal("H", classModels[0].Methods[1].Name);
            Assert.Equal("bool", classModels[0].Methods[1].ReturnType.Name);
            Assert.Empty(classModels[0].Methods[1].ParameterTypes);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[1].ContainingTypeName);
            Assert.Equal("public", classModels[0].Methods[1].AccessModifier);
            Assert.Equal("virtual", classModels[0].Methods[1].Modifier);
            Assert.Empty(classModels[0].Methods[1].CalledMethods);

            Assert.Equal(2, classModels[1].Methods.Count);

            Assert.Equal("M", classModels[1].Methods[0].Name);
            Assert.Equal("int", classModels[1].Methods[0].ReturnType.Name);
            Assert.Empty(classModels[1].Methods[0].ParameterTypes);
            Assert.Equal("TopLevel.Bar", classModels[1].Methods[0].ContainingTypeName);
            Assert.Equal("private", classModels[1].Methods[0].AccessModifier);
            Assert.Equal("", classModels[1].Methods[0].Modifier);
            Assert.Empty(classModels[1].Methods[0].CalledMethods);

            Assert.Equal("H", classModels[1].Methods[1].Name);
            Assert.Equal("bool", classModels[1].Methods[1].ReturnType.Name);
            Assert.Empty(classModels[0].Methods[1].ParameterTypes);
            Assert.Equal("TopLevel.Bar", classModels[1].Methods[1].ContainingTypeName);
            Assert.Equal("public", classModels[1].Methods[1].AccessModifier);
            Assert.Equal("override", classModels[1].Methods[1].Modifier);
            Assert.Equal(3, classModels[1].Methods[1].CalledMethods.Count);
            Assert.Equal("G", classModels[1].Methods[1].CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", classModels[1].Methods[1].CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, classModels[1].Methods[1].CalledMethods[0].ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)classModels[1].Methods[1].CalledMethods[0].ParameterTypes[0];
            Assert.Equal("float", parameterModel2.Name);
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Null(parameterModel2.DefaultValue);
            Assert.Equal("H", classModels[1].Methods[1].CalledMethods[1].Name);
            Assert.Equal("TopLevel.Foo", classModels[1].Methods[1].CalledMethods[1].ContainingTypeName);
            Assert.Empty(classModels[1].Methods[1].CalledMethods[1].ParameterTypes);
            Assert.Equal("M", classModels[1].Methods[1].CalledMethods[2].Name);
            Assert.Equal("TopLevel.Bar", classModels[1].Methods[1].CalledMethods[2].ContainingTypeName);
            Assert.Empty(classModels[1].Methods[1].CalledMethods[2].ParameterTypes);
        }

        [Fact]
        public void Extract_ShouldHaveMethodsWithMethodCalls_WhenGivenMethodsThatCallStaticMethods()
        {
            const string fileContent = @"using System;
                                      
                                      namespace TopLevel
                                      {
                                         public class Foo
                                         {
                                             public int A(int a)
                                             {
                                                 return a * 2;
                                             }

                                             public int B(int a, int b)
                                             {
                                                 return A(a) + this.A(b);
                                             }
                                         }

                                         public class Bar
                                         {
                                             public int F(int a, int b, string c)
                                             {
                                                 Foo f = new Foo();
                                                 var z = new Foo();
                                                 return f.A(a) + f.B(b, K(c)) + z.A(a);
                                             }

                                             private int K(string s)
                                             {
                                                 return s.Length;
                                             }
                                         }                       
                                      }";
            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(2, classModels.Count);

            Assert.Equal(2, classModels[0].Methods.Count);

            Assert.Equal("A", classModels[0].Methods[0].Name);
            Assert.Equal("int", classModels[0].Methods[0].ReturnType.Name);
            Assert.Equal(1, classModels[0].Methods[0].ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)classModels[0].Methods[0].ParameterTypes[0];
            Assert.Equal("int", parameterModel1.Name);
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Null(parameterModel1.DefaultValue);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[0].ContainingTypeName);
            Assert.Equal("public", classModels[0].Methods[0].AccessModifier);
            Assert.Equal("", classModels[0].Methods[0].Modifier);
            Assert.Empty(classModels[0].Methods[0].CalledMethods);

            Assert.Equal("B", classModels[0].Methods[1].Name);
            Assert.Equal("int", classModels[0].Methods[1].ReturnType.Name);
            Assert.Equal(2, classModels[0].Methods[1].ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)classModels[0].Methods[1].ParameterTypes[0];
            Assert.Equal("int", parameterModel2.Name);
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Null(parameterModel2.DefaultValue);
            var parameterModel3 = (ParameterModel)classModels[0].Methods[1].ParameterTypes[1];
            Assert.Equal("int", parameterModel3.Name);
            Assert.Equal("", parameterModel3.Modifier);
            Assert.Null(parameterModel3.DefaultValue);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[1].ContainingTypeName);
            Assert.Equal("public", classModels[0].Methods[1].AccessModifier);
            Assert.Equal("", classModels[0].Methods[1].Modifier);
            Assert.Equal(2, classModels[0].Methods[1].CalledMethods.Count);
            Assert.Equal("A", classModels[0].Methods[1].CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[1].CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, classModels[0].Methods[1].CalledMethods[0].ParameterTypes.Count);
            var parameterModel4 = (ParameterModel)classModels[0].Methods[1].CalledMethods[0].ParameterTypes[0];
            Assert.Equal("int", parameterModel4.Name);
            Assert.Equal("", parameterModel4.Modifier);
            Assert.Null(parameterModel4.DefaultValue);
            Assert.Equal("A", classModels[0].Methods[1].CalledMethods[1].Name);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[1].CalledMethods[1].ContainingTypeName);
            Assert.Equal(1, classModels[0].Methods[1].CalledMethods[1].ParameterTypes.Count);
            var parameterModel5 = (ParameterModel)classModels[0].Methods[1].CalledMethods[1].ParameterTypes[0];
            Assert.Equal("int", parameterModel5.Name);
            Assert.Equal("", parameterModel5.Modifier);
            Assert.Null(parameterModel5.DefaultValue);

            Assert.Equal(2, classModels[1].Methods.Count);

            var methodModelF = classModels[1].Methods[0];
            Assert.Equal("F", methodModelF.Name);
            Assert.Equal("int", methodModelF.ReturnType.Name);
            Assert.Equal(3, methodModelF.ParameterTypes.Count);
            var parameterModel6 = (ParameterModel)methodModelF.ParameterTypes[0];
            Assert.Equal("int", parameterModel6.Name);
            Assert.Equal("", parameterModel6.Modifier);
            Assert.Null(parameterModel6.DefaultValue);
            var parameterModel7 = (ParameterModel)methodModelF.ParameterTypes[1];
            Assert.Equal("int", parameterModel7.Name);
            Assert.Equal("", parameterModel7.Modifier);
            Assert.Null(parameterModel7.DefaultValue);
            var parameterModel8 = (ParameterModel)methodModelF.ParameterTypes[2];
            Assert.Equal("string", parameterModel8.Name);
            Assert.Equal("", parameterModel8.Modifier);
            Assert.Null(parameterModel8.DefaultValue);
            Assert.Equal("TopLevel.Bar", methodModelF.ContainingTypeName);
            Assert.Equal("public", methodModelF.AccessModifier);
            Assert.Equal("", methodModelF.Modifier);
            Assert.Equal(4, methodModelF.CalledMethods.Count);
            Assert.Equal("A", methodModelF.CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", methodModelF.CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, methodModelF.CalledMethods[0].ParameterTypes.Count);
            var parameterModel9 = (ParameterModel)methodModelF.CalledMethods[0].ParameterTypes[0];
            Assert.Equal("int", parameterModel9.Name);
            Assert.Equal("", parameterModel9.Modifier);
            Assert.Null(parameterModel9.DefaultValue);
            Assert.Equal("B", methodModelF.CalledMethods[1].Name);
            Assert.Equal("TopLevel.Foo", methodModelF.CalledMethods[1].ContainingTypeName);
            Assert.Equal(2, methodModelF.CalledMethods[1].ParameterTypes.Count);
            var parameterModel10 = (ParameterModel)methodModelF.CalledMethods[1].ParameterTypes[0];
            Assert.Equal("int", parameterModel10.Name);
            Assert.Equal("", parameterModel10.Modifier);
            Assert.Null(parameterModel10.DefaultValue);
            var parameterModel11 = (ParameterModel)methodModelF.CalledMethods[1].ParameterTypes[1];
            Assert.Equal("int", parameterModel11.Name);
            Assert.Equal("", parameterModel11.Modifier);
            Assert.Null(parameterModel11.DefaultValue);
            Assert.Equal("K", methodModelF.CalledMethods[2].Name);
            Assert.Equal("TopLevel.Bar", methodModelF.CalledMethods[2].ContainingTypeName);
            Assert.Equal(1, methodModelF.CalledMethods[2].ParameterTypes.Count);
            var parameterModel12 = (ParameterModel)methodModelF.CalledMethods[2].ParameterTypes[0];
            Assert.Equal("string", parameterModel12.Name);
            Assert.Equal("", parameterModel12.Modifier);
            Assert.Null(parameterModel12.DefaultValue);
            Assert.Equal("A", methodModelF.CalledMethods[3].Name);
            Assert.Equal("TopLevel.Foo", methodModelF.CalledMethods[3].ContainingTypeName);
            Assert.Equal(1, methodModelF.CalledMethods[3].ParameterTypes.Count);
            var parameterModel13 = (ParameterModel)methodModelF.CalledMethods[3].ParameterTypes[0];
            Assert.Equal("int", parameterModel13.Name);
            Assert.Equal("", parameterModel13.Modifier);
            Assert.Null(parameterModel13.DefaultValue);

            var methodModelK = classModels[1].Methods[1];
            Assert.Equal("K", methodModelK.Name);
            Assert.Equal("int", methodModelK.ReturnType.Name);
            Assert.Equal(1, methodModelK.ParameterTypes.Count);
            var parameterModel14 = (ParameterModel)methodModelK.ParameterTypes[0];
            Assert.Equal("string", parameterModel14.Name);
            Assert.Equal("", parameterModel14.Modifier);
            Assert.Null(parameterModel14.DefaultValue);
            Assert.Equal("TopLevel.Bar", methodModelK.ContainingTypeName);
            Assert.Equal("private", methodModelK.AccessModifier);
            Assert.Equal("", methodModelK.Modifier);
            Assert.Empty(methodModelK.CalledMethods);
        }

        [Fact]
        public void
            Extract_ShouldHaveMethodsWithMethodCallsWithParameterModifiers_WhenGivenMethodsThatCallOtherMethodsWithParameterModifiers()
        {
            const string fileContent = @"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {
                                        public class Foo
                                         {
                                             public void Print()
                                             {
                                                 var a=12;
                                                 F(ref a);
                                                 int b;
                                                 K(out b);
                                                 Z(2);
                                             }
                                             
                                             public int F(ref int a)
                                             {
                                                 var b = a;
                                                 a = 5;
                                                 return b;
                                             }

                                             private int K(out int a)
                                             {
                                                 var c = 6;
                                                 a = 2;
                                                 return c;
                                             }
                                             
                                             private int Z(in int a)
                                             {
                                                 var c = a;
                                                 return c*2;
                                             }
                                         }                                   
                                      }";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            Assert.Equal(4, classModels[0].Methods.Count);
            Assert.Empty(classModels[0].Constructors);

            var printMethod = classModels[0].Methods[0];
            var fMethod = classModels[0].Methods[1];
            var kMethod = classModels[0].Methods[2];
            var zMethod = classModels[0].Methods[3];

            Assert.Equal("Print", printMethod.Name);
            Assert.Equal("void", printMethod.ReturnType.Name);
            Assert.Empty(printMethod.ParameterTypes);
            Assert.Equal("TopLevel.Foo", printMethod.ContainingTypeName);
            Assert.Equal("public", printMethod.AccessModifier);
            Assert.Equal("", printMethod.Modifier);
            Assert.Equal(3, printMethod.CalledMethods.Count);

            Assert.Equal("F", printMethod.CalledMethods[0].Name);
            Assert.Equal("TopLevel.Foo", printMethod.CalledMethods[0].ContainingTypeName);
            Assert.Equal(1, printMethod.CalledMethods[0].ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)printMethod.CalledMethods[0].ParameterTypes[0];
            Assert.Equal("int", parameterModel1.Name);
            Assert.Equal("ref", parameterModel1.Modifier);
            Assert.Null(parameterModel1.DefaultValue);

            Assert.Equal("K", printMethod.CalledMethods[1].Name);
            Assert.Equal("TopLevel.Foo", printMethod.CalledMethods[1].ContainingTypeName);
            Assert.Equal(1, printMethod.CalledMethods[1].ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)printMethod.CalledMethods[1].ParameterTypes[0];
            Assert.Equal("int", parameterModel2.Name);
            Assert.Equal("out", parameterModel2.Modifier);
            Assert.Null(parameterModel2.DefaultValue);

            Assert.Equal("Z", printMethod.CalledMethods[2].Name);
            Assert.Equal("TopLevel.Foo", printMethod.CalledMethods[2].ContainingTypeName);
            Assert.Equal(1, printMethod.CalledMethods[2].ParameterTypes.Count);
            var parameterModel3 = (ParameterModel)printMethod.CalledMethods[2].ParameterTypes[0];
            Assert.Equal("int", parameterModel3.Name);
            Assert.Equal("in", parameterModel3.Modifier);
            Assert.Null(parameterModel3.DefaultValue);


            Assert.Equal("F", fMethod.Name);
            Assert.Equal("int", fMethod.ReturnType.Name);
            Assert.Equal("TopLevel.Foo", fMethod.ContainingTypeName);
            Assert.Equal("public", fMethod.AccessModifier);
            Assert.Equal("", fMethod.Modifier);
            Assert.Empty(fMethod.CalledMethods);
            Assert.Equal(1, fMethod.ParameterTypes.Count);
            var fMethodParameter = (ParameterModel)fMethod.ParameterTypes[0];
            Assert.Equal("int", fMethodParameter.Name);
            Assert.Equal("ref", fMethodParameter.Modifier);
            Assert.Null(fMethodParameter.DefaultValue);

            Assert.Equal("K", kMethod.Name);
            Assert.Equal("int", kMethod.ReturnType.Name);
            Assert.Equal("TopLevel.Foo", kMethod.ContainingTypeName);
            Assert.Equal("private", kMethod.AccessModifier);
            Assert.Equal("", kMethod.Modifier);
            Assert.Empty(kMethod.CalledMethods);
            Assert.Equal(1, kMethod.ParameterTypes.Count);
            var kMethodParameter = (ParameterModel)kMethod.ParameterTypes[0];
            Assert.Equal("int", kMethodParameter.Name);
            Assert.Equal("out", kMethodParameter.Modifier);
            Assert.Null(kMethodParameter.DefaultValue);

            Assert.Equal("Z", zMethod.Name);
            Assert.Equal("int", zMethod.ReturnType.Name);
            Assert.Equal("TopLevel.Foo", zMethod.ContainingTypeName);
            Assert.Equal("private", zMethod.AccessModifier);
            Assert.Equal("", zMethod.Modifier);
            Assert.Empty(zMethod.CalledMethods);
            Assert.Equal(1, zMethod.ParameterTypes.Count);
            var zMethodParameter = (ParameterModel)zMethod.ParameterTypes[0];
            Assert.Equal("int", zMethodParameter.Name);
            Assert.Equal("in", zMethodParameter.Modifier);
            Assert.Null(zMethodParameter.DefaultValue);
        }

        [Fact]
        public void Extract_ShouldHaveConstructors_WhenGivenClassWithConstructors()
        {
            const string fileContent = @"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {
                                          public class Foo {
                                             
                                             public Foo() {}
     
                                             private Foo(int a) {}

                                             public Foo(string a, int b=2) {}  
                                         }                                        
                                      }";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            Assert.Empty(classModels[0].Methods);
            Assert.Equal(3, classModels[0].Constructors.Count);

            var noArgConstructor = classModels[0].Constructors[0];
            Assert.Equal("Foo", noArgConstructor.Name);
            Assert.Empty(noArgConstructor.ParameterTypes);
            Assert.Equal("TopLevel.Foo", noArgConstructor.ContainingTypeName);
            Assert.Equal("public", noArgConstructor.AccessModifier);
            Assert.Equal("", noArgConstructor.Modifier);
            Assert.Empty(noArgConstructor.CalledMethods);

            var intArgConstructor = classModels[0].Constructors[1];
            Assert.Equal("Foo", intArgConstructor.Name);
            Assert.Equal(1, intArgConstructor.ParameterTypes.Count);
            var parameterModel1 = (ParameterModel)intArgConstructor.ParameterTypes[0];
            Assert.Equal("int", parameterModel1.Name);
            Assert.Equal("", parameterModel1.Modifier);
            Assert.Null(parameterModel1.DefaultValue);
            Assert.Equal("TopLevel.Foo", intArgConstructor.ContainingTypeName);
            Assert.Equal("private", intArgConstructor.AccessModifier);
            Assert.Equal("", intArgConstructor.Modifier);
            Assert.Empty(intArgConstructor.CalledMethods);

            var stringIntArgConstructor = classModels[0].Constructors[2];
            Assert.Equal("Foo", stringIntArgConstructor.Name);
            Assert.Equal(2, stringIntArgConstructor.ParameterTypes.Count);
            var parameterModel2 = (ParameterModel)stringIntArgConstructor.ParameterTypes[0];
            Assert.Equal("string", parameterModel2.Name);
            Assert.Equal("", parameterModel2.Modifier);
            Assert.Null(parameterModel2.DefaultValue);
            var parameterModel3 = (ParameterModel)stringIntArgConstructor.ParameterTypes[1];
            Assert.Equal("int", parameterModel3.Name);
            Assert.Equal("", parameterModel3.Modifier);
            Assert.Equal("2", parameterModel3.DefaultValue);
            Assert.Equal("TopLevel.Foo", stringIntArgConstructor.ContainingTypeName);
            Assert.Equal("public", stringIntArgConstructor.AccessModifier);
            Assert.Equal("", stringIntArgConstructor.Modifier);
            Assert.Empty(stringIntArgConstructor.CalledMethods);
        }

        [Fact]
        public void Extract_ShouldExtractNullDefaultValue()
        {
            const string fileContent = @"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {
                                             public class Bar
                                             {
                                                 int MethodWithNullableDefault(string? p = null) {return 2;}
                                             }                                  
                                      }";

            var classModels = _factExtractor.Extract(fileContent);
            Assert.Equal("null", ((ParameterModel)classModels[0].Methods[0].ParameterTypes[0]).DefaultValue);
        }
    }
}
