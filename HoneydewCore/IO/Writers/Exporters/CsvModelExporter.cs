using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.IO.Writers.CSV;
using HoneydewCore.Models.Representations;

namespace HoneydewCore.IO.Writers.Exporters
{
    public class CsvModelExporter : IClassRelationsRepresentationExporter
    {
        public IList<Tuple<string, Func<string, string>>> ColumnFunctionForEachRow =
            new List<Tuple<string, Func<string, string>>>();
        
        public string Export(ClassRelationsRepresentation classRelationsRepresentation)
        {
            var csvBuilder = new CsvBuilder();
            var headers = new List<string>
            {
                "Source", "Target"
            };
            headers.AddRange(classRelationsRepresentation.GetDependenciesTypePretty());

            csvBuilder.AddHeader(headers);

            foreach (var (sourceName, targetDictionary) in classRelationsRepresentation.ClassRelations)
            {
                foreach (var (targetName, dependenciesDictionary) in targetDictionary)
                {
                    var values = new List<string>
                    {
                        sourceName,
                        targetName
                    };

                    values.AddRange(classRelationsRepresentation.DependenciesType
                        .Select(relation => dependenciesDictionary.TryGetValue(relation, out var value)
                            ? value.ToString()
                            : ""));

                    csvBuilder.AddLine(values);
                }
            }

            foreach (var (header, func) in ColumnFunctionForEachRow)
            {
                csvBuilder.AddColumnWithFormulaForEachRow(header, func);
            }

            return csvBuilder.CreateCsv();
        }
    }
}