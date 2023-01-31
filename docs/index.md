# Honeydew

Honeydew is a tool that extracts facts from a C# Solution or C# Project

## Commands

* `.\Honeydew extract <input_path>` - Extract facts.
* `.\Honeydew load <path_to_json_model> ` - Load Model from JSON File.

## Docker

## Docker

Honeydew can be run from a Docker container like this:

### Linux / MacOs

```shell
docker run --rm -it -v $(pwd)/results:/app/results -v $(pwd)/<input>:<input> dxworks/honeydew extract <input> -p
```

### Powershell

```powershell
docker run --rm -it -v ${PWD}/results:/app/results -v ${PWD}/<input>:<input> dxworks/honeydew extract <input> -p
```
