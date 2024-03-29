﻿// Copyright 2021-2024 Ville Penttinen
// Distributed under the MIT License.
// https://github.com/vipentti/Vipentti.IO.Abstractions.FileSystemGlobbing/blob/main/LICENSE

using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing;
using Vipentti.IO.Abstractions.FileSystemGlobbing.Internal;

namespace Vipentti.IO.Abstractions.FileSystemGlobbing;

/// <summary>
/// Provides extensions for <see cref="Matcher" /> to support <see cref="IFileSystem" />
/// </summary>
public static class MatcherExtensions
{
    /// <summary>
    /// Searches the directory specified for all files matching patterns added to this instance of <see cref="Matcher" />
    /// </summary>
    /// <param name="matcher">The matcher</param>
    /// <param name="fileSystem">The filesystem</param>
    /// <param name="directoryPath">The root directory for the search</param>
    /// <returns>Always returns instance of <see cref="PatternMatchingResult" />, even if no files were matched</returns>
    public static PatternMatchingResult Execute(
        this Matcher matcher,
        IFileSystem fileSystem,
        string directoryPath
    )
    {
        ThrowHelpers.ThrowIfNull(matcher);
        ThrowHelpers.ThrowIfNull(fileSystem);

        return Execute(matcher, fileSystem, fileSystem.DirectoryInfo.New(directoryPath));
    }

    /// <inheritdoc cref="Execute(Matcher, IFileSystem, string)"/>
    public static PatternMatchingResult Execute(
        this Matcher matcher,
        IFileSystem fileSystem,
        IDirectoryInfo directoryInfo
    )
    {
        ThrowHelpers.ThrowIfNull(matcher);
        ThrowHelpers.ThrowIfNull(fileSystem);
        ThrowHelpers.ThrowIfNull(directoryInfo);

        return matcher.Execute(new DirectoryInfoGlobbingWrapper(fileSystem, directoryInfo));
    }

    /// <summary>
    /// Searches the directory specified for all files matching patterns added to this instance of <see cref="Matcher" />
    /// </summary>
    /// <param name="matcher">The matcher</param>
    /// <param name="fileSystem">The filesystem</param>
    /// <param name="directoryPath">The root directory for the search</param>
    /// <returns>Absolute file paths of all files matched. Empty enumerable if no files matched given patterns.</returns>
    public static IEnumerable<string> GetResultsInFullPath(
        this Matcher matcher,
        IFileSystem fileSystem,
        string directoryPath
    )
    {
        ThrowHelpers.ThrowIfNull(matcher);
        ThrowHelpers.ThrowIfNull(fileSystem);

        return GetResultsInFullPath(
            matcher,
            fileSystem,
            fileSystem.DirectoryInfo.New(directoryPath)
        );
    }

    /// <inheritdoc cref="GetResultsInFullPath(Matcher, IFileSystem, string)"/>
    public static IEnumerable<string> GetResultsInFullPath(
        this Matcher matcher,
        IFileSystem fileSystem,
        IDirectoryInfo directoryInfo
    )
    {
        ThrowHelpers.ThrowIfNull(matcher);
        ThrowHelpers.ThrowIfNull(fileSystem);
        ThrowHelpers.ThrowIfNull(directoryInfo);

        var matches = Execute(matcher, fileSystem, directoryInfo);

        if (!matches.HasMatches)
        {
            return EmptyStringsEnumerable;
        }

        var fsPath = fileSystem.Path;
        var directoryFullName = directoryInfo.FullName;

        return matches.Files.Select(GetFullPath);

        string GetFullPath(FilePatternMatch match) =>
            fsPath.GetFullPath(fsPath.Combine(directoryFullName, match.Path));
    }

    private static readonly IEnumerable<string> EmptyStringsEnumerable = Enumerable.Empty<string>();
}
