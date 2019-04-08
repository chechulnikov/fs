using System;

namespace Jbta.VirtualFileSystem.Tests.IntegrationTests
{
    public abstract class BaseTests : IDisposable
    {
        protected readonly string VolumePath;
        
        protected BaseTests()
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