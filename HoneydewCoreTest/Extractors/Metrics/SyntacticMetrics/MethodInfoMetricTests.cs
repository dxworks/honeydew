﻿using System;
using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Extractors.Metrics.SyntacticMetrics;
using HoneydewCore.Models;
using Xunit;

namespace HoneydewCoreTest.Extractors.Metrics.SyntacticMetrics
{
    public class MethodInfoMetricTests
    {
        private readonly CSharpMetricExtractor _sut;
        private readonly IFactExtractor _factExtractor;

        public MethodInfoMetricTests()
        {
            _sut = new MethodInfoMetric();

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
            Assert.Equal("Methods Info", _sut.PrettyPrint());
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
                var optional = classModel.GetMetricValue<MethodInfoMetric>();
                Assert.True(optional.HasValue);

                var methodModels = (IList<MethodModel>) optional.Value;

                Assert.Empty(methodModels);
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
            Assert.Equal("float", classModels[0].Methods[0].ParameterTypes[0]);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[0].ContainingClass);
            Assert.Equal("protected", classModels[0].Methods[0].AccessModifier);
            Assert.Equal("", classModels[0].Methods[0].Modifier);
            Assert.Empty(classModels[0].Methods[0].CalledMethods);

            Assert.Equal("H", classModels[0].Methods[1].Name);
            Assert.Equal("bool", classModels[0].Methods[1].ReturnType);
            Assert.Empty(classModels[0].Methods[1].ParameterTypes);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[1].ContainingClass);
            Assert.Equal("public", classModels[0].Methods[1].AccessModifier);
            Assert.Equal("virtual", classModels[0].Methods[1].Modifier);
            Assert.Empty(classModels[0].Methods[1].CalledMethods);

            Assert.Equal("X", classModels[0].Methods[2].Name);
            Assert.Equal("int", classModels[0].Methods[2].ReturnType);
            Assert.Empty(classModels[0].Methods[2].ParameterTypes);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[2].ContainingClass);
            Assert.Equal("public", classModels[0].Methods[2].AccessModifier);
            Assert.Equal("abstract", classModels[0].Methods[2].Modifier);
            Assert.Empty(classModels[0].Methods[2].CalledMethods);

            Assert.Equal(2, classModels[1].Methods.Count);

            Assert.Equal("X", classModels[1].Methods[0].Name);
            Assert.Equal("int", classModels[1].Methods[0].ReturnType);
            Assert.Empty(classModels[1].Methods[0].ParameterTypes);
            Assert.Equal("TopLevel.Bar", classModels[1].Methods[0].ContainingClass);
            Assert.Equal("public", classModels[1].Methods[0].AccessModifier);
            Assert.Equal("override", classModels[1].Methods[0].Modifier);
            Assert.Empty(classModels[1].Methods[0].CalledMethods);

            Assert.Equal("H", classModels[1].Methods[1].Name);
            Assert.Equal("bool", classModels[1].Methods[1].ReturnType);
            Assert.Empty(classModels[0].Methods[1].ParameterTypes);
            Assert.Equal("TopLevel.Bar", classModels[1].Methods[1].ContainingClass);
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
            Assert.Equal("float", classModels[0].Methods[0].ParameterTypes[0]);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[0].ContainingClass);
            Assert.Equal("protected", classModels[0].Methods[0].AccessModifier);
            Assert.Equal("", classModels[0].Methods[0].Modifier);
            Assert.Empty(classModels[0].Methods[0].CalledMethods);

            Assert.Equal("H", classModels[0].Methods[1].Name);
            Assert.Equal("bool", classModels[0].Methods[1].ReturnType);
            Assert.Empty(classModels[0].Methods[1].ParameterTypes);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[1].ContainingClass);
            Assert.Equal("public", classModels[0].Methods[1].AccessModifier);
            Assert.Equal("virtual", classModels[0].Methods[1].Modifier);
            Assert.Empty(classModels[0].Methods[1].CalledMethods);

            Assert.Equal(2, classModels[1].Methods.Count);

            Assert.Equal("M", classModels[1].Methods[0].Name);
            Assert.Equal("int", classModels[1].Methods[0].ReturnType);
            Assert.Empty(classModels[1].Methods[0].ParameterTypes);
            Assert.Equal("TopLevel.Bar", classModels[1].Methods[0].ContainingClass);
            Assert.Equal("private", classModels[1].Methods[0].AccessModifier);
            Assert.Equal("", classModels[1].Methods[0].Modifier);
            Assert.Empty(classModels[1].Methods[0].CalledMethods);

            Assert.Equal("H", classModels[1].Methods[1].Name);
            Assert.Equal("bool", classModels[1].Methods[1].ReturnType);
            Assert.Empty(classModels[0].Methods[1].ParameterTypes);
            Assert.Equal("TopLevel.Bar", classModels[1].Methods[1].ContainingClass);
            Assert.Equal("public", classModels[1].Methods[1].AccessModifier);
            Assert.Equal("override", classModels[1].Methods[1].Modifier);
            Assert.Equal(3, classModels[1].Methods[1].CalledMethods.Count);
            Assert.Equal("G", classModels[1].Methods[1].CalledMethods[0].MethodName);
            Assert.Equal("TopLevel.Bar", classModels[1].Methods[1].CalledMethods[0].ContainingClass);
            Assert.Equal("H", classModels[1].Methods[1].CalledMethods[1].MethodName);
            Assert.Equal("TopLevel.Foo", classModels[1].Methods[1].CalledMethods[1].ContainingClass);
            Assert.Equal("M", classModels[1].Methods[1].CalledMethods[2].MethodName);
            Assert.Equal("TopLevel.Bar", classModels[1].Methods[1].CalledMethods[2].ContainingClass);
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
            Assert.Equal("int", classModels[0].Methods[0].ParameterTypes[0]);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[0].ContainingClass);
            Assert.Equal("public", classModels[0].Methods[0].AccessModifier);
            Assert.Equal("", classModels[0].Methods[0].Modifier);
            Assert.Empty(classModels[0].Methods[0].CalledMethods);

            Assert.Equal("B", classModels[0].Methods[1].Name);
            Assert.Equal("int", classModels[0].Methods[1].ReturnType);
            Assert.Equal(2, classModels[0].Methods[1].ParameterTypes.Count);
            Assert.Equal("int", classModels[0].Methods[1].ParameterTypes[0]);
            Assert.Equal("int", classModels[0].Methods[1].ParameterTypes[1]);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[1].ContainingClass);
            Assert.Equal("public", classModels[0].Methods[1].AccessModifier);
            Assert.Equal("", classModels[0].Methods[1].Modifier);
            Assert.Equal(2, classModels[0].Methods[1].CalledMethods.Count);
            Assert.Equal("A", classModels[0].Methods[1].CalledMethods[0].MethodName);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[1].CalledMethods[0].ContainingClass);
            Assert.Equal("A", classModels[0].Methods[1].CalledMethods[1].MethodName);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[1].CalledMethods[1].ContainingClass);

            Assert.Equal(2, classModels[1].Methods.Count);

            Assert.Equal("F", classModels[1].Methods[0].Name);
            Assert.Equal("int", classModels[1].Methods[0].ReturnType);
            Assert.Equal(3, classModels[1].Methods[0].ParameterTypes.Count);
            Assert.Equal("int", classModels[1].Methods[0].ParameterTypes[0]);
            Assert.Equal("int", classModels[1].Methods[0].ParameterTypes[1]);
            Assert.Equal("string", classModels[1].Methods[0].ParameterTypes[2]);
            Assert.Equal("TopLevel.Bar", classModels[1].Methods[0].ContainingClass);
            Assert.Equal("public", classModels[1].Methods[0].AccessModifier);
            Assert.Equal("", classModels[1].Methods[0].Modifier);
            Assert.Equal(4, classModels[1].Methods[0].CalledMethods.Count);
            Assert.Equal("A", classModels[1].Methods[0].CalledMethods[0].MethodName);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[1].CalledMethods[0].ContainingClass);
            Assert.Equal("B", classModels[1].Methods[0].CalledMethods[1].MethodName);
            Assert.Equal("TopLevel.Foo", classModels[0].Methods[1].CalledMethods[1].ContainingClass);
            Assert.Equal("K", classModels[1].Methods[0].CalledMethods[2].MethodName);
            Assert.Equal("TopLevel.Bar", classModels[1].Methods[0].CalledMethods[2].ContainingClass);
            Assert.Equal("A", classModels[1].Methods[0].CalledMethods[3].MethodName);
            Assert.Equal("TopLevel.Foo", classModels[1].Methods[0].CalledMethods[3].ContainingClass);
            
            Assert.Equal("K", classModels[1].Methods[1].Name);
            Assert.Equal("int", classModels[1].Methods[1].ReturnType);
            Assert.Equal(1, classModels[1].Methods[1].ParameterTypes.Count);
            Assert.Equal("string", classModels[1].Methods[1].ParameterTypes[0]);
            Assert.Equal("TopLevel.Bar", classModels[1].Methods[1].ContainingClass);
            Assert.Equal("private", classModels[1].Methods[1].AccessModifier);
            Assert.Equal("", classModels[1].Methods[1].Modifier);
            Assert.Empty(classModels[1].Methods[1].CalledMethods);
        }
    }
}