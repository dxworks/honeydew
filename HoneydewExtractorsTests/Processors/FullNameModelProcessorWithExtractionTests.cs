using HoneydewCore.Logging;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.Processors;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewExtractorsTests.Processors
{
    

    public class FullNameModelProcessorWithExtractionTests
    {
        private readonly FullNameModelProcessor _sut;
        private readonly Mock<IProgressLogger> _progressLoggerMock = new();
        private readonly CSharpFactExtractor _extractor;

        public FullNameModelProcessorWithExtractionTests()
        {
            _sut = new FullNameModelProcessor(_progressLoggerMock.Object);
            _extractor = new CSharpFactExtractor();
        }

        [Fact]
        public void Process_ShouldReturnFullName_WhenProvidedWithUsings()
        {
            const string fileContent1 = @"
namespace Models
{
    record Model
    {
    }
}
";
            const string fileContent2 = @"
using System;
namespace Services
{
    using Models;

    class Service
    {
        Model _m {get;set;}

        Model GetModel()
        {
            return new();
        }
    }
}
";

            var classModels1 = _extractor.Extract(fileContent1);
            var classModels2 = _extractor.Extract(fileContent2);

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            foreach (var classModel in classModels1)
            {
                projectModel.Add(classModel);
            }

            foreach (var classModel in classModels2)
            {
                projectModel.Add(classModel);
            }

            solutionModel.Projects.Add(projectModel);
            repositoryModel.Solutions.Add(solutionModel);

            var actualRepositoryModel = _sut.Process(repositoryModel);

            var model = actualRepositoryModel.Solutions[0].Projects[0].Namespaces[1].ClassModels[0];
            Assert.Equal("Models.Model", model.Properties[0].Type);
            Assert.Equal("Models.Model", model.Methods[0].ReturnType);
        }

        [Fact]
        public void Process_ShouldReturnFullName_WhenProvidedWithStaticUsings()
        {
            const string fileContent1 = @"
using System;

namespace Utils
{
    using static Math;

    public class IntWrapper
    {
        public void Call()
        {
        }
    }

    public class StringWrapper
    {
        public void Call()
        {
        }
    }

    public static class Util
    {
        public static IntWrapper Int;

        public static StringWrapper Name { get; set; }

        public static double Radical(double value)
        {
            return Sqrt(value);
        }
    }
}";

            const string fileContent2 = @"
namespace MyNamespace
{
    using static Utils.Util;

    public class Client
    {
        public Client()
        {
            var v = Radical(2.0);

            Int.Call();
            Name.Call();
        }

        public void Calculate(double value)
        {
            var v = Radical(value);

            Int.Call();
            Name.Call();
        }
    }
}
";

            var classModels1 = _extractor.Extract(fileContent1);
            var classModels2 = _extractor.Extract(fileContent2);

            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();

            foreach (var classModel in classModels1)
            {
                projectModel.Add(classModel);
            }

            foreach (var classModel in classModels2)
            {
                projectModel.Add(classModel);
            }

            solutionModel.Projects.Add(projectModel);
            repositoryModel.Solutions.Add(solutionModel);

            var actualRepositoryModel = _sut.Process(repositoryModel);

            var utilClass = actualRepositoryModel.Solutions[0].Projects[0].Namespaces[0].ClassModels[2];
            Assert.Equal("Sqrt", utilClass.Methods[0].CalledMethods[0].MethodName);
            Assert.Equal("System.Math", utilClass.Methods[0].CalledMethods[0].ContainingClassName);
            Assert.Equal(1, utilClass.Methods[0].CalledMethods[0].ParameterTypes.Count);
            Assert.Equal("System.Double", utilClass.Methods[0].CalledMethods[0].ParameterTypes[0].Type);

            var clientClass = actualRepositoryModel.Solutions[0].Projects[0].Namespaces[1].ClassModels[0];

            Assert.Equal(3, clientClass.Methods[0].CalledMethods.Count);

            var methodArray = new[]
            {
                clientClass.Methods[0],
                clientClass.Constructors[0],
            };

            foreach (var methodModel in methodArray)
            {
                var radicalCall = methodModel.CalledMethods[0];
                Assert.Equal("Radical", radicalCall.MethodName);
                Assert.Equal("Utils.Util", radicalCall.ContainingClassName);
                Assert.Equal(1, radicalCall.ParameterTypes.Count);
                Assert.Equal("System.Double", radicalCall.ParameterTypes[0].Type);

                var intCall = methodModel.CalledMethods[1];
                Assert.Equal("Call", intCall.MethodName);
                Assert.Equal("Utils.IntWrapper", intCall.ContainingClassName);
                Assert.Empty(intCall.ParameterTypes);

                var stringCall = methodModel.CalledMethods[2];
                Assert.Equal("Call", stringCall.MethodName);
                Assert.Equal("Utils.StringWrapper", stringCall.ContainingClassName);
                Assert.Empty(stringCall.ParameterTypes);
            }
        }
    }
}
