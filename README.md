﻿# Honeydew

Honeydew is a tool that extracts facts from a C# Solution or C# Project

## Extraction

For extracting information about a solution or project use the following command:

```
.\Honeydew extract <input_path> [-n|--project-name <name>] [--no-progress-bars] [--no-trim-paths] [-p|--parallel] [--voyager]
```

If `input_path` is a path to a solution file (.sln), Honeydew will extract facts from that solution file

If `input_path` is a path to a C# project file (.csproj), Honeydew will extract facts from that project

If `input_path` is a path to a folder, Honeydew will find all the solution files and C# project files and extract facts
from those

The output files will be placed in a folder named `results`

### Options

- `-n` or `--project-name`

  The flag must be followed by a string. This flag is used to set the project name. If not present, the project name
  will be deduced from the `<input_path>`. The project name is used to name the output files


- `--no-progress-bars`

  If present, then all the messages will be printed in the console. Otherwise, output will contain progress bars for a
  better visualisation of the progress


- `--no-trim-paths`

  If present, Honeydew will not trim the File Paths present in the created model


- `-p` or `--parallel`

  If present, Honeydew will make the extraction in parallel where possible

- `--voyager`
  *Obsolete*  
  If present, Honeydew will extract the model and export it raw in a json file. The post extraction metrics will not be
  used. The extraction will run as if the `--no-progress-bars` flag was set.


## Load Model from file

For loading a model from a json file

```
.\Honeydew load <path_to_json_model> [-n|--project-name <name>] [--no-progress-bars] [-p|--parallel]
```

### Options

- `-n` or `--project-name`

  The flag must be followed by a string. This flag is used to set the project name. If not present, the project name
  will be deduced from the `<path_to_json_model>`. The project name is used to name the output files


- `--no-progress-bars`

  If present, then all the messages will be printed in the console. Otherwise, output will contain progress bars for a
  better visualisation of the progress


- `-p` or `--parallel`

  If present, Honeydew will run scripts in parallel where possible


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

#### For macOS 64-bit

```
dotnet publish -r osx-x64
```

## End to End Test

This test consists of running Honeydew on a test project located at https://github.com/dxworks/HoneydewTestProject

To pass the test, the resulted files after the extraction must be identical to the files located in the 'e2e_results'

The test files are generated by running Honeydew locally and updating the 'e2e_results' folder content

### Update 'e2e_results' Folder

There are several helper scripts located in 'e2e_scripts' folder to copy the files generated by the local extraction of
the test project, to the 'e2e_results' folder

#### For Windows

```
.\prepare_e2e.bat <source_results_folder> [destination_folder]
```

if `destination_folder` is not provided, the destination folder will be `..\e2e_results`

#### For Linux and macOS

```
./prepare_e2e.sh <source_results_folder> [destination_folder]
```

if `destination_folder` is not provided, the destination folder will be `../e2e_results`
