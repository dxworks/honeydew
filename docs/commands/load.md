# Load

For loading a model from a json file

```sh
Honeydew load <path_to_json_model> [-n|--project-name <name>] [--no-progress-bars] [-p|--parallel]
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

### Example

```shell
Honeydew load "model.json"
```
