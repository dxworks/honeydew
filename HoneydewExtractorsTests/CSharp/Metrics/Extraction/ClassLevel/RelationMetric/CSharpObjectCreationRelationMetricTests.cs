using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel.RelationMetric
{
    public class CSharpObjectCreationRelationMetricTests
    {
        private readonly CSharpObjectCreationRelationMetric _sut;
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpObjectCreationRelationMetricTests()
        {
            _sut = new CSharpObjectCreationRelationMetric();
            _factExtractor = new CSharpFactExtractor();
            _factExtractor.AddMetric<CSharpObjectCreationRelationMetric>();
        }

        [Fact]
        public void GetMetricType_ShouldReturnClassLevel()
        {
            Assert.Equal(ExtractionMetricType.ClassLevel, _sut.GetMetricType());
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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[1].GetMetricValue<CSharpObjectCreationRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[1].GetMetricValue<CSharpObjectCreationRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(2, dependencies.Count);
            Assert.Equal(6, dependencies["App.C[]"]);
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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpObjectCreationRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(1, dependencies.Count);
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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpObjectCreationRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(1, dependencies.Count);
            Assert.Equal(9, dependencies["ExternClass[]"]);
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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpObjectCreationRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(4, dependencies.Count);
            Assert.Equal(3, dependencies["string[]"]);
            Assert.Equal(1, dependencies["int[]"]);
            Assert.Equal(1, dependencies["double[]"]);
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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpObjectCreationRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[0].GetMetricValue<CSharpObjectCreationRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

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

            var classModels = _factExtractor.Extract(fileContent);

            var optional = classModels[1].GetMetricValue<CSharpObjectCreationRelationMetric>();
            Assert.True(optional.HasValue);

            var dependencies = (IDictionary<string, int>) optional.Value;

            Assert.Equal(2, dependencies.Count);
            Assert.Equal(2, dependencies["App.Class1[]"]);
            Assert.Equal(1, dependencies["System.Object[]"]);
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
            Assert.Equal(typeof(CSharpObjectCreationRelationMetric).FullName, fileRelation1.RelationType);
            Assert.Equal(2, fileRelation1.RelationCount);

            var fileRelation2 = fileRelations[1];
            Assert.Equal("CSharpMetricExtractor", fileRelation2.FileTarget);
            Assert.Equal(typeof(CSharpObjectCreationRelationMetric).FullName, fileRelation2.RelationType);
            Assert.Equal(1, fileRelation2.RelationCount);
        }
    }
}
