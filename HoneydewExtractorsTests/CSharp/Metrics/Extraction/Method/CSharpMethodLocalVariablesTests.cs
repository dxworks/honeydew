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
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method.LocalFunctions;
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

            var localVariablesTypeSetterVisitor = new LocalVariablesTypeSetterVisitor(new List<ILocalVariablesVisitor>
            {
                new LocalVariableInfoVisitor()
            });

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<IClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new MethodSetterClassVisitor(new List<ICSharpMethodVisitor>
                {
                    new MethodInfoVisitor(),
                    new LocalFunctionsSetterClassVisitor(new List<ILocalFunctionVisitor>
                    {
                        new LocalFunctionInfoVisitor(new List<ILocalFunctionVisitor>
                        {
                            localVariablesTypeSetterVisitor,
                        }),
                        localVariablesTypeSetterVisitor,
                    }),
                    localVariablesTypeSetterVisitor
                }),
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
                Assert.Equal("int", methodType.LocalVariableTypes[0].Type.Name);
                Assert.Equal("int", methodType.LocalVariableTypes[1].Type.Name);
                Assert.Equal("int", methodType.LocalVariableTypes[2].Type.Name);
                Assert.Equal("string", methodType.LocalVariableTypes[3].Type.Name);
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
                Assert.Equal("Namespace1.Parent", methodType.LocalVariableTypes[0].Type.Name);
                Assert.Equal("Namespace1.Class2", methodType.LocalVariableTypes[3].Type.Name);
                Assert.Equal("Namespace1.Class3", methodType.LocalVariableTypes[4].Type.Name);
            }

            Assert.Equal("Namespace1.Parent", classModel.Methods[0].LocalVariableTypes[1].Type.Name);
            Assert.Equal("Namespace1.Parent", classModel.Methods[0].LocalVariableTypes[2].Type.Name);

            Assert.Equal("Namespace1.Class2", classModel.Methods[1].LocalVariableTypes[1].Type.Name);
            Assert.Equal("Namespace1.Class3", classModel.Methods[1].LocalVariableTypes[2].Type.Name);
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
                    Assert.Equal("ExternClass", localVariableType.Type.Name);
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
                Assert.Equal("int[]", methodType.LocalVariableTypes[0].Type.Name);
                Assert.Equal("Namespace1.Class2[]", methodType.LocalVariableTypes[1].Type.Name);
                Assert.Equal("ExternClass[]", methodType.LocalVariableTypes[2].Type.Name);
            }
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/LocalFunctions/LocalFunctionWithLocalVariables.txt")]
        public void Extract_ShouldExtractLocalVariables_WhenProvidedWithLocalFunctionWithLocalVariables(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            var method = (MethodModel)classModel.Methods[0];
            Assert.Equal(1, method.LocalFunctions.Count);

            foreach (var localFunction in method.LocalFunctions)
            {
                Assert.Equal(3, localFunction.LocalVariableTypes.Count);
                Assert.Equal("int", localFunction.LocalVariableTypes[0].Type.Name);
                Assert.Equal("Namespace1.Class2", localFunction.LocalVariableTypes[1].Type.Name);
                Assert.Equal("ExternClass", localFunction.LocalVariableTypes[2].Type.Name);

                Assert.Equal(1, localFunction.LocalFunctions.Count);
                foreach (var innerLocalFunction in localFunction.LocalFunctions)
                {
                    Assert.Equal(3, innerLocalFunction.LocalVariableTypes.Count);
                    Assert.Equal("int", innerLocalFunction.LocalVariableTypes[0].Type.Name);
                    Assert.Equal("Namespace1.Class2", innerLocalFunction.LocalVariableTypes[1].Type.Name);
                    Assert.Equal("ExternClass", innerLocalFunction.LocalVariableTypes[2].Type.Name);
                }
            }
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/LocalFunctions/LocalFunctionWithArrayLocalVariables.txt")]
        public void Extract_ShouldExtractLocalVariables_WhenProvidedWithLocalFunctionWithArrayLocalVariables(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            var method = (MethodModel)classModel.Methods[0];
            Assert.Equal(1, method.LocalFunctions.Count);

            foreach (var localFunction in method.LocalFunctions)
            {
                Assert.Equal(3, localFunction.LocalVariableTypes.Count);
                Assert.Equal("int[]", localFunction.LocalVariableTypes[0].Type.Name);
                Assert.Equal("Namespace1.Class2[]", localFunction.LocalVariableTypes[1].Type.Name);
                Assert.Equal("ExternClass[]", localFunction.LocalVariableTypes[2].Type.Name);

                Assert.Equal(1, localFunction.LocalFunctions.Count);
                foreach (var innerLocalFunction in localFunction.LocalFunctions)
                {
                    Assert.Equal(3, innerLocalFunction.LocalVariableTypes.Count);
                    Assert.Equal("int[]", innerLocalFunction.LocalVariableTypes[0].Type.Name);
                    Assert.Equal("Namespace1.Class2[]", innerLocalFunction.LocalVariableTypes[1].Type.Name);
                    Assert.Equal("ExternClass[]", innerLocalFunction.LocalVariableTypes[2].Type.Name);
                }
            }
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/LocalVariables/MethodWithLocalVariableFromIfAndSwitch.txt")]
        public void Extract_ShouldExtractLocalVariables_WhenProvidedWithLocalVariablesFromIfAndSwitch(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(2, classModel.Methods.Count);

            foreach (var methodType in classModel.Methods)
            {
                Assert.Equal(2, methodType.LocalVariableTypes.Count);
                Assert.Equal("Namespace1.Class2", methodType.LocalVariableTypes[0].Type.Name);
                Assert.Equal("Namespace1.Class3", methodType.LocalVariableTypes[1].Type.Name);
            }
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/LocalFunctions/LocalFunctionWithLocalVariableFromIfAndSwitch.txt")]
        public void Extract_ShouldExtractLocalVariables_WhenProvidedWithLocalFunctionsWithLocalVariablesFromIfAndSwitch(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            var method = (MethodModel)classModel.Methods[0];
            Assert.Equal(1, method.LocalFunctions.Count);

            foreach (var localFunction in method.LocalFunctions)
            {
                Assert.Equal(2, localFunction.LocalVariableTypes.Count);
                Assert.Equal("Namespace1.Class2", localFunction.LocalVariableTypes[0].Type.Name);
                Assert.Equal("Namespace1.Class3", localFunction.LocalVariableTypes[1].Type.Name);

                Assert.Equal(1, localFunction.LocalFunctions.Count);
                foreach (var innerLocalFunction in localFunction.LocalFunctions)
                {
                    Assert.Equal(2, innerLocalFunction.LocalVariableTypes.Count);
                    Assert.Equal("Namespace1.Class2", innerLocalFunction.LocalVariableTypes[0].Type.Name);
                    Assert.Equal("Namespace1.Class3", innerLocalFunction.LocalVariableTypes[1].Type.Name);
                }
            }
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/LocalVariables/MethodWithLocalVariableFromForeach.txt")]
        public void Extract_ShouldExtractLocalVariables_WhenProvidedWithLocalVariablesFromForeach(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            Assert.Equal(3, classModel.Methods.Count);

            foreach (var methodType in classModel.Methods)
            {
                Assert.Equal(2, methodType.LocalVariableTypes.Count);
            }

            Assert.Equal("Namespace1.Class2", classModel.Methods[0].LocalVariableTypes[0].Type.Name);
            Assert.Equal("Namespace1.Class2", classModel.Methods[0].LocalVariableTypes[1].Type.Name);

            Assert.Equal("int", classModel.Methods[1].LocalVariableTypes[0].Type.Name);
            Assert.Equal("int", classModel.Methods[1].LocalVariableTypes[1].Type.Name);

            Assert.Equal("ExternClass", classModel.Methods[2].LocalVariableTypes[0].Type.Name);
            Assert.Equal("ExternClass", classModel.Methods[2].LocalVariableTypes[1].Type.Name);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/Method/LocalFunctions/LocalFunctionWithLocalVariableFromForeach.txt")]
        public void Extract_ShouldExtractLocalVariables_WhenProvidedWithLocalFunctionsWithLocalVariablesFromForeach(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            foreach (var methodType in classModel.Methods)
            {
                var method = (MethodModel)methodType;
                Assert.Equal(1, method.LocalFunctions.Count);

                foreach (var localFunction in method.LocalFunctions)
                {
                    Assert.Equal(2, localFunction.LocalVariableTypes.Count);

                    Assert.Equal(1, localFunction.LocalFunctions.Count);
                    foreach (var innerLocalFunction in localFunction.LocalFunctions)
                    {
                        Assert.Equal(2, innerLocalFunction.LocalVariableTypes.Count);
                    }
                }
            }


            var method0 = (MethodModel)classModel.Methods[0];
            Assert.Equal("Namespace1.Class2", method0.LocalFunctions[0].LocalVariableTypes[0].Type.Name);
            Assert.Equal("Namespace1.Class2", method0.LocalFunctions[0].LocalVariableTypes[1].Type.Name);
            Assert.Equal("Namespace1.Class2",
                method0.LocalFunctions[0].LocalFunctions[0].LocalVariableTypes[0].Type.Name);
            Assert.Equal("Namespace1.Class2",
                method0.LocalFunctions[0].LocalFunctions[0].LocalVariableTypes[1].Type.Name);

            var method1 = (MethodModel)classModel.Methods[1];
            Assert.Equal("int", method1.LocalFunctions[0].LocalVariableTypes[0].Type.Name);
            Assert.Equal("int", method1.LocalFunctions[0].LocalVariableTypes[1].Type.Name);
            Assert.Equal("int", method1.LocalFunctions[0].LocalFunctions[0].LocalVariableTypes[0].Type.Name);
            Assert.Equal("int", method1.LocalFunctions[0].LocalFunctions[0].LocalVariableTypes[1].Type.Name);

            var method2 = (MethodModel)classModel.Methods[2];
            Assert.Equal("ExternClass", method2.LocalFunctions[0].LocalVariableTypes[0].Type.Name);
            Assert.Equal("ExternClass", method2.LocalFunctions[0].LocalVariableTypes[1].Type.Name);
            Assert.Equal("ExternClass", method2.LocalFunctions[0].LocalFunctions[0].LocalVariableTypes[0].Type.Name);
            Assert.Equal("ExternClass", method2.LocalFunctions[0].LocalFunctions[0].LocalVariableTypes[1].Type.Name);
        }
    }
}
