using System.Collections.Generic;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.AccessedFields;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.AccessField;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Method;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method.LocalFunctions;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.AccessedFields
{
    public class LocalFunctionsAccessedFieldsTests
    {
        private readonly CSharpFactExtractor _factExtractor;
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
        private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

        public LocalFunctionsAccessedFieldsTests()
        {
            var compositeVisitor = new CompositeVisitor();

            var accessedFieldsSetterVisitor = new AccessedFieldsSetterVisitor(new List<ICSharpAccessedFieldsVisitor>
            {
                new AccessFieldVisitor()
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
                            accessedFieldsSetterVisitor
                        }),
                        accessedFieldsSetterVisitor
                    }),
                })
            }));

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _factExtractor = new CSharpFactExtractor(compositeVisitor);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/AccessedFields/LocalFunctionsAccessedFields/LocalFunctionsAccessedNonStaticFieldAndPropertyFromClass.txt")]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/AccessedFields/LocalFunctionsAccessedFields/LocalFunctionsAccessedStaticFieldAndPropertyFromClass.txt")]
        public void
            Extract_ShouldHaveAccessedFields_WhenGivenPropertyAccessorThatAccessesFieldsAndPropertiesFromOtherClass(
                string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            var modelMethod1 = (MethodModel)classModel.Methods[0];
            var modelMethod2 = (MethodModel)classModel.Methods[1];

            var localFunctions = new[]
            {
                modelMethod1.LocalFunctions[0],
                modelMethod1.LocalFunctions[0].LocalFunctions[0],
                modelMethod1.LocalFunctions[0].LocalFunctions[0].LocalFunctions[0],

                modelMethod2.LocalFunctions[0],
                modelMethod2.LocalFunctions[0].LocalFunctions[0],
                modelMethod2.LocalFunctions[0].LocalFunctions[0].LocalFunctions[0],
            };

            foreach (var localFunction in localFunctions)
            {
                Assert.Equal(2, localFunction.AccessedFields.Count);

                Assert.Equal("Field1", localFunction.AccessedFields[0].Name);
                Assert.Equal("Property1", localFunction.AccessedFields[1].Name);

                foreach (var accessedField in localFunction.AccessedFields)
                {
                    Assert.Equal(classModel.Name, accessedField.ContainingTypeName);
                }
            }

            for (var i = 0; i < 3; i++)
            {
                Assert.Equal(AccessedField.AccessKind.Getter, localFunctions[i].AccessedFields[0].Kind);
                Assert.Equal(AccessedField.AccessKind.Getter, localFunctions[i].AccessedFields[1].Kind);
            }

            for (var i = 3; i < localFunctions.Length; i++)
            {
                Assert.Equal(AccessedField.AccessKind.Setter, localFunctions[i].AccessedFields[0].Kind);
                Assert.Equal(AccessedField.AccessKind.Setter, localFunctions[i].AccessedFields[1].Kind);
            }
        }
    }
}
