# Dots
*C# Implant for the Connect Command and Control Framework* 

## Overview
Dots is an Implant designed to remotely administrate windows systems. The implant has been
designed to communicate with the Connect Command and Control Framework to obtain tasks
from operators. These tasks are to be computed similiar to JSON-RPC batch_requests and returned
as JSON-RPC batch_responses. Dots features an wide command set including, download 
and upload, execute_assebmly and command execution. This command set is extensible with 
included compiled .NET assemblies located in the assemblies solution. 

Dot is only to be used for legal applications when the explicit permission of the targeted
organizaiton has been obtained.

## Complimation
Connected is developed towards the .NET 4.8 framework. Target systems may not have all dependenices installed
such as System.Text.JSON. It is recommended to statically compile all dependices when compiling from source 
to avoid any issues. The following ILMerge command will statically compile all current depencies. Note
that this command is intended to be ran in the /bin/Release directory of the Dots solution.

```

```
