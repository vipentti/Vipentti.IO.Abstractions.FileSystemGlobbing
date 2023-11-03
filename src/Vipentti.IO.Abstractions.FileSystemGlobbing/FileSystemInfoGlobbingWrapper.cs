using System.IO.Abstractions;

namespace Vipentti.IO.Abstractions.FileSystemGlobbing
{
    internal class FileSystemInfoGlobbingWrapper : Microsoft.Extensions.FileSystemGlobbing.Abstractions.FileSystemInfoBase
    {
        private readonly IFileSystem _fileSystem;
        private readonly IFileSystemInfo _fileSystemInfo;

        public FileSystemInfoGlobbingWrapper(IFileSystem fileSystem, IFileSystemInfo fileSystemInfo)
        {
            _fileSystemInfo = fileSystemInfo;
            _fileSystem = fileSystem;
        }

        public override string Name => _fileSystemInfo.Name;

        public override string FullName => _fileSystemInfo.FullName;

        public override Microsoft.Extensions.FileSystemGlobbing.Abstractions.DirectoryInfoBase ParentDirectory =>
            new DirectoryInfoGlobbingWrapper(_fileSystem, _fileSystem.DirectoryInfo.New(_fileSystemInfo.FullName));
    }
}
