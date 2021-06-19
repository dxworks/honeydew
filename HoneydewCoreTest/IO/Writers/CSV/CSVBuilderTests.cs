using System;
using HoneydewCore.IO.Writers.CSV;
using Moq;
using Xunit;

namespace HoneydewCoreTest.IO.Writers.CSV
{
    public class CsvBuilderTests
    {
        private readonly CsvBuilder _sut;
        private readonly Mock<ICsvLine> _csvLineMock = new();

        public CsvBuilderTests()
        {
            _sut = new CsvBuilder();
        }

        [Fact]
        public void Add_ShouldThrowInvalidCSVLineLengthException_WhenNoHeaderIsProvided_ButValuesAreAdded()
        {
            Assert.Throws<InvalidCsvLineLengthException>(() => _sut.Add(new[] {"Value", "Value1"}));
        }

        [Fact]
        public void Add_ShouldThrowInvalidCSVLineLengthException_WhenNoHeaderIsProvided_ButCsvLineIsAdded()
        {
            _csvLineMock.Setup(line => line.GetCsvLine()).Returns(new[] {"Value", "Value1"});

            Assert.Throws<InvalidCsvLineLengthException>(() => _sut.Add(_csvLineMock.Object));
        }

        [Fact]
        public void Add_ShouldThrowInvalidCSVLineLengthException_WhenHeaderIsProvided_ButMoreValuesAreAdded()
        {
            _sut.AddHeader(new[] {"H1", "H2"});
            Assert.Throws<InvalidCsvLineLengthException>(() => _sut.Add(new[] {"Value", "Value1", "Value2"}));
        }

        [Fact]
        public void Add_ShouldThrowInvalidCSVLineLengthException_WhenHeaderIsProvided_ButCsvLineWithMoreValuesIsAdded()
        {
            _sut.AddHeader(new[] {"H1", "H2"});

            _csvLineMock.Setup(line => line.GetCsvLine()).Returns(new[] {"Value", "Value1", "Value2"});

            Assert.Throws<InvalidCsvLineLengthException>(() => _sut.Add(_csvLineMock.Object));
        }

        [Fact]
        public void Add_ShouldThrowInvalidCSVLineLengthException_WhenHeaderIsProvided_ButFewerValuesAreAdded()
        {
            _sut.AddHeader(new[] {"H1", "H2", "H3", "H4", "h4"});
            Assert.Throws<InvalidCsvLineLengthException>(() => _sut.Add(new[] {"Value", "Value1", "Value2"}));
        }

        [Fact]
        public void Add_ShouldThrowInvalidCSVLineLengthException_WhenHeaderIsProvided_ButCsvLineWithFewerValuesIsAdded()
        {
            _sut.AddHeader(new[] {"H1", "H2", "H3", "H4", "h4"});

            _csvLineMock.Setup(line => line.GetCsvLine()).Returns(new[] {"Value", "Value1", "Value2"});

            Assert.Throws<InvalidCsvLineLengthException>(() => _sut.Add(_csvLineMock.Object));
        }

        [Fact]
        public void CreateCSV_ShouldReturnEmptyString_WhenNoHeadersAndNoValuesAreProvided()
        {
            Assert.Equal("", _sut.CreateCsv());
        }

        [Fact]
        public void CreateCSV_ShouldReturnCsvStringWithHeaders_WhenHeadersAreProvided()
        {
            Assert.Equal(@"""H1"",""H2"",""H3""", new CsvBuilder(new[] {"H1", "H2", "H3"}).CreateCsv());
        }

        [Fact]
        public void CreateCSV_ShouldCsvString_WhenHeadersAndValuesAreProvided()
        {
            _sut.AddHeader(new[] {"H1","H2","H3"});
            _sut.Add(new[] {"V1", "V2", "V3"});

            _csvLineMock.Setup(line => line.GetCsvLine()).Returns(new[] {"X1", "X2", "X3"});

            _sut.Add(_csvLineMock.Object);
            
            var newLine = Environment.NewLine;

            var expectedCsv = $@"""H1"",""H2"",""H3""{newLine}""V1"",""V2"",""V3""{newLine}""X1"",""X2"",""X3""";
            Assert.Equal(expectedCsv, _sut.CreateCsv());
        }
        
        [Fact]
        public void CreateCSV_ShouldReturnCsvStringWithHeaders_WhenHeadersAreProvided_WithOtherSeparator()
        {
            var csvBuilder = new CsvBuilder(new[] {"H1", "H2", "H3"},';');
            Assert.Equal(@"""H1"";""H2"";""H3""", csvBuilder.CreateCsv());
        }

        [Fact]
        public void CreateCSV_ShouldCsvString_WhenHeadersAndValuesAreProvided_WithOtherSeparator()
        {
            var csvBuilder = new CsvBuilder(';');
            csvBuilder.AddHeader(new[] {"H1","H2","H3"});
            csvBuilder.Add(new[] {"V1", "V2", "V3"});

            _csvLineMock.Setup(line => line.GetCsvLine()).Returns(new[] {"X1", "X2", "X3"});

            csvBuilder.Add(_csvLineMock.Object);
            
            var newLine = Environment.NewLine;

            var expectedCsv = $@"""H1"";""H2"";""H3""{newLine}""V1"";""V2"";""V3""{newLine}""X1"";""X2"";""X3""";
            Assert.Equal(expectedCsv, csvBuilder.CreateCsv());
        }
    }
}