# Honeydew

Honeydew is a tool that extracts facts from a C# Solution

## Load Solution

```
honeydew <path_to_solution> [output_file_path] [-r representation_type=normal] [-e export_type=json]
```

If `output_path` is provided, the resulted model will be written to a file at the specified location.

If `output_path` is not provided, the resulted model will be printed at the standard output

#### Supported Export Types

| Representation Type | JSON | CSV |
|----|----|----|
| normal | YES | NO |
| class_relations | YES | YES |

### Normal Representation

This Representation has all the information that Honeydew had extracted about the solution

```
.\Honeydew.exe <path_to_solution> [output_file_path] -r normal
```

### Class Relations Representation

This Representation indicates the dependencies between all the classes of the solution

```
.\Honeydew.exe <path_to_solution> [output_file_path] -r class_relations
```

### Examples

#### Normal Representation to a JSON File

```
.\Honeydew.exe <path_to_solution> <output_file_path>
```

#### Class Relation Representation to a CSV File

```
.\Honeydew.exe <path_to_solution> <output_file_path> -r class_relations -e csv
```

## Build Project

### Self-contained application - Single-file app

#### For Windows 64-bit

```
dotnet publish -r win-x64 --self-contained true -p:PublishSingleFile=true 
```

#### For Linux 64-bit

```
dotnet publish -r linux-x64 --self-contained true -p:PublishSingleFile=true 
```

#### For macOs 64-bit

```
dotnet publish -r osx-x64 --self-contained true -p:PublishSingleFile=true 
```

### Self-contained application

#### For Windows 64-bit

```
dotnet publish -r win-x64
```

#### For Linux 64-bit

```
dotnet publish -r linux-x64
```

#### For macOs 64-bit

```
dotnet publish -r osx-x64
```