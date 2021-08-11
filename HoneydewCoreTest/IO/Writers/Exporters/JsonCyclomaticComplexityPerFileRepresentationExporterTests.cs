using HoneydewCore.IO.Writers.Exporters;
using HoneydewCore.ModelRepresentations;
using Xunit;

namespace HoneydewCoreTest.IO.Writers.Exporters
{
    public class JsonCyclomaticComplexityPerFileRepresentationExporterTests
    {
        private readonly JsonCyclomaticComplexityPerFileRepresentationExporter _sut;

        public JsonCyclomaticComplexityPerFileRepresentationExporterTests()
        {
            _sut = new JsonCyclomaticComplexityPerFileRepresentationExporter();
        }

        [Fact]
        public void Export_ShouldReturnEmptyObject_WhenProvidedWithEmptyRepresentation()
        {
            const string expectedString = @"{""file"":{""concerns"":[]}}";

            var exportString = _sut.Export(new CyclomaticComplexityPerFileRepresentation());

            Assert.Equal(expectedString, exportString);
        }

        [Fact]
        public void Export_ShouldReturnEmptyObject_WhenProvidedWithRepresentationWithOneFile()
        {
            const string expectedString =
                @"{""file"":{""concerns"":[{""entity"":""path"",""tag"":""maxCyclo"",""strength"":""15""},{""entity"":""path"",""tag"":""minCyclo"",""strength"":""2""},{""entity"":""path"",""tag"":""avgCyclo"",""strength"":""7""},{""entity"":""path"",""tag"":""sumCyclo"",""strength"":""20""}]}}";

            var representation = new CyclomaticComplexityPerFileRepresentation();
            representation.AddConcern(new Concern
            {
                Entity = "path",
                Tag = "maxCyclo",
                Strength = "15"
            });
            representation.AddConcern(new Concern
            {
                Entity = "path",
                Tag = "minCyclo",
                Strength = "2"
            });
            representation.AddConcern(new Concern
            {
                Entity = "path",
                Tag = "avgCyclo",
                Strength = "7"
            });
            representation.AddConcern(new Concern
            {
                Entity = "path",
                Tag = "sumCyclo",
                Strength = "20"
            });
            var exportString = _sut.Export(representation);

            Assert.Equal(expectedString, exportString);
        }

        [Fact]
        public void Export_ShouldReturnEmptyObject_WhenProvidedWithRepresentationWithTwoFiles()
        {
            const string expectedString =
                @"{""file"":{""concerns"":[{""entity"":""path"",""tag"":""maxCyclo"",""strength"":""15""},{""entity"":""path"",""tag"":""minCyclo"",""strength"":""2""},{""entity"":""path"",""tag"":""avgCyclo"",""strength"":""7""},{""entity"":""path"",""tag"":""sumCyclo"",""strength"":""20""},{""entity"":""path/path2"",""tag"":""maxCyclo"",""strength"":""31""},{""entity"":""path/path2"",""tag"":""minCyclo"",""strength"":""4""},{""entity"":""path/path2"",""tag"":""avgCyclo"",""strength"":""12""},{""entity"":""path/path2"",""tag"":""sumCyclo"",""strength"":""45""}]}}";

            var representation = new CyclomaticComplexityPerFileRepresentation();
            representation.AddConcern(new Concern
            {
                Entity = "path",
                Tag = "maxCyclo",
                Strength = "15"
            });
            representation.AddConcern(new Concern
            {
                Entity = "path",
                Tag = "minCyclo",
                Strength = "2"
            });
            representation.AddConcern(new Concern
            {
                Entity = "path",
                Tag = "avgCyclo",
                Strength = "7"
            });
            representation.AddConcern(new Concern
            {
                Entity = "path",
                Tag = "sumCyclo",
                Strength = "20"
            });


            representation.AddConcern(new Concern
            {
                Entity = "path/path2",
                Tag = "maxCyclo",
                Strength = "31"
            });
            representation.AddConcern(new Concern
            {
                Entity = "path/path2",
                Tag = "minCyclo",
                Strength = "4"
            });
            representation.AddConcern(new Concern
            {
                Entity = "path/path2",
                Tag = "avgCyclo",
                Strength = "12"
            });
            representation.AddConcern(new Concern
            {
                Entity = "path/path2",
                Tag = "sumCyclo",
                Strength = "45"
            });
            var exportString = _sut.Export(representation);

            Assert.Equal(expectedString, exportString);
        }
    }
}
