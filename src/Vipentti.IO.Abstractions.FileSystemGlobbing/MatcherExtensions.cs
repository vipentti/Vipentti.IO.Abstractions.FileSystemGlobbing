using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.IO.Abstractions;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Vipentti.IO.Abstractions.FileSystemGlobbing
{
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
        /// <returns>Always returns instance of <see cref="PatternMatchingResult" />, even if not files were matched</returns>
        public static PatternMatchingResult Execute(this Matcher matcher, IFileSystem fileSystem, string directoryPath) =>
            Execute(matcher, fileSystem, fileSystem.DirectoryInfo.New(directoryPath));

        /// <inheritdoc cref="Execute(Matcher, IFileSystem, string)"/>
        public static PatternMatchingResult Execute(this Matcher matcher, IFileSystem fileSystem, IDirectoryInfo directoryInfo) =>
            matcher.Execute(new DirectoryInfoGlobbingWrapper(fileSystem, directoryInfo));

        /// <inheritdoc cref="GetResultsInFullPath(Matcher, IFileSystem, string)"/>
        public static IEnumerable<string> GetResultsInFullPath(this Matcher matcher, IFileSystem fileSystem, IDirectoryInfo directoryInfo)
        {
            var matches = Execute(matcher, fileSystem, directoryInfo).Files;
            return matches
                .Select(match => Path.GetFullPath(Path.Combine(directoryInfo.FullName, match.Path)))
                .ToArray();
        }

        /// <summary>
        /// Searches the directory specified for all files matching patterns added to this instance of <see cref="Matcher" />
        /// </summary>
        /// <param name="matcher">The matcher</param>
        /// <param name="fileSystem">The filesystem</param>
        /// <param name="directoryPath">The root directory for the search</param>
        /// <returns>Absolute file paths of all files matched. Empty enumerable if no files matched given patterns.</returns>
        public static IEnumerable<string> GetResultsInFullPath(this Matcher matcher, IFileSystem fileSystem, string directoryPath) =>
            GetResultsInFullPath(matcher, fileSystem, fileSystem.DirectoryInfo.New(directoryPath));
    }
}
