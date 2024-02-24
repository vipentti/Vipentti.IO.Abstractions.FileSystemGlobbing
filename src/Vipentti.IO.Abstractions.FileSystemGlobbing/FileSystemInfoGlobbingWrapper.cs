// Copyright 2021-2024 Ville Penttinen
// Distributed under the MIT License.
// https://github.com/vipentti/Vipentti.IO.Abstractions.FileSystemGlobbing/blob/main/LICENSE

using System.IO.Abstractions;

namespace Vipentti.IO.Abstractions.FileSystemGlobbing;

internal class FileSystemInfoGlobbingWrapper
    : Microsoft.Extensions.FileSystemGlobbing.Abstractions.FileSystemInfoBase
{
    private readonly IFileSystem _fileSystem;
    private readonly IFileSystemInfo _fileSystemInfo;

    /// <summary>
    /// Initialize a new instance
    /// </summary>
    /// <param name="fileSystem">The filesystem</param>
    /// <param name="fileSystemInfo">The filesystem info</param>
    public FileSystemInfoGlobbingWrapper(IFileSystem fileSystem, IFileSystemInfo fileSystemInfo)
    {
        _fileSystemInfo = fileSystemInfo;
        _fileSystem = fileSystem;
    }

    /// <inheritdoc />
    public override string Name => _fileSystemInfo.Name;

    /// <inheritdoc />
    public override string FullName => _fileSystemInfo.FullName;

    /// <inheritdoc />
    public override Microsoft.Extensions.FileSystemGlobbing.Abstractions.DirectoryInfoBase ParentDirectory =>
        new DirectoryInfoGlobbingWrapper(
            _fileSystem,
            _fileSystem.DirectoryInfo.New(_fileSystemInfo.FullName)
        );
}
