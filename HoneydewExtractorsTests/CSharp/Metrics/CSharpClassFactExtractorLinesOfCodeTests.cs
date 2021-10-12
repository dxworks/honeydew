using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Property;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method.LocalFunctions;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics
{
    public class CSharpClassFactExtractorLinesOfCodeTests
    {
        private readonly CSharpFactExtractor _sut;
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
        private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

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
                    new LocalFunctionsSetterClassVisitor(new List<ICSharpLocalFunctionVisitor>
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
                    linesOfCodeVisitor,
                    new MethodAccessorSetterPropertyVisitor(new List<IMethodVisitor>
                    {
                        linesOfCodeVisitor
                    })
                })
            }));

            compositeVisitor.Add(linesOfCodeVisitor);

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _sut = new CSharpFactExtractor(compositeVisitor);
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/CSharpLinesOfCode/ClassWithCommentsWithPropertyAndMethod.txt")]
        public void Extract_ShouldHaveLinesOfCode_WhenProvidedWithClassWithMethodsAndProperties(string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);

            var compilationUnit = _sut.Extract(syntaxTree, semanticModel);

            var classModels = compilationUnit.ClassTypes;

            Assert.Equal(11, compilationUnit.Loc.SourceLines);
            Assert.Equal(18, compilationUnit.Loc.EmptyLines);
            Assert.Equal(8, compilationUnit.Loc.CommentedLines);

            var classModel = (ClassModel)classModels[0];
            Assert.Equal(8, classModel.Loc.SourceLines);
            Assert.Equal(12, classModel.Loc.EmptyLines);
            Assert.Equal(5, classModel.Loc.CommentedLines);

            Assert.Equal(3, classModel.Methods[0].Loc.SourceLines);
            Assert.Equal(2, classModel.Methods[0].Loc.CommentedLines);
            Assert.Equal(2, classModel.Methods[0].Loc.EmptyLines);

            Assert.Equal(3, classModel.Properties[0].Loc.SourceLines);
            Assert.Equal(1, classModel.Properties[0].Loc.CommentedLines);
            Assert.Equal(5, classModel.Properties[0].Loc.EmptyLines);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/CSharpLinesOfCode/ClassWithPropertyAndMethodAndDelegateWithComments.txt")]
        public void Extract_ShouldHaveLinesOfCode_WhenProvidedWithClassAndDelegate(string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);

            var compilationUnit = _sut.Extract(syntaxTree, semanticModel);

            var classModels = compilationUnit.ClassTypes;

            Assert.Equal(12, compilationUnit.Loc.SourceLines);
            Assert.Equal(19, compilationUnit.Loc.EmptyLines);
            Assert.Equal(8, compilationUnit.Loc.CommentedLines);

            var classModel = (ClassModel)classModels[0];
            Assert.Equal(8, classModel.Loc.SourceLines);
            Assert.Equal(14, classModel.Loc.EmptyLines);
            Assert.Equal(5, classModel.Loc.CommentedLines);

            Assert.Equal(3, classModel.Methods[0].Loc.SourceLines);
            Assert.Equal(2, classModel.Methods[0].Loc.CommentedLines);
            Assert.Equal(4, classModel.Methods[0].Loc.EmptyLines);

            Assert.Equal(3, classModel.Properties[0].Loc.SourceLines);
            Assert.Equal(1, classModel.Properties[0].Loc.CommentedLines);
            Assert.Equal(5, classModel.Properties[0].Loc.EmptyLines);
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/CSharpLinesOfCode/LocalFunctionWithComments.txt")]
        public void Extract_ShouldHaveLinesOfCode_WhenMethodWithLocalFunction(string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);

            var compilationUnit = _sut.Extract(syntaxTree, semanticModel);

            var classTypes = compilationUnit.ClassTypes;

            var localFunction = ((MethodModel)((ClassModel)classTypes[0]).Methods[0]).LocalFunctions[0];

            Assert.Equal(2, localFunction.Loc.SourceLines);
            Assert.Equal(1, localFunction.Loc.CommentedLines);
            Assert.Equal(3, localFunction.Loc.EmptyLines);
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/CSharpLinesOfCode/ClassWithCommentsWithPropertyAndMethod.txt")]
        public void Extract_ShouldHaveLinesOfCode_WhenGivenPropertyWithGetAccessor(string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);

            var compilationUnit = _sut.Extract(syntaxTree, semanticModel);

            var classTypes = compilationUnit.ClassTypes;

            var accessor = ((ClassModel)classTypes[0]).Properties[0].Accessors[0];

            Assert.Equal(2, accessor.Loc.SourceLines);
            Assert.Equal(1, accessor.Loc.CommentedLines);
            Assert.Equal(3, accessor.Loc.EmptyLines);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/ClassLevel/CSharpPropertiesInfo/ClassWithEventPropertyThatCallsMethodFromExternClass.txt")]
        public void Extract_ShouldHaveLinesOfCode_WhenGivenEventPropertyWithGetAccessor(string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);

            var compilationUnit = _sut.Extract(syntaxTree, semanticModel);

            var classTypes = compilationUnit.ClassTypes;

            var addAccessor = ((ClassModel)classTypes[0]).Properties[0].Accessors[0];
            Assert.Equal(2, addAccessor.Loc.SourceLines);
            Assert.Equal(0, addAccessor.Loc.CommentedLines);
            Assert.Equal(2, addAccessor.Loc.EmptyLines);

            var removeAccessor = ((ClassModel)classTypes[0]).Properties[0].Accessors[1];
            Assert.Equal(1, removeAccessor.Loc.SourceLines);
            Assert.Equal(0, removeAccessor.Loc.CommentedLines);
            Assert.Equal(0, removeAccessor.Loc.EmptyLines);
        }
    }
}
