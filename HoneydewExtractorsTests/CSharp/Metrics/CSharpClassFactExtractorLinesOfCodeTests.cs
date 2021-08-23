using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Extraction.Class;
using HoneydewExtractors.Core.Metrics.Extraction.Common;
using HoneydewExtractors.Core.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method.LocalFunctions;
using HoneydewModels.CSharp;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics
{
    public class CSharpClassFactExtractorLinesOfCodeTests
    {
        private readonly CSharpFactExtractor _sut;

        public CSharpClassFactExtractorLinesOfCodeTests()
        {
            var linesOfCodeVisitor = new LinesOfCodeVisitor();
            var compositeVisitor = new CompositeVisitor();

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                linesOfCodeVisitor,
                new ConstructorSetterClassVisitor(new List<ICSharpConstructorVisitor>
                {
                    linesOfCodeVisitor
                }),
                new MethodSetterClassVisitor(new List<ICSharpMethodVisitor>
                {
                    linesOfCodeVisitor,
                    new LocalFunctionsSetterClassVisitor(
                        new List<ICSharpLocalFunctionVisitor>
                        {
                            linesOfCodeVisitor,
                            new LocalFunctionInfoVisitor(new List<ILocalFunctionVisitor>
                            {
                                linesOfCodeVisitor
                            }),
                        })
                }),
                new PropertySetterClassVisitor(new List<ICSharpPropertyVisitor>
                {
                    linesOfCodeVisitor
                })
            }));

            compositeVisitor.Add(linesOfCodeVisitor);

            _sut = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker()), compositeVisitor);
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/CSharpLinesOfCode/ClassWithCommentsWithPropertyAndMethod.txt")]
        public void Extract_ShouldHaveLinesOfCode_WhenProvidedWithClassWithMethodsAndProperties(string fileContent)
        {
            var compilationUnit = _sut.Extract(fileContent);

            var classModels = compilationUnit.ClassTypes;

            Assert.Equal(21, compilationUnit.Loc.SourceLines);
            Assert.Equal(10, compilationUnit.Loc.EmptyLines);
            Assert.Equal(8, compilationUnit.Loc.CommentedLines);

            var classModel = (ClassModel)classModels[0];
            Assert.Equal(16, classModel.Loc.SourceLines);
            Assert.Equal(6, classModel.Loc.EmptyLines);
            Assert.Equal(5, classModel.Loc.CommentedLines);

            Assert.Equal(5, classModel.Methods[0].Loc.SourceLines);
            Assert.Equal(2, classModel.Methods[0].Loc.CommentedLines);
            Assert.Equal(2, classModel.Methods[0].Loc.EmptyLines);

            Assert.Equal(7, classModel.Properties[0].Loc.SourceLines);
            Assert.Equal(1, classModel.Properties[0].Loc.CommentedLines);
            Assert.Equal(1, classModel.Properties[0].Loc.EmptyLines);
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/CSharpLinesOfCode/ClassWithPropertyAndMethodAndDelegateWithComments.txt")]
        public void Extract_ShouldHaveLinesOfCode_WhenProvidedWithClassAndDelegate(string fileContent)
        {
            var compilationUnit = _sut.Extract(fileContent);

            var classModels = compilationUnit.ClassTypes;

            Assert.Equal(22, compilationUnit.Loc.SourceLines);
            Assert.Equal(9, compilationUnit.Loc.EmptyLines);
            Assert.Equal(8, compilationUnit.Loc.CommentedLines);

            var classModel = (ClassModel)classModels[0];
            Assert.Equal(16, classModel.Loc.SourceLines);
            Assert.Equal(6, classModel.Loc.EmptyLines);
            Assert.Equal(5, classModel.Loc.CommentedLines);

            Assert.Equal(5, classModel.Methods[0].Loc.SourceLines);
            Assert.Equal(2, classModel.Methods[0].Loc.CommentedLines);
            Assert.Equal(2, classModel.Methods[0].Loc.EmptyLines);

            Assert.Equal(7, classModel.Properties[0].Loc.SourceLines);
            Assert.Equal(1, classModel.Properties[0].Loc.CommentedLines);
            Assert.Equal(1, classModel.Properties[0].Loc.EmptyLines);
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/CSharpLinesOfCode/LocalFunctionWithComments.txt")]
        public void Extract_ShouldHaveLinesOfCode_WhenMethodWithLocalFunction(string fileContent)
        {
            var compilationUnit = _sut.Extract(fileContent);

            var classTypes = compilationUnit.ClassTypes;

            var localFunction = ((MethodModel)((ClassModel)classTypes[0]).Methods[0]).LocalFunctions[0];

            Assert.Equal(4, localFunction.Loc.SourceLines);
            Assert.Equal(1, localFunction.Loc.CommentedLines);
            Assert.Equal(1, localFunction.Loc.EmptyLines);
        }
    }
}
