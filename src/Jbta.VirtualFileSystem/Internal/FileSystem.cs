using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal.FileOperations;
using Jbta.VirtualFileSystem.Internal.Mounting;
using Nito.AsyncEx;

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
        private readonly AsyncReaderWriterLock _locker;
        
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
            _locker = new AsyncReaderWriterLock();
            IsMounted = true;
        }
        
        public string VolumePath { get; }

        public ulong VolumeSize => (ulong) _fileSystemMeta.BlockSize * (ulong) _fileSystemMeta.BlocksCount;
        
        public ulong UsedSpace => (ulong) _fileSystemMeta.BlockSize * (ulong) _fileSystemMeta.UsedBlocksCount;

        public ulong UnusedSpace => VolumeSize - UsedSpace;
        
        public bool IsMounted { get; private set; }

        public Task CreateFile(string fileName)
        {
            CheckFileSystemState();
            fileName = CheckAndPrepareFileName(fileName);
            return _fileCreator.CreateFile(fileName);
        }

        public async Task<bool> DeleteFile(string fileName)
        {
            CheckFileSystemState();
            fileName = CheckAndPrepareFileName(fileName);
            
            using (_locker.WriterLock())
            {
                if (_openedFiles.ContainsKey(fileName))
                {
                    return false;
                }
            
                await _fileRemover.Remove(fileName);
                _openedFiles.Remove(fileName);
            }

            return true;
        }

        public async Task<IFile> OpenFile(string fileName)
        {
            CheckFileSystemState();
            fileName = CheckAndPrepareFileName(fileName);

            using (await _locker.ReaderLockAsync())
            {
                if (_openedFiles.TryGetValue(fileName, out var alreadyOpenedFile))
                {
                    return alreadyOpenedFile;
                }
            }
            
            using (await _locker.WriterLockAsync())
            {
                if (_openedFiles.TryGetValue(fileName, out var alreadyOpenedFile))
                {
                    return alreadyOpenedFile;
                }
                
                var file = await _fileOpener.Open(fileName);
                _openedFiles.Add(fileName, file);
                
                return file;
            }
        }

        public bool CloseFile(IFile file)
        {
            CheckFileSystemState();
            if (file == null) throw new ArgumentNullException(nameof(file));
            
            using (_locker.ReaderLock())
            {
                if (!_openedFiles.ContainsKey(file.Name))
                {
                    return false;
                }
            }

            using (_locker.WriterLock())
            {
                if (!_openedFiles.ContainsKey(file.Name))
                {
                    return false;
                }
                
                _openedFiles.Remove(file.Name);
                file.Dispose();
                return true;
            }
        }

        private static string CheckAndPrepareFileName(string fileName)
        {
            fileName = fileName?.Trim();
            if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > GlobalConstant.MaxFileNameSize)
            {
                throw new ArgumentException(
                    $"Invalid file name \"{fileName}\". " +
                    $"File name must be greater than 0 and less than {GlobalConstant.MaxFileNameSize + 1}"
                );
            }
            return fileName;
        }

        private void CheckFileSystemState()
        {
            if (!IsMounted)
            {
                throw new FileSystemException("File system isn't mounted");
            }
        }

        public void Dispose()
        {
            Task.Run(() => _unmounter.Unmount()).Wait();
            IsMounted = false;
        }
    }
}