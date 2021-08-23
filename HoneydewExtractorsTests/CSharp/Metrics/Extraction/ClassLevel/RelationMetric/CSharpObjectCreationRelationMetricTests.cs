using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Extraction.Class;
using HoneydewExtractors.Core.Metrics.Extraction.Class.Relations;
using HoneydewExtractors.Core.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel.RelationMetric
{
    public class CSharpObjectCreationRelationMetricTests
    {
        private readonly ObjectCreationRelationVisitor _sut;
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpObjectCreationRelationMetricTests()
        {
            _sut = new ObjectCreationRelationVisitor(new RelationMetricHolder());

            var compositeVisitor = new CompositeVisitor();
            
            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                _sut
            }));
            
            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker()), compositeVisitor);
        }

        [Fact]
        public void PrettyPrint_ShouldReturnReturnValueDependency()
        {
            Assert.Equal("Object Creation Dependency", _sut.PrettyPrint());
        }

        [Fact]
        public void Extract_ShouldHaveObjectCreation_WhenProvidedWithClassInTheSameNamespace()
        {
            const string fileContent = @"using System;
                                     namespace App
                                     {                
                                         class C {}
                       
                                         class MyClass
                                         {                                           
                                            private C _c = new C();
                                            private C _c2 = new();

                                            public C MyC {get;set;} = new C();
                                            public C ComputedC => new();
                                            public C MyC2
                                            {
                                                get { return new C(); }
                                            }

                                            public MyClass() {
                                                new C();
                                                C c = new();
                                            }

                                            public C Method() {
                                                var c = new C();
                                                return c;
                                            }
                                         }
                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[1].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ObjectCreationRelationVisitor",
                classTypes[1].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[1].Metrics[0].ValueType);

            var dependencies = (IDictionary<string, int>)classTypes[1].Metrics[0].Value;

            Assert.Equal(1, dependencies.Count);
            Assert.Equal(8, dependencies["App.C"]);
        }

        [Fact]
        public void Extract_ShouldHaveOArrayCreation_WhenProvidedWithClassInTheSameNamespace()
        {
            const string fileContent = @"using System;
                                     namespace App
                                     {                
                                        class C { }

                                        class MyClass
                                        {
                                            private C[] _c = new C[] { };
                                            private C[] _c2 = { };

                                            public C[] MyC { get; set; } = new[] {new C(), new C()};
                                            public C[] MyC3 { get; set; } = {new C(), new C()};

                                            public C[] ComputedC => new[]
                                            {
                                                new C()
                                            };

                                            public C[] MyC2
                                            {
                                                get { return new C[] {new C()}; }
                                            }

                                            public MyClass()
                                            {
                                                var cs = new C[2] {new C(), new C()};
                                                C[] c = {new C()};
                                            }

                                            public C[] Method()
                                            {
                                                var c = new C[]{new C()};
                                                return c;
                                            }
                                        }

                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[1].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ObjectCreationRelationVisitor",
                classTypes[1].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[1].Metrics[0].ValueType);

            var dependencies = (IDictionary<string, int>)classTypes[1].Metrics[0].Value;

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(8, dependencies["App.C[]"]);
            Assert.Equal(1, dependencies["App.C[2]"]);
            Assert.Equal(10, dependencies["App.C"]);
        }


        [Fact]
        public void Extract_ShouldHaveObjectCreation_WhenProvidedWithClassUnknownClass()
        {
            const string fileContent = @"using System;
                                     namespace App
                                     {                                       
                                         class MyClass
                                         {                                           
                                            private ExternClass _c = new ExternClass();
                                            private ExternClass _c2 = new();

                                            public ExternClass MyC {get;set;} = new ExternClass();
                                            public ExternClass ComputedC => new();
                                            public ExternClass MyC2
                                            {
                                                get { return new ExternClass(); }
                                            }

                                            public MyClass() {
                                                new ExternClass();
                                                ExternClass c = new ExternClass();
                                            }

                                            public ExternClass Method() {
                                                var c = new ExternClass();
                                                return c;
                                            }
                                         }
                                     }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ObjectCreationRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Single(dependencies);
            Assert.Equal(8, dependencies["ExternClass"]);
        }

        [Fact]
        public void Extract_ShouldHaveArrayCreation_WhenProvidedWithClassUnknownClass()
        {
            const string fileContent = @"using System;
                                     namespace App
                                     {                                       
                                        class MyClass
                                        {
                                            private ExternClass[] _c = new ExternClass[] { };
                                            private ExternClass[] _c2 = { };

                                            public ExternClass[] MyC { get; set; } = new[] {new ExternClass(), new ExternClass()};
                                            public ExternClass[] MyC3 { get; set; } = {new ExternClass(), new ExternClass()};

                                            public ExternClass[] ComputedC => new[]
                                            {
                                                new ExternClass()
                                            };

                                            public ExternClass[] MyC2
                                            {
                                                get { return new ExternClass[] {new ExternClass()}; }
                                            }

                                            public MyClass()
                                            {
                                                var cs = new ExternClass[2] {new ExternClass(), new ExternClass()};
                                                ExternClass[] c = {new ExternClass()};
                                            }

                                            public ExternClass[] Method()
                                            {
                                                var c = new ExternClass[]{new ExternClass()};
                                                return c;
                                            }
                                        }
                                    }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ObjectCreationRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(8, dependencies["ExternClass[]"]);
            Assert.Equal(1, dependencies["ExternClass[2]"]);
            Assert.Equal(10, dependencies["ExternClass"]);
        }

        [Fact]
        public void Extract_ShouldHaveArrayCreation_WhenProvidedWithClassPrimitiveTypes()
        {
            const string fileContent = @"using System;
                                     namespace App
                                     {                                       
                                        class MyClass
                                        {
                                            private string[] _c = new string[] { };
                                            private int[] _c2 = { };

                                            public string[] MyC { get; set; } = new[] {""value"", ""other""};
                                            public int[] MyC3 { get; set; } = {2, 51};

                                            public double[] ComputedC => new[]
                                            {
                                                2.0
                                            };

                                            public string[] MyC2
                                            {
                                                get { return new string[] {""Hallo""}; }
                                            }

                                            public MyClass()
                                            {
                                                var cs = new int[2] {6,12};
                                                double[] c = {2.0};
                                            }

                                            public bool[] Method()
                                            {
                                                var c = new bool[]{false};
                                                return c;
                                            }
                                        }
                                    }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ObjectCreationRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(5, dependencies.Count);
            Assert.Equal(3, dependencies["string[]"]);
            Assert.Equal(2, dependencies["int[]"]);
            Assert.Equal(1, dependencies["int[2]"]);
            Assert.Equal(2, dependencies["double[]"]);
            Assert.Equal(1, dependencies["bool[]"]);
        }

        [Fact]
        public void Extract_ShouldHaveArrayCreation_WhenProvidedWithClassPrimitiveTypesInUnknownClassMethod()
        {
            const string fileContent = @"using System;
                                     namespace App
                                     {                            
                                        class MyClass
                                        {                                          
                                            public void Method(ExternClass c)
                                            {
                                                c.Call(new[] {""Value"", ""Other""});
                                                c.Call(new[] {2,6.1f, 6.1, 3}); // double
                                                c.Call(new[] {2,6.1f}); // float
                                                c.Call(new[] {2, 51}); // int
                                                c.Call(new[] {false, true});
                                                c.Call(new[] {""qwe"", true, 2});                                                  
                                            }
                                        }
                                    }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ObjectCreationRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(6, dependencies.Count);
            Assert.Equal(1, dependencies["System.String[]"]);
            Assert.Equal(1, dependencies["System.Double[]"]);
            Assert.Equal(1, dependencies["System.Single[]"]);
            Assert.Equal(1, dependencies["System.Int32[]"]);
            Assert.Equal(1, dependencies["System.Boolean[]"]);
            Assert.Equal(1, dependencies["System.Object[]"]);
        }

        [Fact]
        public void
            Extract_ShouldHaveArrayCreation_WhenProvidedWithArrayCreationOfLocalVariablesPropertiesFieldsAndMethodCallsInUnknownClassMethod()
        {
            const string fileContent = @"using System;
                                     namespace App
                                     {                            
                                        class MyClass
                                        {                                          
                                            private string _v ;
        
                                            public int Val { get; set; }
                                            
                                            public void Method(ExternClass c, bool b)
                                            {
                                                double d = 2.5;
                                                var vd = 5.1;
                                                
                                                c.Call(new[] {_v});
                                                c.Call(new[] {d, vd});
                                                c.Call(new[] {Val});
                                                c.Call(new[] {b, Method()});
                                            }

                                            public bool Method()
                                            {
                                                return false;
                                            }
                                        }
                                    }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ObjectCreationRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(4, dependencies.Count);
            Assert.Equal(1, dependencies["System.String[]"]);
            Assert.Equal(1, dependencies["System.Double[]"]);
            Assert.Equal(1, dependencies["System.Int32[]"]);
            Assert.Equal(1, dependencies["System.Boolean[]"]);
        }

        [Fact]
        public void Extract_ShouldHaveArrayCreation_WhenProvidedWithClassInSameNamespaceUsedWithUnknownClassMethod()
        {
            const string fileContent = @"using System;
                                     namespace App
                                     {                            
                                        class Class1{}
                                        class MyClass
                                        {                                          
                                            public void Method(ExternClass c)
                                            {
                                                c.Call(new[] {new Class1()});
                                                c.Call(new[] {new Class1(), new Class1()});
                                                c.Call(new[] {new Class1(), ""Text""});                                                                                                
                                            }
                                        }
                                    }";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes[1].Metrics.Count);
            Assert.Equal("HoneydewExtractors.Core.Metrics.Extraction.Class.Relations.ObjectCreationRelationVisitor",
                classTypes[1].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[1].Metrics[0].ValueType);

            var dependencies = (IDictionary<string, int>)classTypes[1].Metrics[0].Value;

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(4, dependencies["App.Class1"]);
            Assert.Equal(2, dependencies["App.Class1[]"]);
            Assert.Equal(1, dependencies["System.Object[]"]);
        }
    }
}
