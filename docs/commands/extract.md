# Extract

For extracting information about a solution or project use the following command:

```sh
Honeydew extract <input_path> [-n|--project-name <name>] [--no-progress-bars] [--no-trim-paths] [-p|--parallel]
```

If `input_path` is a path to a solution file (.sln), Honeydew will extract facts from that solution file

If `input_path` is a path to a project file (.csproj or .vbproj), Honeydew will extract facts from that project

If `input_path` is a path to a folder, Honeydew will find all the solution files and project files and extract facts
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

### Example

#### Sequential Extraction

```shell
Honeydew extract "input_folder"
```

#### Parallel Extraction

```shell
Honeydew extract "input_folder" -p
```
