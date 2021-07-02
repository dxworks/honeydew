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
honeydew <path_to_solution> [output_file_path] -r normal
```

### Class Relations Representation

This Representation indicates the dependencies between all the classes of the solution

```
honeydew <path_to_solution> [output_file_path] -r class_relations
```

### Examples

#### Normal Representation to a JSON File

```
honeydew <path_to_solution> <output_file_path>
```

#### Class Relation Representation to a CSV File

```
honeydew <path_to_solution> <output_file_path> -r class_relations -e csv
```

## Run

```
dotnet run  
```

## Build Project

```
dotnet build
```