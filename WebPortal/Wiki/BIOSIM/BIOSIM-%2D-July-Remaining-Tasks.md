# BIOSIM - Data Source required updates

- Confirm if the source data is compressed. We don't see the compressed data in the source server
- Tool to convert CSV -> Compressed binary. Any more changes?
- Move compressed all compressed data to Azure
- File azure_weather.dll needs to be in "External" folder otherwise the DLL throws an exception. Can we move this file in the same directory as the main DLL?
- Need a copy of Canada-USA 1981-2010-Canada-USA 2020-2021.DailyDB.bin.gz

# BIOSIM - Request Pipeline

- **Model management:** We need to store and pass model executions between requests. E.g. SimpleModelRequest and WeatherGeneratorRequest. Compare to WeatherGeneratorEphemeralRequest: init the model execution, do weather generation, then execute the model immediately - no need to keep or pass anything outside the one invocation. However, in other request types, each of those steps would be its own request, so we need to keep the results of each step and be able to retrieve them for the next step.

- **TeleIO passing and merging:** In the Python version, bsutility.py has TeleIODict and TeleIODictList classes, allowing serialization and concatenation of multiple TeleIO instances (produced using multiple Contexts). This is likely a prerequisite for the model management.

- **WeatherGeneratorEphemeralRequest** may need TeleIODict and TeleIODictList (or .net equivalents) for full functionality; SimpleModelRequest, ModelRequest and WeatherGeneratorRequest depend on model management.

- **Controllers (HTTP request handling)**
Should be simple to set up after the above items are complete.

- Multiple concurrent processes using Azure features
Unknown at this time, but will depend on the previous items.

## Datahub Tasks

- **Azure app insights logging**
Should be simple to set up, and once done should seamlessly integrate with the logging that's already present in some classes (Microsoft's generic ILogger interface)
