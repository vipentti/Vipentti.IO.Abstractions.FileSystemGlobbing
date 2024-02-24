// Copyright 2021-2024 Ville Penttinen
// Distributed under the MIT License.
// https://github.com/vipentti/Vipentti.IO.Abstractions.FileSystemGlobbing/blob/main/LICENSE

using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;

namespace Vipentti.IO.Abstractions.FileSystemGlobbing.Tests;

internal static class TestUtils
{
    public static string TestPathRoot() => OperatingSystem.IsWindows() ? @"C:\" : "/";

    public static string RootedTestPath(string? relativePath = default) =>
        Path.GetFullPath(PathCombine(TestPathRoot(), relativePath ?? ""));

    public static MockFileSystem SetupFileSystem(
        IEnumerable<(string Path, string Content)> files,
        string currentDirectory = ""
    )
    {
        var dict = files.ToDictionary(it => it.Path, it => new MockFileData(it.Content));

        return new(dict, currentDirectory);
    }

    /// <summary>
    /// Combine paths ensuring the additional paths contain only the correct separators
    /// </summary>
    public static string PathCombine(this string basePath, params string[] additional)
    {
        var splits = additional.Select(s => s.Split(pathSplitCharacters)).ToArray();
        var totalLength = splits.Sum(arr => arr.Length);
        var segments = new string[totalLength + 1];
        segments[0] = basePath;
        var i = 0;
        foreach (var split in splits)
        {
            foreach (var value in split)
            {
                i++;
                segments[i] = value;
            }
        }
        return Path.Combine(segments);
    }

    private static readonly char[] pathSplitCharacters = new char[] { '/', '\\' };
}
