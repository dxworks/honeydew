using System.Text;

namespace Honeydew.IO.Writers.CSV;

public class CsvBuilder
{
    private readonly char _separator;

    private readonly IList<string> _lines = new List<string>();
    private int _headerSize;

    public CsvBuilder(char separator = ',')
    {
        _separator = separator;
    }

    public CsvBuilder(IList<string> header, char separator = ',')
    {
        _separator = separator;
        AddHeader(header);
    }

    public void AddHeader(IList<string> header)
    {
        _lines.Add(CreateCsvLine(header));
        _headerSize = header.Count;
    }

    public void AddLine(IList<string> values)
    {
        if (_headerSize != values.Count)
        {
            throw new InvalidCsvLineLengthException();
        }

        _lines.Add(CreateCsvLine(values));
    }

    public void AddColumn(IList<string> values)
    {
        for (var i = 0; i < _lines.Count; i++)
        {
            if (i >= values.Count)
            {
                return;
            }

            _lines[i] += @$",""{values[i]}""";
        }
    }

    public void AddColumnWithFormulaForEachRow(Func<string, string> func)
    {
        AddColumn(_lines.Select(func.Invoke).ToList());
    }
        
    public void AddColumnWithFormulaForEachRow(string header, Func<string, string> func)
    {
        var values = new List<string>
        {
            header
        };
        values.AddRange(_lines.Skip(1).Select(func.Invoke).ToList());
        AddColumn(values);
    }

    public string CreateCsv()
    {
        var stringBuilder = new StringBuilder("");
        for (var i = 0; i < _lines.Count; i++)
        {
            stringBuilder.Append(_lines[i]);
            if (i != _lines.Count - 1)
            {
                stringBuilder.Append(Environment.NewLine);
            }
        }

        return stringBuilder.ToString();
    }

    private string CreateCsvLine(IList<string> values)
    {
        var stringBuilder = new StringBuilder("");

        for (var i = 0; i < values.Count; i++)
        {
            stringBuilder.Append('\"');
            stringBuilder.Append(values[i]);
            stringBuilder.Append('\"');

            if (i != values.Count - 1)
            {
                stringBuilder.Append(_separator);
            }
        }

        return stringBuilder.ToString();
    }
}
