# ScriptBee

## Description

[ScriptBee](https://github.com/dxworks/scriptbee) is a scripting module designed as the go-to service when analysis system in the DxWorks ecosystem. 
It offers loaders and linkers via a plug-in architecture to manipulate model files, usually written in the JSON format.
Using its minimalist, yet intuitive frontend, scripts of all sorts that run over the loaded models can easily be written and executed.
ScriptBee offers a variety of programming languages to create the scripts in, as well highly performance due to its in-memory approach of storing the models.

## Loader Plugin

Honeydew can integrate with ScriptBee via its plug-in architecture as a model loader. 
The plugin resides in ”Honeydew.ScriptBeePlugin” project that has a dependency to ”DxWorks.ScriptBee.Plugin.Api”, the nuget package used for plugins.
Being a loader means parsing a json file and converting it to a dictionary of ”ScriptBeeModel” types.

More information about the model is described in [Honeydew's ScriptBee Model](/scriptbee/model)
