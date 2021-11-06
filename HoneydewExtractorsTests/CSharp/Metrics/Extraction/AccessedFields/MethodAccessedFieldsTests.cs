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
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.AccessedFields
{
    public class MethodAccessedFieldsTests
    {
        private readonly CSharpFactExtractor _factExtractor;
        private readonly Mock<ILogger> _loggerMock = new();
        private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
        private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

        public MethodAccessedFieldsTests()
        {
            var compositeVisitor = new CompositeVisitor();

            var accessedFieldsSetterVisitor = new AccessedFieldsSetterVisitor(new List<ICSharpAccessedFieldsVisitor>
            {
                new AccessFieldVisitor()
            });

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new MethodSetterClassVisitor(new List<ICSharpMethodVisitor>
                {
                    new MethodInfoVisitor(),
                    accessedFieldsSetterVisitor
                }),
            }));

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _factExtractor = new CSharpFactExtractor(compositeVisitor);
        }


        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/AccessedFields/MethodAccessedFields/MethodAccessedNonStaticFieldAndPropertyFromClass.txt")]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/AccessedFields/MethodAccessedFields/MethodAccessedStaticFieldAndPropertyFromClass.txt")]
        public void Extract_ShouldHaveAccessedFields_WhenGivenMethodThatAccessesFieldsAndPropertiesFromInsideTheClass(
            string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            foreach (var methodType in classModel.Methods)
            {
                Assert.Equal(2, methodType.AccessedFields.Count);

                Assert.Equal("Field1", methodType.AccessedFields[0].Name);
                Assert.Equal("Property1", methodType.AccessedFields[1].Name);

                foreach (var accessedField in methodType.AccessedFields)
                {
                    Assert.Equal(classModel.Name, accessedField.ContainingTypeName);
                }
            }

            foreach (var accessedField in classModel.Methods[0].AccessedFields)
            {
                Assert.Equal(AccessedField.AccessKind.Getter, accessedField.Kind);
            }

            foreach (var accessedField in classModel.Methods[1].AccessedFields)
            {
                Assert.Equal(AccessedField.AccessKind.Setter, accessedField.Kind);
            }
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/AccessedFields/MethodAccessedFields/MethodAccessedNonStaticFieldAndPropertyFromOtherClass.txt")]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/AccessedFields/MethodAccessedFields/MethodAccessedStaticFieldAndPropertyFromOtherClass.txt")]
        public void Extract_ShouldHaveAccessedFields_WhenGivenMethodThatAccessesFieldsAndPropertiesFromOtherClass(
            string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            foreach (var methodType in classModel.Methods)
            {
                Assert.Equal(2, methodType.AccessedFields.Count);

                Assert.Equal("Field1", methodType.AccessedFields[0].Name);
                Assert.Equal("Property1", methodType.AccessedFields[1].Name);

                foreach (var accessedField in methodType.AccessedFields)
                {
                    Assert.Equal(classTypes[1].Name, accessedField.ContainingTypeName);
                }
            }

            foreach (var accessedField in classModel.Methods[0].AccessedFields)
            {
                Assert.Equal(AccessedField.AccessKind.Getter, accessedField.Kind);
            }

            foreach (var accessedField in classModel.Methods[1].AccessedFields)
            {
                Assert.Equal(AccessedField.AccessKind.Setter, accessedField.Kind);
            }
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/AccessedFields/MethodAccessedFields/MethodAccessedNonStaticFieldAndPropertyFromExternClass.txt")]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/AccessedFields/MethodAccessedFields/MethodAccessedStaticFieldAndPropertyFromExternClass.txt")]
        public void Extract_ShouldHaveAccessedFields_WhenGivenMethodThatAccessesFieldsAndPropertiesFromExternClass(
            string fileContent)
        {
            var syntaxTree = _syntacticModelCreator.Create(fileContent);
            var semanticModel = _semanticModelCreator.Create(syntaxTree);
            var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

            var classModel = (ClassModel)classTypes[0];
            foreach (var methodType in classModel.Methods)
            {
                Assert.Equal(2, methodType.AccessedFields.Count);

                Assert.Equal("Field1", methodType.AccessedFields[0].Name);
                Assert.Equal("Property1", methodType.AccessedFields[1].Name);

                foreach (var accessedField in methodType.AccessedFields)
                {
                    Assert.Equal("ExternClass", accessedField.ContainingTypeName);
                }
            }

            foreach (var accessedField in classModel.Methods[0].AccessedFields)
            {
                Assert.Equal(AccessedField.AccessKind.Getter, accessedField.Kind);
            }

            foreach (var accessedField in classModel.Methods[1].AccessedFields)
            {
                Assert.Equal(AccessedField.AccessKind.Setter, accessedField.Kind);
            }
        }
    }
}
