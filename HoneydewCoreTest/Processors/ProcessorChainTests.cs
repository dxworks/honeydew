using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using HoneydewCore.Processors;
using Moq;
using Xunit;

namespace HoneydewCoreTest.Processors
{
    public class ProcessorBuilderTests
    {
        [Fact]
        public void Process_ShouldAddFunc_WhenGivenOneIdentityFunction()
        {
            var mock = new Mock<Func<Processable<int>, Processable<int>>>();
            var processableMock = new Mock<Processable<int>>();

            mock.Setup(func => func(processableMock.Object)).Returns(processableMock.Object);

            var sut = new ProcessorChain(processableMock.Object);
            sut.Process(mock.Object);

            mock.Verify(func => func.Invoke(processableMock.Object), Times.Once);
        }


        [Fact]
        public void Process_ShouldAddFunc_WhenGivenMultipleIdentityFunctions()
        {
            var processableMock = new Mock<IProcessable>();
            var processableMock1 = new Mock<IProcessable>();
            var processableMock2 = new Mock<IProcessable>();
            var processableMock3 = new Mock<IProcessable>();

            var mock = new Mock<Func<IProcessable, IProcessable>>();
            var mock1 = new Mock<Func<IProcessable, IProcessable>>();
            var mock2 = new Mock<Func<IProcessable, IProcessable>>();

            mock.Setup(func => func(processableMock.Object)).Returns(processableMock1.Object);
            mock1.Setup(func => func(processableMock1.Object)).Returns(processableMock2.Object);
            mock2.Setup(func => func(processableMock2.Object)).Returns(processableMock3.Object);


            var sut = new ProcessorChain(processableMock.Object);

            sut.Process(mock.Object);
            sut.Process(mock1.Object)
                .Process(mock2.Object);
            var processable = sut.Finish();

            mock.Verify(func => func.Invoke(processableMock.Object), Times.Once);
            mock1.Verify(func => func.Invoke(processableMock1.Object), Times.Once);
            mock2.Verify(func => func.Invoke(processableMock2.Object), Times.Once);

            Assert.Equal(processableMock3.Object, processable);
        }

        [Fact]
        public void Process_ShouldGiveCorrectResult_WhenGivenProcessorFunctions()
        {
            var processableMock = new Mock<Processable<int>>();
            var processableMock1 = new Mock<Processable<int>>();
            var processableMock2 = new Mock<Processable<float>>();
            var processableMock3 = new Mock<Processable<string>>();

            var funcMock = new Mock<Func<Processable<int>, Processable<int>>>();
            var funcMock1 = new Mock<Func<Processable<int>, Processable<float>>>();
            var funcMock2 = new Mock<Func<Processable<float>, Processable<string>>>();
            
            funcMock.Setup(func => func(processableMock.Object)).Returns(processableMock1.Object);
            funcMock1.Setup(func => func(processableMock1.Object)).Returns(processableMock2.Object);
            funcMock2.Setup(func => func(processableMock2.Object)).Returns(processableMock3.Object);
            
            var mock = new Mock<IProcessorFunction<int, int>>();
            var mock1 = new Mock<IProcessorFunction<int, float>>();
            var mock2 = new Mock<IProcessorFunction<float, string>>();

            mock.Setup(function => function.GetFunction()).Returns(funcMock.Object);
            mock1.Setup(function => function.GetFunction()).Returns(funcMock1.Object);
            mock2.Setup(function => function.GetFunction()).Returns(funcMock2.Object);

            var sut = new ProcessorChain(processableMock.Object);

            sut.Process(mock.Object);
            sut.Process(mock1.Object)
                .Process(mock2.Object);
            var processable = sut.Finish();

            funcMock.Verify(func => func.Invoke(processableMock.Object), Times.Once);
            funcMock1.Verify(func => func.Invoke(processableMock1.Object), Times.Once);
            funcMock2.Verify(func => func.Invoke(processableMock2.Object), Times.Once);

            Assert.Equal(processableMock3.Object, processable);
        }
        
        [Fact]
        public void Finish_ShouldReturnInputProcessable_WhenProcessingWithoutProcessors()
        {
            Mock<IProcessable> processableMock = new();

            var sut = new ProcessorChain(processableMock.Object);
            var processable = sut.Finish();

            Assert.Equal(processableMock.Object, processable);
        }

        [Fact]
        public void Process_ShouldReturnIntProcessable_WhenGivenAFloatProcessable()
        {
            var processable =
                new ProcessorChain(new Processable<float>(2.5f))
                    .Process<float, int>(p => new Processable<int>((int) p.Value))
                    .Finish<int>();

            Assert.Equal(2, processable.Value);
        }

        [Fact]
        public void Process_ShouldGiveCorrectResult_WhenGivenMultipleProcessorFunctionsWithTypes()
        {
            var resultedProcessable =
                new ProcessorChain(IProcessable.Of(2))
                    .Process<int, float>(processable => IProcessable.Of<float>(processable.Value))
                    .Process<float, string>(processable =>
                        new Processable<string>(processable.Value.ToString(CultureInfo.InvariantCulture)))
                    .Process<string, double>(processable => new Processable<double>(double.Parse(processable.Value)))
                    .Process<double, long>(processable => IProcessable.Of((long) processable.Value))
                    .Finish<long>();

            Assert.Equal(2, resultedProcessable.Value);
        }

        [Fact]
        public void Process_ShouldThrowCastException_WhenGivenProcessorsThatDontMatch()
        {
            Assert.Throws<InvalidCastException>(() => new ProcessorChain(new Processable<float>(2.5f))
                .Process<int, float>(p => new Processable<float>(p.Value))
                .Finish());
        }

        [Fact]
        public void Finish_ShouldThrowCastException_WhenGivenProcessorsThatDontMatch()
        {
            Assert.Throws<InvalidCastException>(() => new ProcessorChain(new Processable<float>(2.5f))
                .Process<float, int>(p => new Processable<int>((int) p.Value))
                .Finish<string>());
        }

        [Theory]
        [ClassData(typeof(IdentityProcessorClassData))]
        public void Process_ShouldReturnTheSameProcessable_WhenGivenAProcessable(IProcessable input)
        {
            var processorBuilder = new ProcessorChain(input);

            var processable = processorBuilder
                .Process(p => p)
                .Finish();

            Assert.Equal(input, processable);
        }

        [Theory]
        [ClassData(typeof(IdentityProcessorClassData))]
        public void Process_ShouldReturnTheSameProcessable_WhenGivenAChainOfIdentityProcessors(IProcessable input)
        {
            var processable = new ProcessorChain(input)
                .Process(p => p)
                .Process(p => p)
                .Process(p => p)
                .Process(p => p)
                .Finish();

            Assert.Equal(input, processable);
        }

        private class IdentityProcessorClassData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {new Processable<int>()};
                yield return new object[] {new Processable<string>()};
                yield return new object[] {new Processable<char>()};
                yield return new object[] {new Processable<object>()};
                yield return new object[] {new Processable<float>()};
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}