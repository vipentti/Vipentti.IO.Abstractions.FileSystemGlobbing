// Copyright 2021-2024 Ville Penttinen
// Distributed under the MIT License.
// https://github.com/vipentti/Vipentti.IO.Abstractions.FileSystemGlobbing/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.FileSystemGlobbing;
using Xunit;
using static Vipentti.IO.Abstractions.FileSystemGlobbing.Tests.TestUtils;

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

    [Fact]
    public void ExecuteWithDirectoryPathReturnsMatchingResultWithNoFiles()
    {
        // Arrange
        var root = RootedTestPath();
        var fileSystem = SetupFileSystem([], root);

        // Act
        var matcher = new Matcher();
        matcher.AddInclude("**/*");

        var result = matcher.Execute(fileSystem, root);

        // Assert
        using var scope = new AssertionScope();
        result.HasMatches.Should().BeFalse();
        result.Files.Should().BeEmpty();
    }

    [Fact]
    public void GetResultsInFullPathReturnsFilesWithFullPath()
    {
        // Arrange
        var root = RootedTestPath();
        var fileSystem = SetupFileSystem(
            [
                (root.PathCombine("file1.txt"), ""),
                (root.PathCombine("nested/file2.txt"), ""),
                (root.PathCombine("nested/nested2/file3.txt"), ""),
                (root.PathCombine("nested1/nested2/nested3/file4.txt"), ""),
            ],
            root
        );

        // Act
        var matcher = new Matcher();
        matcher.AddInclude("**/*");

        var result = matcher.GetResultsInFullPath(fileSystem, root);

        // Assert
        using var scope = new AssertionScope();

        var sorted = result.OrderBy(it => it, StringComparer.Ordinal).ToArray();

        sorted
            .Should()
            .BeEquivalentTo(
                [
                    root.PathCombine("file1.txt"),
                    root.PathCombine("nested/file2.txt"),
                    root.PathCombine("nested/nested2/file3.txt"),
                    root.PathCombine("nested1/nested2/nested3/file4.txt"),
                ]
            );
    }

    [Fact]
    public void GetResultsInFullPathReturnsEmptyEnumerableWhenNoFilesMatches()
    {
        // Arrange
        var root = RootedTestPath();
        var fileSystem = SetupFileSystem(
            [
                (root.PathCombine("file1.txt"), ""),
                (root.PathCombine("nested/file2.txt"), ""),
                (root.PathCombine("nested/nested2/file3.txt"), ""),
                (root.PathCombine("nested1/nested2/nested3/file4.txt"), ""),
            ],
            root
        );

        // Act
        var matcher = new Matcher();
        matcher.AddInclude("**/will_not_match.txt");

        var result = matcher.GetResultsInFullPath(fileSystem, root);

        // Assert
        using var scope = new AssertionScope();

        result.Should().BeEmpty();
    }
}
