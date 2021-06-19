using System;
using HoneydewCore.IO.Writers.Exporters;
using HoneydewCore.Models.Representations;
using Xunit;

namespace HoneydewCoreTest.IO.Writers.Exporters
{
    public class CsvModelExporterTests
    {
        private readonly CsvModelExporter _sut;

        public CsvModelExporterTests()
        {
            _sut = new CsvModelExporter();
        }

        [Fact]
        public void Export_ShouldReturnEmptyString_WhenGivenEmptyRelations()
        {
            Assert.Equal(@"""Source"",""Target""", _sut.Export(new FileRelationsRepresentation()));
        }

        [Fact]
        public void Export_ShouldReturnCsv_WhenGivenOneRelationRepresentation()
        {
            var fileRelationsRepresentation = new FileRelationsRepresentation();
            fileRelationsRepresentation.Add("source", "target", "dependency", 4);

            var newLine = Environment.NewLine;
            var expectedString = $@"""Source"",""Target"",""dependency""{newLine}""source"",""target"",""4""";

            Assert.Equal(expectedString, _sut.Export(fileRelationsRepresentation));
        }
        
        [Fact]
        public void Export_ShouldReturnCsv_WhenGivenOneRelationRepresentationWithMultipleDependencies()
        {
            var fileRelationsRepresentation = new FileRelationsRepresentation();
            fileRelationsRepresentation.Add("source", "target", "dependency1", 4);
            fileRelationsRepresentation.Add("source", "target", "dependency2", 6);
            fileRelationsRepresentation.Add("source", "target", "dependency3", 1);

            var newLine = Environment.NewLine;
            var expectedString = $@"""Source"",""Target"",""dependency1"",""dependency2"",""dependency3""{newLine}""source"",""target"",""4"",""6"",""1""";

            Assert.Equal(expectedString, _sut.Export(fileRelationsRepresentation));
        }
        
        [Fact]
        public void Export_ShouldReturnCsv_WhenGivenMultipleRelationRepresentationOneDependency()
        {
            var fileRelationsRepresentation = new FileRelationsRepresentation();
            fileRelationsRepresentation.Add("source1", "target1", "dependency", 4);
            fileRelationsRepresentation.Add("source2", "target2", "dependency", 5);
            fileRelationsRepresentation.Add("source3", "target3", "dependency", 1);
            
            var newLine = Environment.NewLine;
            var expectedString = $@"""Source"",""Target"",""dependency""{newLine}""source1"",""target1"",""4""{newLine}""source2"",""target2"",""5""{newLine}""source3"",""target3"",""1""";

            Assert.Equal(expectedString, _sut.Export(fileRelationsRepresentation));
        }
        
        [Fact]
        public void Export_ShouldReturnCsv_WhenGivenMultipleRelationRepresentationMultipleDependencies()
        {
            var fileRelationsRepresentation = new FileRelationsRepresentation();
            fileRelationsRepresentation.Add("source1", "target1", "dependency1", 4);
            fileRelationsRepresentation.Add("source1", "target2", "dependency2", 6);
            fileRelationsRepresentation.Add("source1", "target2", "dependency1", 2);
            fileRelationsRepresentation.Add("source1", "target3", "dependency3", 8);
            
            fileRelationsRepresentation.Add("source2", "target3", "dependency2", 4);
            fileRelationsRepresentation.Add("source2", "target4", "dependency1", 8);
            fileRelationsRepresentation.Add("source2", "target5", "dependency1", 1);
            
            fileRelationsRepresentation.Add("source3", "target1", "dependency1", 9);
            fileRelationsRepresentation.Add("source3", "target1", "dependency2", 12);
            fileRelationsRepresentation.Add("source3", "target1", "dependency3", 31);

            var newLine = Environment.NewLine;
            var expectedString = $@"""Source"",""Target"",""dependency1"",""dependency2"",""dependency3""{newLine}" +
                                 $@"""source1"",""target1"",""4"","""",""""{newLine}" +
                                 $@"""source1"",""target2"",""2"",""6"",""""{newLine}" +
                                 $@"""source1"",""target3"","""","""",""8""{newLine}" +

                                 $@"""source2"",""target3"","""",""4"",""""{newLine}" +
                                 $@"""source2"",""target4"",""8"","""",""""{newLine}" +
                                 $@"""source2"",""target5"",""1"","""",""""{newLine}" +

                                 @"""source3"",""target1"",""9"",""12"",""31""";
                

            Assert.Equal(expectedString, _sut.Export(fileRelationsRepresentation));
        }
    }
}