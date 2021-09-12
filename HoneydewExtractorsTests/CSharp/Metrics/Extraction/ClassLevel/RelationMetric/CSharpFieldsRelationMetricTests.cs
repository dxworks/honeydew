﻿using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Fields;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Field;
using HoneydewExtractors.CSharp.Metrics.Iterators;
using HoneydewModels.Types;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel.RelationMetric
{
    public class CSharpFieldsRelationVisitorTests
    {
        private readonly FieldsRelationVisitor _sut;
        private readonly CSharpFactExtractor _factExtractor;
        private readonly ClassTypePropertyIterator _classTypePropertyIterator;
        private readonly Mock<ILogger> _loggerMock = new();

        public CSharpFieldsRelationVisitorTests()
        {
            _sut = new FieldsRelationVisitor();

            var compositeVisitor = new CompositeVisitor();

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new FieldSetterClassVisitor(new List<IFieldVisitor>
                {
                    new FieldInfoVisitor()
                })
            }));

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker(_loggerMock.Object)), compositeVisitor);

            _classTypePropertyIterator = new ClassTypePropertyIterator(new List<IModelVisitor<IClassType>>
            {
                _sut
            });
        }

        [Fact]
        public void PrettyPrint_ShouldReturnReturnValueDependency()
        {
            Assert.Equal("Fields Dependency", _sut.PrettyPrint());
        }

        [Theory]
        [InlineData("class")]
        [InlineData("record")]
        [InlineData("struct")]
        public void Extract_ShouldHavePrimitiveFields_WhenClassHasFieldsOfPrimitiveTypes(string classType)
        {
            var fileContent = $@"using System;

                                     namespace App
                                     {{                                       
                                         {classType} MyClass
                                         {{                                           
                                             public int Foo;

                                             private float Bar;

                                             protected int Zoo;

                                             internal string Goo;
                                         }}
                                     }}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            foreach (var model in classTypes)
            {
                _classTypePropertyIterator.Iterate(model);
            }

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.FieldsRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(2, dependencies["int"]);
            Assert.Equal(1, dependencies["float"]);
            Assert.Equal(1, dependencies["string"]);
        }

        [Theory]
        [InlineData("class")]
        [InlineData("record")]
        [InlineData("struct")]
        public void Extract_ShouldHavePrimitiveFields_WhenClassHasEventFieldsOfPrimitiveTypes(string classType)
        {
            var fileContent = $@"using System;
                                     namespace App
                                     {{                                       
                                        {classType} MyClass
                                        {{
                                            public event Func<int> Foo;
                                            
                                            public event Action<string> Bar;
                                        }}
                                     }}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            foreach (var model in classTypes)
            {
                _classTypePropertyIterator.Iterate(model);
            }

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.FieldsRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(2, dependencies.Count);
            Assert.Equal(1, dependencies["System.Func<int>"]);
            Assert.Equal(1, dependencies["System.Action<string>"]);
        }

        [Theory]
        [InlineData("class")]
        [InlineData("record")]
        [InlineData("struct")]
        public void Extract_ShouldHaveDependenciesFields_WhenClassHasFields(string classType)
        {
            var fileContent = $@"using System;
                                     using HoneydewCore.Extractors;
                                     using HoneydewCore.Extractors.Metrics;
                                     using HoneydewCore.Extractors.Metrics.SemanticMetrics;
                                     namespace App
                                     {{                                       
                                         public {classType} IInterface
                                         {{                                           
                                             public CSharpMetricExtractor Foo;

                                             private CSharpMetricExtractor Foo2 ;

                                             protected IFactExtractor Bar;
                                         }}
                                     }}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            foreach (var model in classTypes)
            {
                _classTypePropertyIterator.Iterate(model);
            }

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.FieldsRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(2, dependencies.Count);
            Assert.Equal(2, dependencies["CSharpMetricExtractor"]);
            Assert.Equal(1, dependencies["IFactExtractor"]);
        }

        [Theory]
        [InlineData("class")]
        [InlineData("record")]
        [InlineData("struct")]
        public void Extract_ShouldHaveDependenciesEventFields_WhenClassHasEventFields(string classType)
        {
            var fileContent = $@"using System;
                                     using HoneydewCore.Extractors;
                                     using HoneydewCore.Extractors.Metrics;
                                     namespace App
                                     {{                                       
                                         public {classType} IInterface
                                         {{                                           
                                             internal event Func<CSharpMetricExtractor> Foo;

                                             public event Action<IFactExtractor> Bar;

                                             private event Func<IFactExtractor,CSharpMetricExtractor> Goo;
                                         }}
                                     }}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            foreach (var model in classTypes)
            {
                _classTypePropertyIterator.Iterate(model);
            }

            Assert.Equal(1, classTypes[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations.FieldsRelationVisitor",
                classTypes[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Collections.Generic.Dictionary`2[System.String,System.Int32]",
                classTypes[0].Metrics[0].ValueType);

            var dependencies = (Dictionary<string, int>)classTypes[0].Metrics[0].Value;

            Assert.Equal(3, dependencies.Count);
            Assert.Equal(1, dependencies["System.Func<CSharpMetricExtractor>"]);
            Assert.Equal(1, dependencies["System.Action<IFactExtractor>"]);
            Assert.Equal(1, dependencies["System.Func<IFactExtractor, CSharpMetricExtractor>"]);
        }
    }
}
