using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.IO.Writers.CSV;
using HoneydewCore.Models.Representations;

namespace HoneydewCore.IO.Writers.Exporters
{
    public class CsvModelExporter : IFileRelationsRepresentationExporter
    {
        public IList<Tuple<string, Func<string, string>>> ColumnFunctionForEachRow =
            new List<Tuple<string, Func<string, string>>>();
        
        public string Export(FileRelationsRepresentation fileRelationsRepresentation)
        {
            var csvBuilder = new CsvBuilder();
            var headers = new List<string>
            {
                "Source", "Target"
            };
            headers.AddRange(fileRelationsRepresentation.GetDependenciesTypePretty());

            csvBuilder.AddHeader(headers);

            foreach (var (sourceName, targetDictionary) in fileRelationsRepresentation.FileRelations)
            {
                foreach (var (targetName, dependenciesDictionary) in targetDictionary)
                {
                    var values = new List<string>
                    {
                        sourceName,
                        targetName
                    };

                    values.AddRange(fileRelationsRepresentation.DependenciesType
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