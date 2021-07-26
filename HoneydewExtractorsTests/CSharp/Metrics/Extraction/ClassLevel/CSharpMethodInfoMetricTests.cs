﻿using HoneydewExtractors.Core.Metrics.Extraction;
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

                var infoDataMetric = (CSharpMethodInfoDataMetric) optional.Value;

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
            Assert.Equal("int", classModels[0].Methods[0].ReturnType);
            Assert.Equal(1, classModels[0].Methods[0].ParameterTypes.Count);
            Assert.Equal("float", classModels[0].Methods[0].ParameterTypes[0].Type);
            Assert.Equal("", classModels[0].Methods[0].ParameterTypes[0].Modifier);
            Assert.Null(classModels[0].Methods[0].ParameterTypes[0].DefaultValue);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[0].ContainingClassName);
            Assert.Equal("protected", classModels[0].Methods[0].AccessModifier);
            Assert.Equal("", classModels[0].Methods[0].Modifier);
            Assert.Empty(classModels[0].Methods[0].CalledMethods);

            Assert.Equal("H", classModels[0].Methods[1].Name);
            Assert.Equal("bool", classModels[0].Methods[1].ReturnType);
            Assert.Empty(classModels[0].Methods[1].ParameterTypes);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[1].ContainingClassName);
            Assert.Equal("public", classModels[0].Methods[1].AccessModifier);
            Assert.Equal("virtual", classModels[0].Methods[1].Modifier);
            Assert.Empty(classModels[0].Methods[1].CalledMethods);

            Assert.Equal("X", classModels[0].Methods[2].Name);
            Assert.Equal("int", classModels[0].Methods[2].ReturnType);
            Assert.Empty(classModels[0].Methods[2].ParameterTypes);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[2].ContainingClassName);
            Assert.Equal("public", classModels[0].Methods[2].AccessModifier);
            Assert.Equal("abstract", classModels[0].Methods[2].Modifier);
            Assert.Empty(classModels[0].Methods[2].CalledMethods);

            Assert.Equal(2, classModels[1].Methods.Count);

            Assert.Equal("X", classModels[1].Methods[0].Name);
            Assert.Equal("int", classModels[1].Methods[0].ReturnType);
            Assert.Empty(classModels[1].Methods[0].ParameterTypes);
            Assert.Equal("TopLevel.Bar", classModels[1].Methods[0].ContainingClassName);
            Assert.Equal("public", classModels[1].Methods[0].AccessModifier);
            Assert.Equal("override", classModels[1].Methods[0].Modifier);
            Assert.Empty(classModels[1].Methods[0].CalledMethods);

            Assert.Equal("H", classModels[1].Methods[1].Name);
            Assert.Equal("bool", classModels[1].Methods[1].ReturnType);
            Assert.Empty(classModels[0].Methods[1].ParameterTypes);
            Assert.Equal("TopLevel.Bar", classModels[1].Methods[1].ContainingClassName);
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
            Assert.Equal("int", classModels[0].Methods[0].ReturnType);
            Assert.Equal(1, classModels[0].Methods[0].ParameterTypes.Count);
            Assert.Equal("float", classModels[0].Methods[0].ParameterTypes[0].Type);
            Assert.Equal("", classModels[0].Methods[0].ParameterTypes[0].Modifier);
            Assert.Null(classModels[0].Methods[0].ParameterTypes[0].DefaultValue);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[0].ContainingClassName);
            Assert.Equal("protected", classModels[0].Methods[0].AccessModifier);
            Assert.Equal("", classModels[0].Methods[0].Modifier);
            Assert.Empty(classModels[0].Methods[0].CalledMethods);

            Assert.Equal("H", classModels[0].Methods[1].Name);
            Assert.Equal("bool", classModels[0].Methods[1].ReturnType);
            Assert.Empty(classModels[0].Methods[1].ParameterTypes);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[1].ContainingClassName);
            Assert.Equal("public", classModels[0].Methods[1].AccessModifier);
            Assert.Equal("virtual", classModels[0].Methods[1].Modifier);
            Assert.Empty(classModels[0].Methods[1].CalledMethods);

            Assert.Equal(2, classModels[1].Methods.Count);

            Assert.Equal("M", classModels[1].Methods[0].Name);
            Assert.Equal("int", classModels[1].Methods[0].ReturnType);
            Assert.Empty(classModels[1].Methods[0].ParameterTypes);
            Assert.Equal("TopLevel.Bar", classModels[1].Methods[0].ContainingClassName);
            Assert.Equal("private", classModels[1].Methods[0].AccessModifier);
            Assert.Equal("", classModels[1].Methods[0].Modifier);
            Assert.Empty(classModels[1].Methods[0].CalledMethods);

            Assert.Equal("H", classModels[1].Methods[1].Name);
            Assert.Equal("bool", classModels[1].Methods[1].ReturnType);
            Assert.Empty(classModels[0].Methods[1].ParameterTypes);
            Assert.Equal("TopLevel.Bar", classModels[1].Methods[1].ContainingClassName);
            Assert.Equal("public", classModels[1].Methods[1].AccessModifier);
            Assert.Equal("override", classModels[1].Methods[1].Modifier);
            Assert.Equal(3, classModels[1].Methods[1].CalledMethods.Count);
            Assert.Equal("G", classModels[1].Methods[1].CalledMethods[0].MethodName);
            Assert.Equal("TopLevel.Bar", classModels[1].Methods[1].CalledMethods[0].ContainingClassName);
            Assert.Equal(1, classModels[1].Methods[1].CalledMethods[0].ParameterTypes.Count);
            Assert.Equal("float", classModels[1].Methods[1].CalledMethods[0].ParameterTypes[0].Type);
            Assert.Equal("", classModels[1].Methods[1].CalledMethods[0].ParameterTypes[0].Modifier);
            Assert.Null(classModels[1].Methods[1].CalledMethods[0].ParameterTypes[0].DefaultValue);
            Assert.Equal("H", classModels[1].Methods[1].CalledMethods[1].MethodName);
            Assert.Equal("TopLevel.Foo", classModels[1].Methods[1].CalledMethods[1].ContainingClassName);
            Assert.Empty(classModels[1].Methods[1].CalledMethods[1].ParameterTypes);
            Assert.Equal("M", classModels[1].Methods[1].CalledMethods[2].MethodName);
            Assert.Equal("TopLevel.Bar", classModels[1].Methods[1].CalledMethods[2].ContainingClassName);
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
            Assert.Equal("int", classModels[0].Methods[0].ReturnType);
            Assert.Equal(1, classModels[0].Methods[0].ParameterTypes.Count);
            Assert.Equal("int", classModels[0].Methods[0].ParameterTypes[0].Type);
            Assert.Equal("", classModels[0].Methods[0].ParameterTypes[0].Modifier);
            Assert.Null(classModels[0].Methods[0].ParameterTypes[0].DefaultValue);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[0].ContainingClassName);
            Assert.Equal("public", classModels[0].Methods[0].AccessModifier);
            Assert.Equal("", classModels[0].Methods[0].Modifier);
            Assert.Empty(classModels[0].Methods[0].CalledMethods);

            Assert.Equal("B", classModels[0].Methods[1].Name);
            Assert.Equal("int", classModels[0].Methods[1].ReturnType);
            Assert.Equal(2, classModels[0].Methods[1].ParameterTypes.Count);
            Assert.Equal("int", classModels[0].Methods[1].ParameterTypes[0].Type);
            Assert.Equal("", classModels[0].Methods[1].ParameterTypes[0].Modifier);
            Assert.Null(classModels[0].Methods[1].ParameterTypes[0].DefaultValue);
            Assert.Equal("int", classModels[0].Methods[1].ParameterTypes[1].Type);
            Assert.Equal("", classModels[0].Methods[1].ParameterTypes[1].Modifier);
            Assert.Null(classModels[0].Methods[1].ParameterTypes[1].DefaultValue);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[1].ContainingClassName);
            Assert.Equal("public", classModels[0].Methods[1].AccessModifier);
            Assert.Equal("", classModels[0].Methods[1].Modifier);
            Assert.Equal(2, classModels[0].Methods[1].CalledMethods.Count);
            Assert.Equal("A", classModels[0].Methods[1].CalledMethods[0].MethodName);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[1].CalledMethods[0].ContainingClassName);
            Assert.Equal(1, classModels[0].Methods[1].CalledMethods[0].ParameterTypes.Count);
            Assert.Equal("int", classModels[0].Methods[1].CalledMethods[0].ParameterTypes[0].Type);
            Assert.Equal("", classModels[0].Methods[1].CalledMethods[0].ParameterTypes[0].Modifier);
            Assert.Null(classModels[0].Methods[1].CalledMethods[0].ParameterTypes[0].DefaultValue);
            Assert.Equal("A", classModels[0].Methods[1].CalledMethods[1].MethodName);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[1].CalledMethods[1].ContainingClassName);
            Assert.Equal(1, classModels[0].Methods[1].CalledMethods[1].ParameterTypes.Count);
            Assert.Equal("int", classModels[0].Methods[1].CalledMethods[1].ParameterTypes[0].Type);
            Assert.Equal("", classModels[0].Methods[1].CalledMethods[1].ParameterTypes[0].Modifier);
            Assert.Null(classModels[0].Methods[1].CalledMethods[1].ParameterTypes[0].DefaultValue);

            Assert.Equal(2, classModels[1].Methods.Count);

            var methodModelF = classModels[1].Methods[0];
            Assert.Equal("F", methodModelF.Name);
            Assert.Equal("int", methodModelF.ReturnType);
            Assert.Equal(3, methodModelF.ParameterTypes.Count);
            Assert.Equal("int", methodModelF.ParameterTypes[0].Type);
            Assert.Equal("", methodModelF.ParameterTypes[0].Modifier);
            Assert.Null(methodModelF.ParameterTypes[0].DefaultValue);
            Assert.Equal("int", methodModelF.ParameterTypes[1].Type);
            Assert.Equal("", methodModelF.ParameterTypes[1].Modifier);
            Assert.Null(methodModelF.ParameterTypes[1].DefaultValue);
            Assert.Equal("string", methodModelF.ParameterTypes[2].Type);
            Assert.Equal("", methodModelF.ParameterTypes[2].Modifier);
            Assert.Null(methodModelF.ParameterTypes[2].DefaultValue);
            Assert.Equal("TopLevel.Bar", methodModelF.ContainingClassName);
            Assert.Equal("public", methodModelF.AccessModifier);
            Assert.Equal("", methodModelF.Modifier);
            Assert.Equal(4, methodModelF.CalledMethods.Count);
            Assert.Equal("A", methodModelF.CalledMethods[0].MethodName);
            Assert.Equal("TopLevel.Foo", methodModelF.CalledMethods[0].ContainingClassName);
            Assert.Equal(1, methodModelF.CalledMethods[0].ParameterTypes.Count);
            Assert.Equal("int", methodModelF.CalledMethods[0].ParameterTypes[0].Type);
            Assert.Equal("", methodModelF.CalledMethods[0].ParameterTypes[0].Modifier);
            Assert.Null(methodModelF.CalledMethods[0].ParameterTypes[0].DefaultValue);
            Assert.Equal("B", methodModelF.CalledMethods[1].MethodName);
            Assert.Equal("TopLevel.Foo", methodModelF.CalledMethods[1].ContainingClassName);
            Assert.Equal(2, methodModelF.CalledMethods[1].ParameterTypes.Count);
            Assert.Equal("int", methodModelF.CalledMethods[1].ParameterTypes[0].Type);
            Assert.Equal("", methodModelF.CalledMethods[1].ParameterTypes[0].Modifier);
            Assert.Null(methodModelF.CalledMethods[1].ParameterTypes[0].DefaultValue);
            Assert.Equal("int", methodModelF.CalledMethods[1].ParameterTypes[1].Type);
            Assert.Equal("", methodModelF.CalledMethods[1].ParameterTypes[1].Modifier);
            Assert.Null(methodModelF.CalledMethods[1].ParameterTypes[1].DefaultValue);
            Assert.Equal("K", methodModelF.CalledMethods[2].MethodName);
            Assert.Equal("TopLevel.Bar", methodModelF.CalledMethods[2].ContainingClassName);
            Assert.Equal(1, methodModelF.CalledMethods[2].ParameterTypes.Count);
            Assert.Equal("string", methodModelF.CalledMethods[2].ParameterTypes[0].Type);
            Assert.Equal("", methodModelF.CalledMethods[2].ParameterTypes[0].Modifier);
            Assert.Null(methodModelF.CalledMethods[2].ParameterTypes[0].DefaultValue);
            Assert.Equal("A", methodModelF.CalledMethods[3].MethodName);
            Assert.Equal("TopLevel.Foo", methodModelF.CalledMethods[3].ContainingClassName);
            Assert.Equal(1, methodModelF.CalledMethods[3].ParameterTypes.Count);
            Assert.Equal("int", methodModelF.CalledMethods[3].ParameterTypes[0].Type);
            Assert.Equal("", methodModelF.CalledMethods[3].ParameterTypes[0].Modifier);
            Assert.Null(methodModelF.CalledMethods[3].ParameterTypes[0].DefaultValue);

            var methodModelK = classModels[1].Methods[1];
            Assert.Equal("K", methodModelK.Name);
            Assert.Equal("int", methodModelK.ReturnType);
            Assert.Equal(1, methodModelK.ParameterTypes.Count);
            Assert.Equal("string", methodModelK.ParameterTypes[0].Type);
            Assert.Equal("", methodModelK.ParameterTypes[0].Modifier);
            Assert.Null(methodModelK.ParameterTypes[0].DefaultValue);
            Assert.Equal("TopLevel.Bar", methodModelK.ContainingClassName);
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
            Assert.False(printMethod.IsConstructor);
            Assert.Equal("void", printMethod.ReturnType);
            Assert.Empty(printMethod.ParameterTypes);
            Assert.Equal("TopLevel.Foo", printMethod.ContainingClassName);
            Assert.Equal("public", printMethod.AccessModifier);
            Assert.Equal("", printMethod.Modifier);
            Assert.Equal(3, printMethod.CalledMethods.Count);

            Assert.Equal("F", printMethod.CalledMethods[0].MethodName);
            Assert.Equal("TopLevel.Foo", printMethod.CalledMethods[0].ContainingClassName);
            Assert.Equal(1, printMethod.CalledMethods[0].ParameterTypes.Count);
            Assert.Equal("int", printMethod.CalledMethods[0].ParameterTypes[0].Type);
            Assert.Equal("ref", printMethod.CalledMethods[0].ParameterTypes[0].Modifier);
            Assert.Null(printMethod.CalledMethods[0].ParameterTypes[0].DefaultValue);

            Assert.Equal("K", printMethod.CalledMethods[1].MethodName);
            Assert.Equal("TopLevel.Foo", printMethod.CalledMethods[1].ContainingClassName);
            Assert.Equal(1, printMethod.CalledMethods[1].ParameterTypes.Count);
            Assert.Equal("int", printMethod.CalledMethods[1].ParameterTypes[0].Type);
            Assert.Equal("out", printMethod.CalledMethods[1].ParameterTypes[0].Modifier);
            Assert.Null(printMethod.CalledMethods[1].ParameterTypes[0].DefaultValue);

            Assert.Equal("Z", printMethod.CalledMethods[2].MethodName);
            Assert.Equal("TopLevel.Foo", printMethod.CalledMethods[2].ContainingClassName);
            Assert.Equal(1, printMethod.CalledMethods[2].ParameterTypes.Count);
            Assert.Equal("int", printMethod.CalledMethods[2].ParameterTypes[0].Type);
            Assert.Equal("in", printMethod.CalledMethods[2].ParameterTypes[0].Modifier);
            Assert.Null(printMethod.CalledMethods[2].ParameterTypes[0].DefaultValue);


            Assert.Equal("F", fMethod.Name);
            Assert.False(fMethod.IsConstructor);
            Assert.Equal("int", fMethod.ReturnType);
            Assert.Equal("TopLevel.Foo", fMethod.ContainingClassName);
            Assert.Equal("public", fMethod.AccessModifier);
            Assert.Equal("", fMethod.Modifier);
            Assert.Empty(fMethod.CalledMethods);
            Assert.Equal(1, fMethod.ParameterTypes.Count);
            Assert.Equal("int", fMethod.ParameterTypes[0].Type);
            Assert.Equal("ref", fMethod.ParameterTypes[0].Modifier);
            Assert.Null(fMethod.ParameterTypes[0].DefaultValue);

            Assert.Equal("K", kMethod.Name);
            Assert.False(kMethod.IsConstructor);
            Assert.Equal("int", kMethod.ReturnType);
            Assert.Equal("TopLevel.Foo", kMethod.ContainingClassName);
            Assert.Equal("private", kMethod.AccessModifier);
            Assert.Equal("", kMethod.Modifier);
            Assert.Empty(kMethod.CalledMethods);
            Assert.Equal(1, kMethod.ParameterTypes.Count);
            Assert.Equal("int", kMethod.ParameterTypes[0].Type);
            Assert.Equal("out", kMethod.ParameterTypes[0].Modifier);
            Assert.Null(kMethod.ParameterTypes[0].DefaultValue);

            Assert.Equal("Z", zMethod.Name);
            Assert.False(zMethod.IsConstructor);
            Assert.Equal("int", zMethod.ReturnType);
            Assert.Equal("TopLevel.Foo", zMethod.ContainingClassName);
            Assert.Equal("private", zMethod.AccessModifier);
            Assert.Equal("", zMethod.Modifier);
            Assert.Empty(zMethod.CalledMethods);
            Assert.Equal(1, zMethod.ParameterTypes.Count);
            Assert.Equal("int", zMethod.ParameterTypes[0].Type);
            Assert.Equal("in", zMethod.ParameterTypes[0].Modifier);
            Assert.Null(zMethod.ParameterTypes[0].DefaultValue);
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
            Assert.True(noArgConstructor.IsConstructor);
            Assert.Null(noArgConstructor.ReturnType);
            Assert.Empty(noArgConstructor.ParameterTypes);
            Assert.Equal("TopLevel.Foo", noArgConstructor.ContainingClassName);
            Assert.Equal("public", noArgConstructor.AccessModifier);
            Assert.Equal("", noArgConstructor.Modifier);
            Assert.Empty(noArgConstructor.CalledMethods);

            var intArgConstructor = classModels[0].Constructors[1];
            Assert.Equal("Foo", intArgConstructor.Name);
            Assert.True(intArgConstructor.IsConstructor);
            Assert.Null(intArgConstructor.ReturnType);
            Assert.Equal(1, intArgConstructor.ParameterTypes.Count);
            Assert.Equal("int", intArgConstructor.ParameterTypes[0].Type);
            Assert.Equal("", intArgConstructor.ParameterTypes[0].Modifier);
            Assert.Null(intArgConstructor.ParameterTypes[0].DefaultValue);
            Assert.Equal("TopLevel.Foo", intArgConstructor.ContainingClassName);
            Assert.Equal("private", intArgConstructor.AccessModifier);
            Assert.Equal("", intArgConstructor.Modifier);
            Assert.Empty(intArgConstructor.CalledMethods);

            var stringIntArgConstructor = classModels[0].Constructors[2];
            Assert.Equal("Foo", stringIntArgConstructor.Name);
            Assert.True(stringIntArgConstructor.IsConstructor);
            Assert.Null(stringIntArgConstructor.ReturnType);
            Assert.Equal(2, stringIntArgConstructor.ParameterTypes.Count);
            Assert.Equal("string", stringIntArgConstructor.ParameterTypes[0].Type);
            Assert.Equal("", stringIntArgConstructor.ParameterTypes[0].Modifier);
            Assert.Null(stringIntArgConstructor.ParameterTypes[0].DefaultValue);
            Assert.Equal("int", stringIntArgConstructor.ParameterTypes[1].Type);
            Assert.Equal("", stringIntArgConstructor.ParameterTypes[1].Modifier);
            Assert.Equal("2", stringIntArgConstructor.ParameterTypes[1].DefaultValue);
            Assert.Equal("TopLevel.Foo", stringIntArgConstructor.ContainingClassName);
            Assert.Equal("public", stringIntArgConstructor.AccessModifier);
            Assert.Equal("", stringIntArgConstructor.Modifier);
            Assert.Empty(stringIntArgConstructor.CalledMethods);
        }

        [Fact]
        public void Extract_ShouldHaveConstructors_WhenGivenClassWithConstructorThatCallsOtherMethods()
        {
            const string fileContent = @"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {
                                          public class Foo {
                                             
                                             public Foo(int a) {            
                                                 Function(Compute(a));                                
                                             }
     
                                             private void Function(int a) {}

                                             public int Compute(int a) {return 2*a;}  
                                         }                                        
                                      }";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            Assert.Equal(1, classModels[0].Constructors.Count);
            Assert.Equal(2, classModels[0].Methods.Count);

            var functionMethod = classModels[0].Methods[0];
            Assert.Equal("Function", functionMethod.Name);
            Assert.False(functionMethod.IsConstructor);
            Assert.Equal("void", functionMethod.ReturnType);
            Assert.Equal(1, functionMethod.ParameterTypes.Count);
            Assert.Equal("int", functionMethod.ParameterTypes[0].Type);
            Assert.Equal("", functionMethod.ParameterTypes[0].Modifier);
            Assert.Null(functionMethod.ParameterTypes[0].DefaultValue);
            Assert.Equal("TopLevel.Foo", functionMethod.ContainingClassName);
            Assert.Equal("private", functionMethod.AccessModifier);
            Assert.Equal("", functionMethod.Modifier);
            Assert.Empty(functionMethod.CalledMethods);

            var computeMethod = classModels[0].Methods[1];
            Assert.Equal("Compute", computeMethod.Name);
            Assert.False(computeMethod.IsConstructor);
            Assert.Equal("int", computeMethod.ReturnType);
            Assert.Equal(1, computeMethod.ParameterTypes.Count);
            Assert.Equal("int", computeMethod.ParameterTypes[0].Type);
            Assert.Equal("", computeMethod.ParameterTypes[0].Modifier);
            Assert.Null(computeMethod.ParameterTypes[0].DefaultValue);
            Assert.Equal("TopLevel.Foo", computeMethod.ContainingClassName);
            Assert.Equal("public", computeMethod.AccessModifier);
            Assert.Equal("", computeMethod.Modifier);
            Assert.Empty(computeMethod.CalledMethods);

            var intArgConstructor = classModels[0].Constructors[0];
            Assert.Equal("Foo", intArgConstructor.Name);
            Assert.True(intArgConstructor.IsConstructor);
            Assert.Null(intArgConstructor.ReturnType);
            Assert.Equal(1, intArgConstructor.ParameterTypes.Count);
            Assert.Equal("int", intArgConstructor.ParameterTypes[0].Type);
            Assert.Equal("", intArgConstructor.ParameterTypes[0].Modifier);
            Assert.Null(intArgConstructor.ParameterTypes[0].DefaultValue);
            Assert.Equal("TopLevel.Foo", intArgConstructor.ContainingClassName);
            Assert.Equal("public", intArgConstructor.AccessModifier);
            Assert.Equal("", intArgConstructor.Modifier);
            Assert.Equal(2, intArgConstructor.CalledMethods.Count);

            Assert.Equal("Function", intArgConstructor.CalledMethods[0].MethodName);
            Assert.Equal("TopLevel.Foo", intArgConstructor.CalledMethods[0].ContainingClassName);
            Assert.Equal(1, intArgConstructor.CalledMethods[0].ParameterTypes.Count);
            Assert.Equal("int", intArgConstructor.CalledMethods[0].ParameterTypes[0].Type);
            Assert.Equal("", intArgConstructor.CalledMethods[0].ParameterTypes[0].Modifier);
            Assert.Null(intArgConstructor.CalledMethods[0].ParameterTypes[0].DefaultValue);

            Assert.Equal("Compute", intArgConstructor.CalledMethods[1].MethodName);
            Assert.Equal("TopLevel.Foo", intArgConstructor.CalledMethods[1].ContainingClassName);
            Assert.Equal(1, intArgConstructor.CalledMethods[1].ParameterTypes.Count);
            Assert.Equal("int", intArgConstructor.CalledMethods[1].ParameterTypes[0].Type);
            Assert.Equal("", intArgConstructor.CalledMethods[1].ParameterTypes[0].Modifier);
            Assert.Null(intArgConstructor.CalledMethods[1].ParameterTypes[0].DefaultValue);
        }

        [Fact]
        public void Extract_ShouldHaveConstructors_WhenGivenClassWithConstructorsThatCallEachOther()
        {
            const string fileContent = @"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {
                                        public class Foo
                                         {
                                             public Foo() : this(2) { }

                                             public Foo(int a) : this(""value"") { Print(); }

                                             public Foo(string a, int b = 2) { }
                                             
                                             public void Print(){}
                                       }                                    
                                      }";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            Assert.Equal(1, classModels[0].Methods.Count);
            Assert.Equal(3, classModels[0].Constructors.Count);

            var noArgConstructor = classModels[0].Constructors[0];
            var intArgConstructor = classModels[0].Constructors[1];
            var stringIntArgConstructor = classModels[0].Constructors[2];
            var printMethod = classModels[0].Methods[0];

            Assert.Equal("Foo", noArgConstructor.Name);
            Assert.True(noArgConstructor.IsConstructor);
            Assert.Null(noArgConstructor.ReturnType);
            Assert.Empty(noArgConstructor.ParameterTypes);
            Assert.Equal("TopLevel.Foo", noArgConstructor.ContainingClassName);
            Assert.Equal("public", noArgConstructor.AccessModifier);
            Assert.Equal("", noArgConstructor.Modifier);
            Assert.Equal(1, noArgConstructor.CalledMethods.Count);
            Assert.Equal("Foo", noArgConstructor.CalledMethods[0].MethodName);
            Assert.Equal("TopLevel.Foo", noArgConstructor.CalledMethods[0].ContainingClassName);
            Assert.Equal(1, noArgConstructor.CalledMethods[0].ParameterTypes.Count);
            Assert.Equal("int", noArgConstructor.CalledMethods[0].ParameterTypes[0].Type);
            Assert.Equal("", noArgConstructor.CalledMethods[0].ParameterTypes[0].Modifier);
            Assert.Null(noArgConstructor.CalledMethods[0].ParameterTypes[0].DefaultValue);


            Assert.Equal("Foo", intArgConstructor.Name);
            Assert.Equal("TopLevel.Foo", intArgConstructor.ContainingClassName);
            Assert.True(intArgConstructor.IsConstructor);
            Assert.Null(intArgConstructor.ReturnType);
            Assert.Equal(1, intArgConstructor.ParameterTypes.Count);
            Assert.Equal("int", intArgConstructor.ParameterTypes[0].Type);
            Assert.Equal("", intArgConstructor.ParameterTypes[0].Modifier);
            Assert.Null(intArgConstructor.ParameterTypes[0].DefaultValue);
            Assert.Equal("TopLevel.Foo", intArgConstructor.ContainingClassName);
            Assert.Equal("public", intArgConstructor.AccessModifier);
            Assert.Equal("", intArgConstructor.Modifier);
            Assert.Equal(2, intArgConstructor.CalledMethods.Count);

            Assert.Equal("Foo", intArgConstructor.CalledMethods[0].MethodName);
            Assert.Equal("TopLevel.Foo", intArgConstructor.CalledMethods[0].ContainingClassName);
            Assert.Equal(2, intArgConstructor.CalledMethods[0].ParameterTypes.Count);
            Assert.Equal("string", intArgConstructor.CalledMethods[0].ParameterTypes[0].Type);
            Assert.Equal("", intArgConstructor.CalledMethods[0].ParameterTypes[0].Modifier);
            Assert.Null(intArgConstructor.CalledMethods[0].ParameterTypes[0].DefaultValue);
            Assert.Equal("int", intArgConstructor.CalledMethods[0].ParameterTypes[1].Type);
            Assert.Equal("", intArgConstructor.CalledMethods[0].ParameterTypes[1].Modifier);
            Assert.Equal("2", intArgConstructor.CalledMethods[0].ParameterTypes[1].DefaultValue);

            Assert.Equal("Print", intArgConstructor.CalledMethods[1].MethodName);
            Assert.Equal("TopLevel.Foo", intArgConstructor.CalledMethods[1].ContainingClassName);
            Assert.Empty(intArgConstructor.CalledMethods[1].ParameterTypes);


            Assert.Equal("Foo", stringIntArgConstructor.Name);
            Assert.Equal("TopLevel.Foo", stringIntArgConstructor.ContainingClassName);
            Assert.True(stringIntArgConstructor.IsConstructor);
            Assert.Null(stringIntArgConstructor.ReturnType);
            Assert.Equal(2, stringIntArgConstructor.ParameterTypes.Count);
            Assert.Equal("string", stringIntArgConstructor.ParameterTypes[0].Type);
            Assert.Equal("", stringIntArgConstructor.ParameterTypes[0].Modifier);
            Assert.Null(stringIntArgConstructor.ParameterTypes[0].DefaultValue);
            Assert.Equal("int", stringIntArgConstructor.ParameterTypes[1].Type);
            Assert.Equal("", stringIntArgConstructor.ParameterTypes[1].Modifier);
            Assert.Equal("2", stringIntArgConstructor.ParameterTypes[1].DefaultValue);
            Assert.Equal("TopLevel.Foo", stringIntArgConstructor.ContainingClassName);
            Assert.Equal("public", stringIntArgConstructor.AccessModifier);
            Assert.Equal("", stringIntArgConstructor.Modifier);
            Assert.Empty(stringIntArgConstructor.CalledMethods);

            Assert.Equal("Print", printMethod.Name);
            Assert.Equal("TopLevel.Foo", printMethod.ContainingClassName);
            Assert.False(printMethod.IsConstructor);
            Assert.Equal("void", printMethod.ReturnType);
            Assert.Empty(printMethod.ParameterTypes);
            Assert.Equal("public", printMethod.AccessModifier);
            Assert.Equal("", printMethod.Modifier);
            Assert.Empty(printMethod.CalledMethods);
        }

        [Fact]
        public void Extract_ShouldHaveConstructors_WhenGivenClassWithConstructorsThatCallEachOtherAndBaseConstructor()
        {
            const string fileContent = @"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {
                                        public class Foo
                                         {
                                             public Foo() : this(2) { }

                                             public Foo(int a):this(a, 6) { }
                                             
                                             public Foo(int a, int b) { }
                                         }
                                         
                                         public class Bar : Foo
                                         {
                                             public Bar() : base(2) { }

                                             public Bar(int a) : base() {  }

                                             public Bar(string a,in int b=52) : this() { }
                                         }                                   
                                      }";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(2, classModels.Count);

            Assert.Empty(classModels[0].Methods);
            Assert.Empty(classModels[1].Methods);

            Assert.Equal(3, classModels[0].Constructors.Count);
            Assert.Equal(3, classModels[1].Constructors.Count);

            var noArgConstructorBase = classModels[0].Constructors[0];
            var intArgConstructorBase = classModels[0].Constructors[1];
            var intIntConstructorBase = classModels[0].Constructors[2];

            AssertBasicConstructorInfo(noArgConstructorBase, "Foo");
            Assert.Empty(noArgConstructorBase.ParameterTypes);
            Assert.Equal(1, noArgConstructorBase.CalledMethods.Count);
            Assert.Equal("Foo", noArgConstructorBase.CalledMethods[0].MethodName);
            Assert.Equal("TopLevel.Foo", noArgConstructorBase.CalledMethods[0].ContainingClassName);
            Assert.Equal(1, noArgConstructorBase.CalledMethods[0].ParameterTypes.Count);
            Assert.Equal("int", noArgConstructorBase.CalledMethods[0].ParameterTypes[0].Type);
            Assert.Equal("", noArgConstructorBase.CalledMethods[0].ParameterTypes[0].Modifier);
            Assert.Null(noArgConstructorBase.CalledMethods[0].ParameterTypes[0].DefaultValue);

            AssertBasicConstructorInfo(intArgConstructorBase, "Foo");
            Assert.Equal(1, intArgConstructorBase.ParameterTypes.Count);
            Assert.Equal("int", intArgConstructorBase.ParameterTypes[0].Type);
            Assert.Equal("", intArgConstructorBase.ParameterTypes[0].Modifier);
            Assert.Null(intArgConstructorBase.ParameterTypes[0].DefaultValue);
            Assert.Equal(1, intArgConstructorBase.CalledMethods.Count);
            Assert.Equal("Foo", intArgConstructorBase.CalledMethods[0].MethodName);
            Assert.Equal("TopLevel.Foo", intArgConstructorBase.CalledMethods[0].ContainingClassName);
            Assert.Equal(2, intArgConstructorBase.CalledMethods[0].ParameterTypes.Count);
            Assert.Equal("int", intArgConstructorBase.CalledMethods[0].ParameterTypes[0].Type);
            Assert.Equal("", intArgConstructorBase.CalledMethods[0].ParameterTypes[0].Modifier);
            Assert.Null(intArgConstructorBase.CalledMethods[0].ParameterTypes[0].DefaultValue);
            Assert.Equal("int", intArgConstructorBase.CalledMethods[0].ParameterTypes[1].Type);
            Assert.Equal("", intArgConstructorBase.CalledMethods[0].ParameterTypes[1].Modifier);
            Assert.Null(intArgConstructorBase.CalledMethods[0].ParameterTypes[1].DefaultValue);

            AssertBasicConstructorInfo(intArgConstructorBase, "Foo");
            Assert.Equal(2, intIntConstructorBase.ParameterTypes.Count);
            Assert.Equal("int", intIntConstructorBase.ParameterTypes[0].Type);
            Assert.Equal("", intIntConstructorBase.ParameterTypes[0].Modifier);
            Assert.Null(intIntConstructorBase.ParameterTypes[0].DefaultValue);
            Assert.Equal("int", intIntConstructorBase.ParameterTypes[1].Type);
            Assert.Equal("", intIntConstructorBase.ParameterTypes[1].Modifier);
            Assert.Null(intIntConstructorBase.ParameterTypes[1].DefaultValue);
            Assert.Empty(intIntConstructorBase.CalledMethods);

            var noArgConstructorChild = classModels[1].Constructors[0];
            var intArgConstructorChild = classModels[1].Constructors[1];
            var stringIntConstructorBase = classModels[1].Constructors[2];

            AssertBasicConstructorInfo(noArgConstructorChild, "Bar");
            Assert.Empty(noArgConstructorChild.ParameterTypes);
            Assert.Equal(1, noArgConstructorChild.CalledMethods.Count);
            Assert.Equal("Foo", noArgConstructorChild.CalledMethods[0].MethodName);
            Assert.Equal("TopLevel.Foo", noArgConstructorChild.CalledMethods[0].ContainingClassName);
            Assert.Equal(1, noArgConstructorChild.CalledMethods[0].ParameterTypes.Count);
            Assert.Equal("int", noArgConstructorChild.CalledMethods[0].ParameterTypes[0].Type);
            Assert.Equal("", noArgConstructorChild.CalledMethods[0].ParameterTypes[0].Modifier);
            Assert.Null(noArgConstructorChild.CalledMethods[0].ParameterTypes[0].DefaultValue);

            AssertBasicConstructorInfo(intArgConstructorChild, "Bar");
            Assert.Equal(1, intArgConstructorChild.ParameterTypes.Count);
            Assert.Equal("int", intArgConstructorChild.ParameterTypes[0].Type);
            Assert.Equal("", intArgConstructorChild.ParameterTypes[0].Modifier);
            Assert.Null(intArgConstructorChild.ParameterTypes[0].DefaultValue);
            Assert.Equal(1, intArgConstructorChild.CalledMethods.Count);
            Assert.Equal("Foo", intArgConstructorChild.CalledMethods[0].MethodName);
            Assert.Equal("TopLevel.Foo", intArgConstructorChild.CalledMethods[0].ContainingClassName);
            Assert.Empty(intArgConstructorChild.CalledMethods[0].ParameterTypes);

            AssertBasicConstructorInfo(intArgConstructorChild, "Bar");
            Assert.Equal(2, stringIntConstructorBase.ParameterTypes.Count);
            Assert.Equal("string", stringIntConstructorBase.ParameterTypes[0].Type);
            Assert.Equal("", stringIntConstructorBase.ParameterTypes[0].Modifier);
            Assert.Null(stringIntConstructorBase.ParameterTypes[0].DefaultValue);
            Assert.Equal("int", stringIntConstructorBase.ParameterTypes[1].Type);
            Assert.Equal("in", stringIntConstructorBase.ParameterTypes[1].Modifier);
            Assert.Equal("52", stringIntConstructorBase.ParameterTypes[1].DefaultValue);
            Assert.Equal(1, stringIntConstructorBase.CalledMethods.Count);
            Assert.Equal("Bar", stringIntConstructorBase.CalledMethods[0].MethodName);
            Assert.Equal("TopLevel.Bar", stringIntConstructorBase.CalledMethods[0].ContainingClassName);
            Assert.Empty(stringIntConstructorBase.CalledMethods[0].ParameterTypes);

            static void AssertBasicConstructorInfo(MethodModel constructorModel, string className)
            {
                Assert.Equal(className, constructorModel.Name);
                Assert.Equal($"TopLevel.{className}", constructorModel.ContainingClassName);
                Assert.True(constructorModel.IsConstructor);
                Assert.Null(constructorModel.ReturnType);
                Assert.Equal("public", constructorModel.AccessModifier);
                Assert.Equal("", constructorModel.Modifier);
            }
        }

        [Fact]
        public void
            Extract_ShouldHaveConstructors_WhenGivenClassWithConstructorsThatCallsBaseConstructor_ButBaseClassIsNotPresentInCompilationUnit()
        {
            const string fileContent = @"using System;

                                      namespace TopLevel
                                      {
                                         public class Bar : Foo
                                         {
                                             public Bar() : base(2) { }

                                             public Bar(int a) : base() {  }
                                         }                                   
                                      }";

            var classModels = _factExtractor.Extract(fileContent);

            Assert.Equal(1, classModels.Count);

            Assert.Empty(classModels[0].Methods);

            Assert.Equal(2, classModels[0].Constructors.Count);

            var noArgConstructorChild = classModels[0].Constructors[0];
            var intArgConstructorChild = classModels[0].Constructors[1];

            AssertBasicConstructorInfo(noArgConstructorChild, "Bar");
            Assert.Empty(noArgConstructorChild.ParameterTypes);
            Assert.Equal(1, noArgConstructorChild.CalledMethods.Count);
            Assert.Equal("Foo", noArgConstructorChild.CalledMethods[0].MethodName);
            Assert.Equal("Foo", noArgConstructorChild.CalledMethods[0].ContainingClassName);
            Assert.Empty(noArgConstructorChild.CalledMethods[0].ParameterTypes);

            AssertBasicConstructorInfo(intArgConstructorChild, "Bar");
            Assert.Equal(1, intArgConstructorChild.ParameterTypes.Count);
            Assert.Equal("int", intArgConstructorChild.ParameterTypes[0].Type);
            Assert.Equal("", intArgConstructorChild.ParameterTypes[0].Modifier);
            Assert.Null(intArgConstructorChild.ParameterTypes[0].DefaultValue);
            Assert.Equal(1, intArgConstructorChild.CalledMethods.Count);
            Assert.Equal("Foo", intArgConstructorChild.CalledMethods[0].MethodName);
            Assert.Equal("Foo", intArgConstructorChild.CalledMethods[0].ContainingClassName);
            Assert.Empty(intArgConstructorChild.CalledMethods[0].ParameterTypes);

            static void AssertBasicConstructorInfo(MethodModel constructorModel, string className)
            {
                Assert.Equal(className, constructorModel.Name);
                Assert.Equal($"TopLevel.{className}", constructorModel.ContainingClassName);
                Assert.True(constructorModel.IsConstructor);
                Assert.Null(constructorModel.ReturnType);
                Assert.Equal("public", constructorModel.AccessModifier);
                Assert.Equal("", constructorModel.Modifier);
            }
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
            Assert.Equal("null", classModels[0].Methods[0].ParameterTypes[0].DefaultValue);
        }

        [Fact]
        public void Extract_ShouldJaveCalledMethods_WhenProvidedClassThatCallsMethodsFromAFieldOfADifferentClass()
        {
            const string fileContent = @"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {
                                            public class Foo 
                                            {
                                                public void Method(int a) 
                                                {
                                                }
                                            }

                                             public class Bar
                                             {
                                                private Foo _foo;

                                                void M()
                                                {
                                                    _foo.Method(2);    
                                                }
                                             }                                  
                                      }";

            var classModels = _factExtractor.Extract(fileContent);

            var methodModel = classModels[1].Methods[0];

            Assert.Equal("M", methodModel.Name);
            Assert.Equal(1, methodModel.CalledMethods.Count);
            Assert.Equal("Method", methodModel.CalledMethods[0].MethodName);
            Assert.Equal("TopLevel.Foo", methodModel.CalledMethods[0].ContainingClassName);
            Assert.Equal(1, methodModel.CalledMethods[0].ParameterTypes.Count);
            Assert.Equal("", methodModel.CalledMethods[0].ParameterTypes[0].Modifier);
            Assert.Null(methodModel.CalledMethods[0].ParameterTypes[0].DefaultValue);
            Assert.Equal("int", methodModel.CalledMethods[0].ParameterTypes[0].Type);
        }

        [Fact]
        public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallsStaticMethods()
        {
            const string fileContent = @"using System;

                                          namespace TopLevel
                                          {
                                                public class Foo 
                                                {
                                                    public static void Method(int a) 
                                                    {
                                                    }
                                                }
    
                                                 public class Bar
                                                 {
                                                    private static void OtherMethod(){}
    
                                                    void M()
                                                    {
                                                        OtherMethod();
                                                        Foo.Method(2);    
                                                        int.Parse(""5"");
                                                    }
                                                 }                                  
                                          }";

            var classModels = _factExtractor.Extract(fileContent);

            var methodModelM = classModels[1].Methods[1];
            Assert.Equal("M", methodModelM.Name);
            Assert.Equal(3, methodModelM.CalledMethods.Count);

            var calledMethod1 = methodModelM.CalledMethods[0];
            Assert.Equal("OtherMethod", calledMethod1.MethodName);
            Assert.Equal("TopLevel.Bar", calledMethod1.ContainingClassName);
            Assert.Empty(calledMethod1.ParameterTypes);

            var calledMethod2 = methodModelM.CalledMethods[1];
            Assert.Equal("Method", calledMethod2.MethodName);
            Assert.Equal("TopLevel.Foo", calledMethod2.ContainingClassName);
            Assert.Equal(1, calledMethod2.ParameterTypes.Count);
            Assert.Equal("", calledMethod2.ParameterTypes[0].Modifier);
            Assert.Null(calledMethod2.ParameterTypes[0].DefaultValue);
            Assert.Equal("int", calledMethod2.ParameterTypes[0].Type);

            var calledMethod3 = methodModelM.CalledMethods[2];
            Assert.Equal("Parse", calledMethod3.MethodName);
            Assert.Equal("int", calledMethod3.ContainingClassName);
            Assert.Equal(1, calledMethod3.ParameterTypes.Count);
            Assert.Equal("", calledMethod3.ParameterTypes[0].Modifier);
            Assert.Null(calledMethod3.ParameterTypes[0].DefaultValue);
            Assert.Equal("string", calledMethod3.ParameterTypes[0].Type);
        }

        [Fact]
        public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallsStaticMethodsFromUnknownClass()
        {
            const string fileContent = @"using System;
                                          
                                          namespace TopLevel
                                          {                                                   
                                                 public class Bar
                                                 {
                                                    void M()
                                                    {
                                                        Foo.Method(2);    
                                                    }
                                                 }                                  
                                          }";

            var classModels = _factExtractor.Extract(fileContent);

            var methodModelM = classModels[0].Methods[0];
            Assert.Equal("M", methodModelM.Name);
            Assert.Equal(1, methodModelM.CalledMethods.Count);

            var calledMethod = methodModelM.CalledMethods[0];
            Assert.Equal("Method", calledMethod.MethodName);
            Assert.Equal("Foo", calledMethod.ContainingClassName);
            Assert.Empty(calledMethod.ParameterTypes);
        }

        [Fact]
        public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallsFuncLambdas()
        {
            const string fileContent = @"using System;

                                          namespace TopLevel
                                          {    
                                                 public class Bar
                                                 {
                                                    void Other(Func<int> a)
                                                    {
                                                    } 
                                                    void Method()
                                                    {
                                                        Func<int> func = () => 66;
                                                        Other(func);
                                                        Other(() => 0);
                                                        Other(() => { return 6;});
                                                        Other(A);
                                                        Other(delegate { return 7;});
                                                    }

                                                    private int A()
                                                    {
                                                        throw new NotImplementedException();
                                                    }
                                                 }                                  
                                          }";

            var classModels = _factExtractor.Extract(fileContent);

            var methodModelM = classModels[0].Methods[1];
            Assert.Equal("Method", methodModelM.Name);
            Assert.Equal(5, methodModelM.CalledMethods.Count);

            foreach (var calledMethod in methodModelM.CalledMethods)
            {
                Assert.Equal("Other", calledMethod.MethodName);
                Assert.Equal("TopLevel.Bar", calledMethod.ContainingClassName);
                Assert.Equal(1, calledMethod.ParameterTypes.Count);
                Assert.Equal("", calledMethod.ParameterTypes[0].Modifier);
                Assert.Null(calledMethod.ParameterTypes[0].DefaultValue);
                Assert.Equal("System.Func<int>", calledMethod.ParameterTypes[0].Type);
            }
        }

        [Fact]
        public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallsActionLambdas()
        {
            const string fileContent = @"using System;
                                          namespace TopLevel
                                          {    
                                                 
                                            public class Bar
                                            {
                                                void Other(System.Action<int> a)
                                                {
                                                }

                                                void Method()
                                                {
                                                    Other(i => { });
                                                                                            
                                                    Other(i =>
                                                    {
                                                        i++;
                                                    });
                                                                                            
                                                    Other(delegate(int i) {  });
                                                    Other(A);
                                                }

                                                private void A(int obj)
                                                {
                                                    throw new System.NotImplementedException();
                                                }
                                            }                                      
                                          }";
            var classModels = _factExtractor.Extract(fileContent);

            var methodModelM = classModels[0].Methods[1];
            Assert.Equal("Method", methodModelM.Name);
            Assert.Equal(4, methodModelM.CalledMethods.Count);

            foreach (var calledMethod in methodModelM.CalledMethods)
            {
                Assert.Equal("Other", calledMethod.MethodName);
                Assert.Equal("TopLevel.Bar", calledMethod.ContainingClassName);
                Assert.Equal(1, calledMethod.ParameterTypes.Count);
                Assert.Equal("", calledMethod.ParameterTypes[0].Modifier);
                Assert.Null(calledMethod.ParameterTypes[0].DefaultValue);
                Assert.Equal("System.Action<int>", calledMethod.ParameterTypes[0].Type);
            }
        }

        [Fact]
        public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallMethodsInsideLambdas()
        {
            const string fileContent = @"using System;
                                          namespace TopLevel
                                          {           
                                             public class Bar
                                            {
                                                void Other(System.Action<int> a)
                                                {
                                                }

                                                void Other(Func<int> a)
                                                {
                                                }
                                                void Method()
                                                {
                                                    Other(i => { Calc(i);});

                                                    Other(() => Calc(2));
                                                }

                                                private int Calc(int a)
                                                {
                                                    return a * 2;
                                                }
                                            }                                       
                                          }";
            var classModels = _factExtractor.Extract(fileContent);

            var methodModelM = classModels[0].Methods[2];
            Assert.Equal("Method", methodModelM.Name);
            Assert.Equal(4, methodModelM.CalledMethods.Count);


            var calledMethod1 = methodModelM.CalledMethods[0];
            Assert.Equal("Other", calledMethod1.MethodName);
            Assert.Equal("TopLevel.Bar", calledMethod1.ContainingClassName);
            Assert.Equal(1, calledMethod1.ParameterTypes.Count);
            Assert.Equal("", calledMethod1.ParameterTypes[0].Modifier);
            Assert.Null(calledMethod1.ParameterTypes[0].DefaultValue);
            Assert.Equal("System.Action<int>", calledMethod1.ParameterTypes[0].Type);

            var calledMethod2 = methodModelM.CalledMethods[1];
            Assert.Equal("Calc", calledMethod2.MethodName);
            Assert.Equal("TopLevel.Bar", calledMethod2.ContainingClassName);
            Assert.Equal(1, calledMethod2.ParameterTypes.Count);
            Assert.Equal("", calledMethod2.ParameterTypes[0].Modifier);
            Assert.Null(calledMethod2.ParameterTypes[0].DefaultValue);
            Assert.Equal("int", calledMethod2.ParameterTypes[0].Type);

            var calledMethod3 = methodModelM.CalledMethods[2];
            Assert.Equal("Other", calledMethod3.MethodName);
            Assert.Equal("TopLevel.Bar", calledMethod3.ContainingClassName);
            Assert.Equal(1, calledMethod3.ParameterTypes.Count);
            Assert.Equal("", calledMethod3.ParameterTypes[0].Modifier);
            Assert.Null(calledMethod3.ParameterTypes[0].DefaultValue);
            Assert.Equal("System.Func<int>", calledMethod3.ParameterTypes[0].Type);

            var calledMethod4 = methodModelM.CalledMethods[3];
            Assert.Equal("Calc", calledMethod4.MethodName);
            Assert.Equal("TopLevel.Bar", calledMethod4.ContainingClassName);
            Assert.Equal(1, calledMethod4.ParameterTypes.Count);
            Assert.Equal("", calledMethod4.ParameterTypes[0].Modifier);
            Assert.Null(calledMethod4.ParameterTypes[0].DefaultValue);
            Assert.Equal("int", calledMethod4.ParameterTypes[0].Type);
        }

        [Fact]
        public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallHasCalledMethodsChained()
        {
            const string fileContent = @"using System;

                                          namespace TopLevel
                                          {           
                                            class Foo
                                            {
                                                public string GetName()
                                                {
                                                    return """";
                                                } 
                                            }
                                            
                                            class Builder
                                            {
                                                public Builder Set()
                                                {
                                                    return this;
                                                }

                                                public Foo Build()
                                                {
                                                    return new Foo();
                                                }
                                            }
                                            
                                            public class Bar
                                            {
                                                
                                                void Method()
                                                {
                                                    var a = Create()
                                                        .Set()
                                                        .Build()
                                                        .GetName().Trim();
                                                }

                                                private Builder Create()
                                                {
                                                    return new Builder();
                                                }
                                            }                                          
                                          }";
            var classModels = _factExtractor.Extract(fileContent);

            var methodModelM = classModels[2].Methods[0];
            Assert.Equal("Method", methodModelM.Name);
            Assert.Equal(5, methodModelM.CalledMethods.Count);

            var calledMethod0 = methodModelM.CalledMethods[0];
            Assert.Equal("Trim", calledMethod0.MethodName);
            Assert.Equal("string", calledMethod0.ContainingClassName);
            Assert.Empty(calledMethod0.ParameterTypes);

            var calledMethod1 = methodModelM.CalledMethods[1];
            Assert.Equal("GetName", calledMethod1.MethodName);
            Assert.Equal("TopLevel.Foo", calledMethod1.ContainingClassName);
            Assert.Empty(calledMethod1.ParameterTypes);

            var calledMethod2 = methodModelM.CalledMethods[2];
            Assert.Equal("Build", calledMethod2.MethodName);
            Assert.Equal("TopLevel.Builder", calledMethod2.ContainingClassName);
            Assert.Empty(calledMethod2.ParameterTypes);

            var calledMethod3 = methodModelM.CalledMethods[3];
            Assert.Equal("Set", calledMethod3.MethodName);
            Assert.Equal("TopLevel.Builder", calledMethod3.ContainingClassName);
            Assert.Empty(calledMethod3.ParameterTypes);

            var calledMethod4 = methodModelM.CalledMethods[4];
            Assert.Equal("Create", calledMethod4.MethodName);
            Assert.Equal("TopLevel.Bar", calledMethod4.ContainingClassName);
            Assert.Empty(calledMethod4.ParameterTypes);
        }

        [Fact]
        public void Extract_ShouldHaveCalledMethods_WhenProvidedClassThatCallsLinqMethods()
        {
            const string fileContent = @"
                                        using System;
                                        using System.Collections.Generic;
                                        using System.Linq;
                                          namespace TopLevel
                                          {                                                       
                                            public class Bar
                                            {
                                                void Method()
                                                {
                                                    var list = new List<string>();
                                                    var enumerable = list.Where(s => s != null).Skip(6).Select(s=>s.Trim()).ToList();
                                                }
                                            }                                          
                                          }";

            var classModels = _factExtractor.Extract(fileContent);

            var methodModelM = classModels[0].Methods[0];
            Assert.Equal("Method", methodModelM.Name);
            Assert.Equal(5, methodModelM.CalledMethods.Count);

            var calledMethod0 = methodModelM.CalledMethods[0];
            Assert.Equal("ToList", calledMethod0.MethodName);
            Assert.Equal("System.Collections.Generic.IEnumerable<string>", calledMethod0.ContainingClassName);
            Assert.Empty(calledMethod0.ParameterTypes);

            var calledMethod1 = methodModelM.CalledMethods[1];
            Assert.Equal("Select", calledMethod1.MethodName);
            Assert.Equal("System.Collections.Generic.IEnumerable<string>", calledMethod1.ContainingClassName);
            Assert.Equal(1, calledMethod1.ParameterTypes.Count);
            Assert.Equal("", calledMethod1.ParameterTypes[0].Modifier);
            Assert.Equal("System.Func<string, string>", calledMethod1.ParameterTypes[0].Type);
            Assert.Null(calledMethod1.ParameterTypes[0].DefaultValue);

            var calledMethod2 = methodModelM.CalledMethods[2];
            Assert.Equal("Skip", calledMethod2.MethodName);
            Assert.Equal("System.Collections.Generic.IEnumerable<string>", calledMethod2.ContainingClassName);
            Assert.Equal(1, calledMethod2.ParameterTypes.Count);
            Assert.Equal("", calledMethod2.ParameterTypes[0].Modifier);
            Assert.Equal("int", calledMethod2.ParameterTypes[0].Type);
            Assert.Null(calledMethod2.ParameterTypes[0].DefaultValue);

            var calledMethod3 = methodModelM.CalledMethods[3];
            Assert.Equal("Where", calledMethod3.MethodName);
            Assert.Equal("System.Collections.Generic.List<string>", calledMethod3.ContainingClassName);
            Assert.Equal(1, calledMethod3.ParameterTypes.Count);
            Assert.Equal("", calledMethod3.ParameterTypes[0].Modifier);
            Assert.Equal("System.Func<string, bool>", calledMethod3.ParameterTypes[0].Type);
            Assert.Null(calledMethod3.ParameterTypes[0].DefaultValue);

            var calledMethod4 = methodModelM.CalledMethods[4];
            Assert.Equal("Trim", calledMethod4.MethodName);
            Assert.Equal("string", calledMethod4.ContainingClassName);
            Assert.Empty(calledMethod4.ParameterTypes);
        }
    }
}
