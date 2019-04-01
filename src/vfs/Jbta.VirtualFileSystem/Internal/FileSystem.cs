using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal.FileOperations;
using Jbta.VirtualFileSystem.Internal.Mounting;
using Jbta.VirtualFileSystem.Utils;

namespace Jbta.VirtualFileSystem.Internal
{
    internal class FileSystem : IFileSystem
    {
        private readonly FileSystemMeta _fileSystemMeta;
        private readonly FileCreator _fileCreator;
        private readonly FileOpener _fileOpener;
        private readonly FileRemover _fileRemover;
        private readonly Unmounter _unmounter;
        private readonly Dictionary<string, IFile> _openedFiles;
        private readonly ReaderWriterLockSlim _locker;
        
        public FileSystem(
            string volumePath,
            FileSystemMeta fileSystemMeta,
            FileCreator fileCreator,
            FileOpener fileOpener,
            FileRemover fileRemover,
            Unmounter unmounter)
        {
            VolumePath = volumePath;
            _fileSystemMeta = fileSystemMeta;
            _fileCreator = fileCreator;
            _fileOpener = fileOpener;
            _fileRemover = fileRemover;
            _unmounter = unmounter;
            _openedFiles = new Dictionary<string, IFile>();
            _locker = new ReaderWriterLockSlim();
            IsMounted = true;
        }

        private int BlocksCount => _fileSystemMeta.BlockSize * GlobalConstant.BitmapBlocksCount * 8;
        
        public string VolumePath { get; }

        public ulong VolumeSize => (ulong) (_fileSystemMeta.BlockSize * BlocksCount);
        
        public ulong UsedSpace => (ulong) (_fileSystemMeta.BlockSize * _fileSystemMeta.UsedBlocksCount);

        public ulong UnusedSpace => VolumeSize - UsedSpace;
        
        public bool IsMounted { get; private set; }

        public Task<IFile> CreateFile(string fileName)
        {
            fileName = CheckAndPrepareFileName(fileName);
            return _fileCreator.CreateFile(fileName);
        }

        public async Task DeleteFile(string fileName)
        {
            fileName = CheckAndPrepareFileName(fileName);
            
            await _fileRemover.Remove(fileName);
            using (_locker.WriterLock())
            {
                _openedFiles.Remove(fileName);
            }
        }

        public async Task<IFile> OpenFile(string fileName)
        {
            fileName = CheckAndPrepareFileName(fileName);
            
            using (_locker.UpgradableReaderLock())
            {
                if (_openedFiles.TryGetValue(fileName, out var file)) return file;
                
                file = await _fileOpener.Open(fileName);
                using (_locker.WriterLock())
                {
                    _openedFiles.Add(fileName, file);
                }
                
                return file;
            }
        }

        public bool TryCloseFile(IFile file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            
            using (_locker.UpgradableReaderLock())
            {
                if (!_openedFiles.ContainsKey(file.Name)) return false;

                using (_locker.WriterLock())
                {
                    _openedFiles.Remove(file.Name);
                    file.Dispose();
                    return true;
                }
            }
        }

        private static string CheckAndPrepareFileName(string fileName)
        {
            fileName = fileName?.Trim();
            
            if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > GlobalConstant.MaxFileNameSize)
                throw new ArgumentException("Invalid file name");

            return fileName;
        }

        public void Dispose()
        {
            _unmounter.Unmount().Wait();
            IsMounted = false;
        }
    }
}