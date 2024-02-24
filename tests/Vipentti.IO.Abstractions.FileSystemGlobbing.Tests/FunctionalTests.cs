// Copyright 2021-2024 Ville Penttinen
// Distributed under the MIT License.
// https://github.com/vipentti/Vipentti.IO.Abstractions.FileSystemGlobbing/blob/main/LICENSE

using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.FileSystemGlobbing;
using Xunit;

namespace Vipentti.IO.Abstractions.FileSystemGlobbing.Tests;

public class FunctionalTests
{
    [Fact]
    public void FindsAllFiles()
    {
        MockFileSystem fileSystem;

        if (OperatingSystem.IsWindows())
        {
            fileSystem = new MockFileSystem(
                new Dictionary<string, MockFileData>
                {
                    { @"c:\file_at_root", new MockFileData("") },
                    { @"c:\nested\file", new MockFileData("") },
                    { @"c:\nested\inside\nested\file", new MockFileData("") },
                }
            );
        }
        else
        {
            fileSystem = new MockFileSystem(
                new Dictionary<string, MockFileData>()
                {
                    { "/file_at_root", new MockFileData("") },
                    { "/nested/file", new MockFileData("") },
                    { "/nested/inside/nested/file", new MockFileData("") },
                }
            );
        }

        var matcher = new Matcher();
        matcher.AddInclude("**/*");

        var root = OperatingSystem.IsWindows() ? @"c:\" : "/";

        var result = matcher.Execute(fileSystem, root);

        var paths = result.Files.Select(file => file.Path).ToArray();

        paths
            .Should()
            .BeEquivalentTo(new[] { "file_at_root", "nested/file", "nested/inside/nested/file", });
    }
}
