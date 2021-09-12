﻿using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.Common;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Delegate;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpClassImportsMetricTests
    {
        private readonly CSharpFactExtractor _factExtractor;
        private readonly Mock<ILogger> _loggerMock = new();

        public CSharpClassImportsMetricTests()
        {
            var compositeVisitor = new CompositeVisitor();

            var importsVisitor = new ImportsVisitor();
            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                importsVisitor
            }));
            compositeVisitor.Add(new DelegateSetterCompilationUnitVisitor(new List<ICSharpDelegateVisitor>
            {
                new BaseInfoDelegateVisitor(),
                importsVisitor
            }));

            compositeVisitor.Accept(new LoggerSetterVisitor(_loggerMock.Object));

            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker(_loggerMock.Object)), compositeVisitor);
        }

        [Theory]
        [InlineData("class")]
        [InlineData("record")]
        [InlineData("struct")]
        [InlineData("enum")]
        [InlineData("interface")]
        public void Extract_ShouldHaveUsings_WhenGivenOneClass(string classType)
        {
            var fileContent = $@"using System;
                                     using System.Collections.Generic;
                                     using System.Linq;
                                     using System.Text;
                                     using Microsoft.CodeAnalysis;
                                     using Microsoft.CodeAnalysis.CSharp;

                                     namespace TopLevel
                                     {{
                                         public {classType} Foo {{ }}                                        
                                     }}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var importTypes = classTypes[0].Imports;

            Assert.Equal(6, importTypes.Count);

            Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System"));
            Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
            Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System.Linq"));
            Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System.Text"));
            Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis"));
            Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis.CSharp"));
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/CSharpImports/CompilationUnitWithOneDelegate.txt")]
        public void Extract_ShouldHaveUsings_WhenGivenOneDelegate(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(1, classTypes.Count);

            var importTypes = classTypes[0].Imports;

            Assert.Equal(6, importTypes.Count);

            Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System"));
            Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
            Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System.Linq"));
            Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System.Text"));
            Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis"));
            Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis.CSharp"));
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/CSharpImports/CompilationUnitWithClassRecordStructInterfaceAndDelegate.txt")]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/CSharpImports/CompilationUnitWithClassRecordStructInterfaceAndDelegate_DeclaredInside.txt")]
        public void Extract_ShouldHaveUsings_WhenGivenMultipleClassesAndDelegatesWithTheSameUsings(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(5, classTypes.Count);

            foreach (var classType in classTypes)
            {
                var importTypes = classType.Imports;
                Assert.Equal(6, importTypes.Count);

                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System"));
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System.Linq"));
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System.Text"));
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis"));
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis.CSharp"));
            }
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/CSharpImports/CompilationUnitWithClassRecordStructInterfaceAndDelegate_SharedUsings.txt")]
        public void Extract_ShouldHaveUsings_WhenGivenMultipleClassesAndDelegatesInMultipleNamespacesWithSharedUsings(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(5, classTypes.Count);

            foreach (var classType in classTypes)
            {
                var importTypes = classType.Imports;
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System"));
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
            }

            var class1ImportTypes = classTypes[0].Imports;
            Assert.Equal(4, class1ImportTypes.Count);
            Assert.Contains(class1ImportTypes, model => model.Name == "System.Linq");
            Assert.Contains(class1ImportTypes, model => model.Name == "System.Text");

            var recordImportTypes = classTypes[1].Imports;
            Assert.Equal(4, recordImportTypes.Count);
            Assert.Contains(recordImportTypes, model => model.Name == "System.Linq");
            Assert.Contains(recordImportTypes, model => model.Name == "System.Text");

            var structImportTypes = classTypes[2].Imports;
            Assert.Equal(4, structImportTypes.Count);
            Assert.Contains(structImportTypes, model => model.Name == "System.Linq");
            Assert.Contains(structImportTypes, model => model.Name == "System.Text");

            var interfaceImportTypes = classTypes[3].Imports;
            Assert.Equal(5, interfaceImportTypes.Count);
            Assert.Contains(interfaceImportTypes, model => model.Name == "System.Linq");
            Assert.Contains(interfaceImportTypes, model => model.Name == "Microsoft.CodeAnalysis");
            Assert.Contains(interfaceImportTypes, model => model.Name == "Microsoft.CodeAnalysis.CSharp");

            var delegateImportTypes = classTypes[4].Imports;
            Assert.Equal(5, delegateImportTypes.Count);
            Assert.Contains(delegateImportTypes, model => model.Name == "System.Linq");
            Assert.Contains(delegateImportTypes, model => model.Name == "Microsoft.CodeAnalysis");
            Assert.Contains(delegateImportTypes, model => model.Name == "Microsoft.CodeAnalysis.CSharp");
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/CSharpImports/CompilationUnitWithRecordStructInterfaceAndDelegate_ImbricatedNamespaces.txt")]
        public void Extract_ShouldHaveUsings_WhenGivenMultipleClassesAndDelegatesInImbricatedNamespaces(
            string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(4, classTypes.Count);

            foreach (var classType in classTypes)
            {
                var importTypes = classType.Imports;
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System"));
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System.Linq"));
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System.Text"));
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis"));
            }

            var interfaceImportTypes = classTypes[0].Imports;
            Assert.Equal(5, interfaceImportTypes.Count);

            var structImportTypes = classTypes[1].Imports;
            Assert.Equal(7, structImportTypes.Count);
            Assert.Contains(structImportTypes, model => model.Name == "Microsoft.CodeAnalysis.CSharp");
            Assert.Contains(structImportTypes, model => model.Name == "MyLib");

            var recordImportTypes = classTypes[2].Imports;
            Assert.Equal(6, recordImportTypes.Count);
            Assert.Contains(recordImportTypes, model => model.Name == "MyLib.Records");

            var delegateImportTypes = classTypes[3].Imports;
            Assert.Equal(6, delegateImportTypes.Count);
            Assert.Contains(delegateImportTypes, model => model.Name == "Microsoft.CodeAnalysis.CSharp");
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/CSharpImports/CompilationUnitWithInnerClasses.txt")]
        public void Extract_ShouldHaveUsings_WhenGivenMultipleClassesWithInnerClasses(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(7, classTypes.Count);

            foreach (var classType in classTypes)
            {
                var importTypes = classType.Imports;
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System"));
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System.Linq"));
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System.Text"));
                Assert.Null(importTypes.SingleOrDefault(model => model.Name == "MyLib"));
            }

            Assert.Equal(4, classTypes[0].Imports.Count);

            var class2ImportTypes = classTypes[1].Imports;
            Assert.Equal(5, class2ImportTypes.Count);
            Assert.Contains(class2ImportTypes, model => model.Name == "Microsoft.CodeAnalysis.CSharp");

            var innerClass1ImportTypes = classTypes[2].Imports;
            Assert.Equal(5, innerClass1ImportTypes.Count);
            Assert.Contains(innerClass1ImportTypes, model => model.Name == "Microsoft.CodeAnalysis.CSharp");

            var innerClass2ImportTypes = classTypes[3].Imports;
            Assert.Equal(5, innerClass2ImportTypes.Count);
            Assert.Contains(innerClass2ImportTypes, model => model.Name == "Microsoft.CodeAnalysis.CSharp");

            var interface1ImportTypes = classTypes[4].Imports;
            Assert.Equal(5, interface1ImportTypes.Count);
            Assert.Contains(interface1ImportTypes, model => model.Name == "Microsoft.CodeAnalysis.CSharp");

            var innerInterfaceImportTypes = classTypes[5].Imports;
            Assert.Equal(5, innerInterfaceImportTypes.Count);
            Assert.Contains(innerInterfaceImportTypes, model => model.Name == "Microsoft.CodeAnalysis.CSharp");

            Assert.Equal(4, classTypes[6].Imports.Count);
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/CSharpImports/CompilationUnitWithInnerClasses_ButNoNamespace.txt")]
        public void Extract_ShouldHaveUsings_WhenGivenMultipleClassesWithInnerClasses_ButNoNamespace(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(3, classTypes.Count);

            foreach (var classType in classTypes)
            {
                var importTypes = classType.Imports;
                Assert.Equal(4, importTypes.Count);
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System"));
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System.Linq"));
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "MyLib"));
            }
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/CSharpImports/CompilationUnitWithClassRecordStructInterfaceAndDelegate_SharedUsings_MultipleLevels.txt")]
        public void Extract_ShouldHaveUsingsInClassModels_WhenGivenText(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(5, classTypes.Count);

            foreach (var classType in classTypes)
            {
                var importTypes = classType.Imports;
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System"));
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System.Collections.Generic"));
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System.Linq"));
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "System.Text"));
                Assert.NotNull(importTypes.SingleOrDefault(model => model.Name == "Microsoft.CodeAnalysis"));
            }

            Assert.Equal(5, classTypes[0].Imports.Count);

            var structImportTypes = classTypes[1].Imports;
            Assert.Equal(7, structImportTypes.Count);
            Assert.Contains(structImportTypes, model => model.Name == "Microsoft.CodeAnalysis.CSharp");
            Assert.Contains(structImportTypes, model => model.Name == "MyLib");

            var recordImportTypes = classTypes[2].Imports;
            Assert.Equal(6, recordImportTypes.Count);
            Assert.Contains(recordImportTypes, model => model.Name == "MyLib.Records");

            var classImportTypes = classTypes[3].Imports;
            Assert.Equal(6, classImportTypes.Count);
            Assert.Contains(classImportTypes, model => model.Name == "MyLib.Records");

            var delegateImportTypes = classTypes[4].Imports;
            Assert.Equal(6, delegateImportTypes.Count);
            Assert.Contains(delegateImportTypes, model => model.Name == "Microsoft.CodeAnalysis.CSharp");
        }

        [Theory]
        [FileData(
            "TestData/CSharp/Metrics/Extraction/CSharpImports/CompilationUnitWithClassRecordStructInterfaceAndDelegate_StaticUsings.txt")]
        public void Extract_ShouldHaveUsingsInClassModels_WhenGivenStaticUsings(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(5, classTypes.Count);

            foreach (var classType in classTypes)
            {
                var usingModels = classType.Imports.Cast<UsingModel>().ToArray();

                Assert.NotNull(usingModels.SingleOrDefault(model => model.Name == "System" && !model.IsStatic));
                Assert.NotNull(usingModels.SingleOrDefault(model =>
                    model.Name == "System.Collections.Generic" && !model.IsStatic));
                Assert.NotNull(
                    usingModels.SingleOrDefault(model => model.Name == "System.Linq" && !model.IsStatic));
                Assert.NotNull(
                    usingModels.SingleOrDefault(model => model.Name == "System.Text" && !model.IsStatic));
                Assert.NotNull(usingModels.SingleOrDefault(model =>
                    model.Name == "Microsoft.CodeAnalysis" && !model.IsStatic));

                Assert.NotNull(usingModels.SingleOrDefault(model => model.Name == "System.Math" && model.IsStatic));
            }

            Assert.Equal(6, classTypes[0].Imports.Count);

            var structImportTypes = classTypes[1].Imports.Cast<UsingModel>().ToList();
            Assert.Equal(8, structImportTypes.Count);
            Assert.Contains(structImportTypes,
                model => model.Name == "Microsoft.CodeAnalysis.CSharp" && model.IsStatic);
            Assert.Contains(structImportTypes, model => model.Name == "MyLib" && model.IsStatic);

            var recordImportTypes = classTypes[2].Imports.Cast<UsingModel>().ToList();
            Assert.Equal(7, recordImportTypes.Count);
            Assert.Contains(recordImportTypes, model => model.Name == "MyLib.Records" && model.IsStatic);

            var classImportTypes = classTypes[3].Imports.Cast<UsingModel>().ToList();
            Assert.Equal(7, classImportTypes.Count);
            Assert.Contains(classImportTypes, model => model.Name == "MyLib.Records" && model.IsStatic);

            var delegateImportTypes = classTypes[4].Imports.Cast<UsingModel>().ToList();
            Assert.Equal(7, delegateImportTypes.Count);
            Assert.Contains(delegateImportTypes,
                model => model.Name == "Microsoft.CodeAnalysis.CSharp" && model.IsStatic);
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/CSharpImports/CompilationUnitWithAliasedNamespace.txt")]
        public void Extract_ShouldHaveUsingsInClassModels_WhenGivenAliasedNamespace(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(2, classTypes.Count);

            foreach (var classType in classTypes)
            {
                var importTypes = classType.Imports;
                Assert.NotNull(importTypes.SingleOrDefault(model =>
                    model.Name == "System" && model.AliasType == nameof(EAliasType.None) && model.Alias == ""));
                Assert.NotNull(importTypes.SingleOrDefault(model =>
                    model.Name == "PC.MyCompany.Project" && model.AliasType == nameof(EAliasType.Namespace) &&
                    model.Alias == "Project"));
            }

            Assert.Equal(2, classTypes[0].Imports.Count);

            var myClassImportTypes = classTypes[1].Imports;
            Assert.Equal(3, myClassImportTypes.Count);
            Assert.Contains(myClassImportTypes,
                model => model.Name == "MyCompany" && model.AliasType == nameof(EAliasType.Namespace) &&
                         model.Alias == "Company");
        }

        [Theory]
        [FileData("TestData/CSharp/Metrics/Extraction/CSharpImports/CompilationUnitWithAliasedClass.txt")]
        public void Extract_ShouldHaveUsingsInClassModels_WhenGivenAliasedClass(string fileContent)
        {
            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            Assert.Equal(3, classTypes.Count);

            foreach (var classType in classTypes)
            {
                var importTypes = classType.Imports;
                Assert.NotNull(importTypes.SingleOrDefault(model =>
                    model.Name == "System" && model.AliasType == nameof(EAliasType.None) && model.Alias == ""));

                Assert.NotNull(importTypes.SingleOrDefault(model =>
                    model.Name == "NameSpace1.MyClass" && model.AliasType == nameof(EAliasType.Class) &&
                    model.Alias == "AliasToMyClass"));

                Assert.NotNull(importTypes.SingleOrDefault(model =>
                    model.Name == "NameSpace2.MyClass<int>" && model.AliasType == nameof(EAliasType.Class) &&
                    model.Alias == "UsingAlias"));
            }
        }
    }
}
