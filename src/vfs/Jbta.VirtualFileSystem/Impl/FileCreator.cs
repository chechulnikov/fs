using System;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Exceptions;

namespace Jbta.VirtualFileSystem.Impl
{
    internal class FileCreator
    {
        private readonly FileFactory _fileFactory;
        private readonly Allocator _allocator;
        private readonly Volume _volume;

        public FileCreator(
            FileFactory fileFactory,
            Allocator allocator,
            Volume volume)
        {
            _fileFactory = fileFactory;
            _allocator = allocator;
            _volume = volume;
        }

        public async Task<IFile> CreateFile(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            if (name.Length > GlobalConstant.MaxFileNameSizeInBytes)
            {
                throw new FileSystemException($"File name cannot be greater then {GlobalConstant.MaxFileNameSizeInBytes} symbols");
            }
            
            var fileMetaBlock = new FileMetaBlock();
            var allocationResult = _allocator.AllocateBlocks(1);
            await _volume.WriteBlocks(fileMetaBlock.Serialize(), allocationResult.ReservedBlocks);
            return _fileFactory.New(fileMetaBlock, name, allocationResult.BytesAllocated);
        } 
    }
}