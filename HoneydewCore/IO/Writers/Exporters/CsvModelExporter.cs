using System.Collections.Generic;
using System.Linq;
using HoneydewCore.IO.Writers.CSV;
using HoneydewCore.Models.Representations;

namespace HoneydewCore.IO.Writers.Exporters
{
    public class CsvModelExporter : IFileRelationsRepresentationExporter
    {
        public string Export(FileRelationsRepresentation fileRelationsRepresentation)
        {
            var csvBuilder = new CsvBuilder();
            var headers = new List<string>
            {
                "Source", "Target"
            };
            headers.AddRange(fileRelationsRepresentation.Dependencies);

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

                    values.AddRange(fileRelationsRepresentation.Dependencies
                        .Select(relation => dependenciesDictionary.TryGetValue(relation, out var value)
                            ? value.ToString()
                            : ""));

                    csvBuilder.Add(values);
                }
            }

            return csvBuilder.CreateCsv();
        }
    }
}