# Vipentti.IO.Abstractions.FileSystemGlobbing

A small utility library which enables using [System.IO.Abstractions](https://www.nuget.org/packages/System.IO.Abstractions) with
[Microsoft.Extensions.FileSystemGlobbing](https://www.nuget.org/packages/Microsoft.Extensions.FileSystemGlobbing/)

## Installation

```
dotnet add package Vipentti.IO.Abstractions.FileSystemGlobbing
```

## Example

```csharp
using System;
using System.IO.Abstractions;
using Microsoft.Extensions.FileSystemGlobbing;
using Vipentti.IO.Abstractions.FileSystemGlobbing;

namespace Example
{
    class Program
    {
        static void Main()
        {
            var fileSystem = new FileSystem();
            var matcher = new Matcher();
            // Find all text files in any directory under the current directory
            matcher.AddInclude("**/*.txt");

            var result = matcher.Execute(
                fileSystem,
                fileSystem.Directory.GetCurrentDirectory());

            foreach (var file in result.Files)
            {
                Console.WriteLine($"Found {file.Path}");
            }
        }
    }
}
```

## License

MIT. See [LICENSE](https://github.com/vipentti/Vipentti.IO.Abstractions.FileSystemGlobbing/blob/main/LICENSE).
