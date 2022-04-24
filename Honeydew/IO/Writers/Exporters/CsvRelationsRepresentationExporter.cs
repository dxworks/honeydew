using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Honeydew.IO.Writers.CSV;
using Honeydew.ModelRepresentations;

namespace Honeydew.IO.Writers.Exporters;

public class CsvRelationsRepresentationExporter
{
    public IList<Tuple<string, Func<string, string>>> ColumnFunctionForEachRow =
        new List<Tuple<string, Func<string, string>>>();

    public void Export(string filePath, RelationsRepresentation classRelationsRepresentation,
        List<string> csvHeaders = null)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        var csvBuilder = new CsvBuilder();
        var headers = new List<string>
        {
            "Source", "Target"
        };
        if (csvHeaders == null)
        {
            headers.AddRange(classRelationsRepresentation.DependenciesType);
        }
        else
        {
            headers.AddRange(csvHeaders);
        }

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

                if (csvHeaders == null)
                {
                    values.AddRange(classRelationsRepresentation.DependenciesType
                        .Select(relation => dependenciesDictionary.TryGetValue(relation, out var value)
                            ? value.ToString()
                            : "0"));
                }
                else
                {
                    values.AddRange(csvHeaders.Select(relation =>
                        dependenciesDictionary.TryGetValue(relation, out var value) ? value.ToString() : "0"));
                }

                csvBuilder.AddLine(values);
            }
        }

        foreach (var (header, func) in ColumnFunctionForEachRow)
        {
            csvBuilder.AddColumnWithFormulaForEachRow(header, func);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        using (var writer = new StreamWriter(filePath))
        {
            writer.Write(csvBuilder.CreateCsv());
        }
    }
}
