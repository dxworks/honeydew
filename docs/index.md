# Honeydew

Honeydew is a tool that extracts facts from C# or Visual Basic Solution or Project

## Commands

* `Honeydew extract <input_path>` - Extract facts. For more information, visit [Extract Command](commands/extract)
* `Honeydew load <path_to_json_model> ` - Load Model from JSON File. For more information, visit [Load Command](commands/load)

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
