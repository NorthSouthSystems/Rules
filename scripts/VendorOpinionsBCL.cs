#!/usr/bin/env dotnet
#:include Helpers.cs
#:package NorthSouthSystems.Opinions.BCL

#pragma warning disable CA1031 // Catch a more specific exception or rethrow
#pragma warning disable CA1303 // Passes a literal string as parameter 'value' of a call to 'void Console.WriteLine'
try
{
    Console.WriteLine("Vendoring ...");

    string solutionDirectory = Helpers.CrawlToRepositoryRoot();
    string destinationDirectory = Path.Combine(solutionDirectory, "NorthSouthSystems.Rules");
    string destinationFilePath = Path.Combine(destinationDirectory, "NorthSouthSystems.Opinions.BCL.generated.cs");
    
    string vendored = await NorthSouthSystems.Opinions.BCL.VendorFiles.DownloadAndMergeAsync(
        "NorthSouthSystems.Rules",
        "ArgumentExceptionX.cs",
        "ObjectX.cs",
        "RandomX.cs",
        "Throw.cs",
        "TypeX.cs",
        "Buffers/Binary/BinaryRoundTrip.cs",
        "IO/PathX.cs",
        "IO/Hashing/XxHash128X.cs",
        "Linq/ReverseNoBuffer.cs",
        "Reflection/PropertyInfoX.cs");

    await File.WriteAllTextAsync(destinationFilePath, vendored);

    Console.WriteLine("Vendored!");
    return 0;
}
catch (Exception exception)
{
    await Console.Error.WriteLineAsync(exception.ToString());
    return 1;
}
#pragma warning restore
