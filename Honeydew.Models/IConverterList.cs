using System.Collections.Generic;
using Newtonsoft.Json;

namespace Honeydew.Models;

public interface IConverterList
{
    IEnumerable<JsonConverter> GetConverters();
}
