# Honeydew

Honeydew is a tool that extracts facts from a C# Solution or C# Project

## Extraction

For extracting information about a solution or project use the following command:

```
.\Honeydew extract <input_path> [--disable-progress-bars]
```

If `input_path` is a path to a solution file (.sln), Honeydew will extract facts from that solution file

If `input_path` is a path to a C# project file (.csproj), Honeydew will extract facts from that project

If `input_path` is a path to a folder, Honeydew will find all the solution files and C# project files and extract facts
from those

The output files will be placed in a folder named `results`

### Options

- `--disable-progress-bars`

  If present, then all the messages will be printed in the console. Otherwise, output will contain progress bars for a
  better visualisation of the progress

## Load Model from file

For loading a model from a json file

```
.\Honeydew load <path_to_json_model> [--disable-progress-bars]
```

### Options

- `--disable-progress-bars`

  If present, then all the messages will be printed in the console. Otherwise, output will contain progress bars for a
  better visualisation of the progress

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
