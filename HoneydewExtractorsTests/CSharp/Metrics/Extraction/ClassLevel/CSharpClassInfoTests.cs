using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Extraction.Parameter;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpClassInfoTests
    {
        private readonly CSharpFactExtractor _factExtractor;
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
        private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

        public CSharpClassInfoTests()
        {
            var compositeVisitor = new CompositeVisitor();

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new MethodSetterClassVisitor(new List<ICSharpMethodVisitor>
                {
                    new MethodInfoVisitor(),
                    new ParameterSetterVisitor(new List<IParameterVisitor>
                    {
                        new ParameterInfoVisitor()
                    })
                })
            }));

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _factExtractor = new CSharpFactExtractor(compositeVisitor);
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/ClassLevel/ClassInfo/InterfaceWithImplementedMethods.txt")]
        public void Extract_ShouldHaveMethods_WhenProvidedWithInterfaceWithImplementedMethods(string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal("Namespace1.IInterface", classModel.Name);

            Assert.Equal(2, classModel.Methods.Count);
            Assert.Equal("Method1", classModel.Methods[0].Name);
            Assert.Equal("void", classModel.Methods[0].ReturnValue.Type.Name);
            Assert.Equal(1, classModel.Methods[0].ParameterTypes.Count);
            Assert.Equal("int", classModel.Methods[0].ParameterTypes[0].Type.Name);

            Assert.Equal("Method2", classModel.Methods[1].Name);
            Assert.Equal("int", classModel.Methods[1].ReturnValue.Type.Name);
            Assert.Equal(2, classModel.Methods[1].ParameterTypes.Count);
            Assert.Equal("string", classModel.Methods[1].ParameterTypes[0].Type.Name);
            Assert.Equal("string", classModel.Methods[1].ParameterTypes[1].Type.Name);
        }
    }
}
