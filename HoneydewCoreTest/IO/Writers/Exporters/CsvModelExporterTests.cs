using System;
using HoneydewCore.IO.Writers.Exporters;
using HoneydewCore.Models.Representations;
using HoneydewModels.Representations;
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
            Assert.Equal(@"""Source"",""Target""", _sut.Export(new ClassRelationsRepresentation()));
        }

        [Fact]
        public void Export_ShouldReturnCsv_WhenGivenOneRelationRepresentation()
        {
            var classRelationsRepresentation = new ClassRelationsRepresentation();
            classRelationsRepresentation.Add("source", "target", "dependency", 4);

            var newLine = Environment.NewLine;
            var expectedString = $@"""Source"",""Target"",""dependency""{newLine}""source"",""target"",""4""";

            Assert.Equal(expectedString, _sut.Export(classRelationsRepresentation));
        }

        [Fact]
        public void Export_ShouldReturnCsv_WhenGivenOneRelationRepresentationWithMultipleDependencies()
        {
            var classRelationsRepresentation = new ClassRelationsRepresentation();
            classRelationsRepresentation.Add("source", "target", "dependency1", 4);
            classRelationsRepresentation.Add("source", "target", "dependency2", 6);
            classRelationsRepresentation.Add("source", "target", "dependency3", 1);

            var newLine = Environment.NewLine;
            var expectedString =
                $@"""Source"",""Target"",""dependency1"",""dependency2"",""dependency3""{newLine}""source"",""target"",""4"",""6"",""1""";

            Assert.Equal(expectedString, _sut.Export(classRelationsRepresentation));
        }

        [Fact]
        public void Export_ShouldReturnCsv_WhenGivenMultipleRelationRepresentationOneDependency()
        {
            var classRelationsRepresentation = new ClassRelationsRepresentation();
            classRelationsRepresentation.Add("source1", "target1", "dependency", 4);
            classRelationsRepresentation.Add("source2", "target2", "dependency", 5);
            classRelationsRepresentation.Add("source3", "target3", "dependency", 1);

            var newLine = Environment.NewLine;
            var expectedString =
                $@"""Source"",""Target"",""dependency""{newLine}""source1"",""target1"",""4""{newLine}""source2"",""target2"",""5""{newLine}""source3"",""target3"",""1""";

            Assert.Equal(expectedString, _sut.Export(classRelationsRepresentation));
        }

        [Fact]
        public void Export_ShouldReturnCsv_WhenGivenMultipleRelationRepresentationMultipleDependencies()
        {
            var classRelationsRepresentation = new ClassRelationsRepresentation();
            classRelationsRepresentation.Add("source1", "target1", "dependency1", 4);
            classRelationsRepresentation.Add("source1", "target2", "dependency2", 6);
            classRelationsRepresentation.Add("source1", "target2", "dependency1", 2);
            classRelationsRepresentation.Add("source1", "target3", "dependency3", 8);

            classRelationsRepresentation.Add("source2", "target3", "dependency2", 4);
            classRelationsRepresentation.Add("source2", "target4", "dependency1", 8);
            classRelationsRepresentation.Add("source2", "target5", "dependency1", 1);

            classRelationsRepresentation.Add("source3", "target1", "dependency1", 9);
            classRelationsRepresentation.Add("source3", "target1", "dependency2", 12);
            classRelationsRepresentation.Add("source3", "target1", "dependency3", 31);

            var newLine = Environment.NewLine;
            var expectedString = $@"""Source"",""Target"",""dependency1"",""dependency2"",""dependency3""{newLine}" +
                                 $@"""source1"",""target1"",""4"","""",""""{newLine}" +
                                 $@"""source1"",""target2"",""2"",""6"",""""{newLine}" +
                                 $@"""source1"",""target3"","""","""",""8""{newLine}" +
                                 $@"""source2"",""target3"","""",""4"",""""{newLine}" +
                                 $@"""source2"",""target4"",""8"","""",""""{newLine}" +
                                 $@"""source2"",""target5"",""1"","""",""""{newLine}" +
                                 @"""source3"",""target1"",""9"",""12"",""31""";


            Assert.Equal(expectedString, _sut.Export(classRelationsRepresentation));
        }

        [Fact]
        public void
            Export_ShouldReturnCsv_WhenGivenMultipleRelationRepresentationMultipleDependenciesWithColumnFunctions()
        {
            var classRelationsRepresentation = new ClassRelationsRepresentation();
            classRelationsRepresentation.Add("source1", "target1", "dependency1", 4);
            classRelationsRepresentation.Add("source1", "target2", "dependency2", 6);
            classRelationsRepresentation.Add("source1", "target2", "dependency1", 2);
            classRelationsRepresentation.Add("source1", "target3", "dependency3", 8);

            classRelationsRepresentation.Add("source2", "target3", "dependency2", 4);
            classRelationsRepresentation.Add("source2", "target4", "dependency1", 8);
            classRelationsRepresentation.Add("source2", "target5", "dependency1", 1);

            classRelationsRepresentation.Add("source3", "target1", "dependency1", 9);
            classRelationsRepresentation.Add("source3", "target1", "dependency2", 12);
            classRelationsRepresentation.Add("source3", "target1", "dependency3", 31);

            var newLine = Environment.NewLine;
            var expectedString = $@"""Source"",""Target"",""dependency1"",""dependency2"",""dependency3"",""Length"",""Count""{newLine}" +
                                 $@"""source1"",""target1"",""4"","""","""",""29"",""33""{newLine}" +
                                 $@"""source1"",""target2"",""2"",""6"","""",""30"",""38""{newLine}" +
                                 $@"""source1"",""target3"","""","""",""8"",""29"",""37""{newLine}" +
                                 $@"""source2"",""target3"","""",""4"","""",""29"",""33""{newLine}" +
                                 $@"""source2"",""target4"",""8"","""","""",""29"",""37""{newLine}" +
                                 $@"""source2"",""target5"",""1"","""","""",""29"",""30""{newLine}" +
                                 @"""source3"",""target1"",""9"",""12"",""31"",""33"",""85""";


            _sut.ColumnFunctionForEachRow.Add(
                new Tuple<string, Func<string, string>>("Length", line => line.Length.ToString()));
            _sut.ColumnFunctionForEachRow.Add(
                new Tuple<string, Func<string, string>>("Count", line =>
                {
                    var sum = 0;
                    foreach (var s in line.Split(","))
                    {
                        var trim = s.Trim('\"');
                        if (int.TryParse(trim, out var v))
                        {
                            sum += v;
                        }
                    }

                    return sum.ToString();
                }));

            Assert.Equal(expectedString, _sut.Export(classRelationsRepresentation));
        }
    }
}