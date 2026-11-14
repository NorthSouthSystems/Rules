set allow-duplicate-recipes

import 'dotnet.justfile'
mod dotnet 'dotnet.justfile'

# This is intended to be "explicitly" invoked; we are not calling it from other recipes.
update-package-versions:
    @just dotnet::update-package-versions
    dotnet run --file scripts/VendorOpinionsBCL.cs

format: (format-solution "NorthSouthSystems.Rules.slnx")

integrate: (coverage-summary "Release")

deploy: (publish "https://api.nuget.org/v3/index.json")
