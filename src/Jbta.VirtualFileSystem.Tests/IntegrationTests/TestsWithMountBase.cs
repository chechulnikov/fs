using System;
using System.Threading.Tasks;
using Xunit;

namespace Jbta.VirtualFileSystem.Tests.IntegrationTests
{
    public abstract class TestsWithMountBase : IAsyncLifetime
    {
        protected string VolumePath;
        protected IFileSystem FileSystem;
        
        public async Task InitializeAsync()
        {
            VolumePath = $"test-volume_{Guid.NewGuid()}.dat";
            await FileSystemManager.Init(VolumePath);
            FileSystem = await FileSystemManager.Mount(VolumePath);
        }
        
        public Task DisposeAsync()
        {
            if (System.IO.File.Exists(VolumePath))
            {
                System.IO.File.Delete(VolumePath);
            }

            return Task.CompletedTask;
        }
    }
}