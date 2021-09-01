using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.LocalVariables;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.LocalVariables;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewModels.CSharp;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.Method
{
    public class CSharpMethodLocalVariablesTests
    {
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpMethodLocalVariablesTests()
        {
            var compositeVisitor = new CompositeVisitor();

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<IClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new MethodSetterClassVisitor(new List<ICSharpMethodVisitor>
                {
                    new MethodInfoVisitor(),
                    new LocalVariablesTypeSetterVisitor(new List<ILocalVariablesVisitor>
                    {
                        new LocalVariableInfoVisitor()
                    })
                })
            }));

            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker()), compositeVisitor);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/LocalVariables/MethodWithPrimitiveLocalVariables.txt")]
        public void Extract_ShouldExtractLocalVariables_WhenProvidedWithPrimitiveTypes(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(2, classModel.Methods.Count);

            foreach (var methodType in classModel.Methods)
            {
                Assert.Equal(4, methodType.LocalVariableTypes.Count);
                Assert.Equal("int", methodType.LocalVariableTypes[0].Name);
                Assert.Equal("int", methodType.LocalVariableTypes[1].Name);
                Assert.Equal("int", methodType.LocalVariableTypes[2].Name);
                Assert.Equal("string", methodType.LocalVariableTypes[3].Name);
            }
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/LocalVariables/MethodWithCustomClassLocalVariables.txt")]
        public void Extract_ShouldExtractLocalVariables_WhenProvidedWithCustomClassTypes(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(2, classModel.Methods.Count);

            foreach (var methodType in classModel.Methods)
            {
                Assert.Equal(5, methodType.LocalVariableTypes.Count);
                Assert.Equal("Namespace1.Parent", methodType.LocalVariableTypes[0].Name);
                Assert.Equal("Namespace1.Class2", methodType.LocalVariableTypes[3].Name);
                Assert.Equal("Namespace1.Class3", methodType.LocalVariableTypes[4].Name);
            }

            Assert.Equal("Namespace1.Parent", classModel.Methods[0].LocalVariableTypes[1].Name);
            Assert.Equal("Namespace1.Parent", classModel.Methods[0].LocalVariableTypes[2].Name);

            Assert.Equal("Namespace1.Class2", classModel.Methods[1].LocalVariableTypes[1].Name);
            Assert.Equal("Namespace1.Class3", classModel.Methods[1].LocalVariableTypes[2].Name);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/LocalVariables/MethodWithExternClassLocalVariables.txt")]
        public void Extract_ShouldExtractLocalVariables_WhenProvidedWithExternClassTypes(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(2, classModel.Methods.Count);

            foreach (var methodType in classModel.Methods)
            {
                Assert.Equal(3, methodType.LocalVariableTypes.Count);
                foreach (var localVariableType in methodType.LocalVariableTypes)
                {
                    Assert.Equal("ExternClass", localVariableType.Name);
                }
            }
        }
        
        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/LocalVariables/MethodWithArrayLocalVariable.txt")]
        public void Extract_ShouldExtractLocalVariables_WhenProvidedWithArrayLocalVariable(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(2, classModel.Methods.Count);

            foreach (var methodType in classModel.Methods)
            {
                Assert.Equal(3, methodType.LocalVariableTypes.Count);
                Assert.Equal("int[]", methodType.LocalVariableTypes[0].Name);
                Assert.Equal("Namespace1.Class2[]", methodType.LocalVariableTypes[1].Name);
                Assert.Equal("ExternClass[]", methodType.LocalVariableTypes[2].Name);
            }
        }
    }
}
