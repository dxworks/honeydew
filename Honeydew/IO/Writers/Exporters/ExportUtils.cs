using System.Globalization;

namespace Honeydew.IO.Writers.Exporters;

public static class ExportUtils
{
    public static Func<string, string> CsvSumPerLine =>
        line =>
        {
            double sum = 0;
            foreach (var s in line.Split(","))
            {
                var trim = s.Trim('\"');
                if (double.TryParse(trim, out var result))
                {
                    sum += result;
                }
            }

            return sum.ToString(CultureInfo.InvariantCulture);
        };
}
