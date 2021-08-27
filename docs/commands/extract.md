# Extract

For extracting information about a solution or project use the following command:

```
.\Honeydew extract <input_path>
```

If `input_path` is a path to a solution file (.sln), Honeydew will extract facts from that solution file

If `input_path` is a path to a C# project file (.csproj), Honeydew will extract facts from that project

If `input_path` is a path to a folder, Honeydew will find all the solution files and C# project files and extract facts
from those

The output files will be placed in a folder named `results`
