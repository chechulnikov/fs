using System;

namespace Jbta.VirtualFileSystem.Tests.FileSystemManagerTests
{
    public abstract class BaseTestsWithFileSystemInit : IDisposable
    {
        protected readonly string VolumePath;
        
        protected BaseTestsWithFileSystemInit()
        {
            VolumePath = $"test-volume_{Guid.NewGuid()}.dat";
            FileSystemManager.Init(VolumePath).Wait();
        }
        
        public void Dispose()
        {
            if (System.IO.File.Exists(VolumePath))
            {
                System.IO.File.Delete(VolumePath);
            }
        }
    }
}