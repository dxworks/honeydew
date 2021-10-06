using System.Collections.Generic;

namespace Honeydew.Scripts
{
    internal record ScriptRuntime(Script Script, Dictionary<string, object> Arguments = null);
}
