import 'dotnet.justfile'

format: (format-solution "NorthSouthSystems.Rules.slnx")

deploy: tools (publish "https://api.nuget.org/v3/index.json")
