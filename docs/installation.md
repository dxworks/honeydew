# Installation

## Download

The Binaries and source code can be downloaded from [Honeydew Releases](https://github.com/dxworks/honeydew/releases) from GitHub

## Build Project Source Code

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
