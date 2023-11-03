using System.IO.Abstractions;

namespace Vipentti.IO.Abstractions.FileSystemGlobbing
{
    internal class FileInfoGlobbingWrapper : Microsoft.Extensions.FileSystemGlobbing.Abstractions.FileInfoBase
    {
        private readonly IFileSystem _fileSystem;
        private readonly IFileInfo _fileInfo;

        public FileInfoGlobbingWrapper(IFileSystem fileSystem, IFileInfo fileInfo)
        {
            _fileSystem = fileSystem;
            _fileInfo = fileInfo;
        }

        public override string Name => _fileInfo.Name;

        public override string FullName => _fileInfo.FullName;

        public override Microsoft.Extensions.FileSystemGlobbing.Abstractions.DirectoryInfoBase? ParentDirectory =>
            _fileInfo.Directory is null ? null : new DirectoryInfoGlobbingWrapper(_fileSystem, _fileInfo.Directory);
    }
}
