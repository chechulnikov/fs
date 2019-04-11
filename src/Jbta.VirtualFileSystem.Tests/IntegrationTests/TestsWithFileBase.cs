using System;
using System.Threading.Tasks;
using Xunit;

namespace Jbta.VirtualFileSystem.Tests.IntegrationTests
{
    public abstract class TestsWithFileBase : IAsyncLifetime
    {
        private const string FileName = "foobar";
        private string _volumePath;
        protected IFileSystem FileSystem;
        protected IFile File;
        
        public async Task InitializeAsync()
        {
            _volumePath = $"test-volume_{Guid.NewGuid()}.dat";
            await FileSystemManager.Init(_volumePath);
            FileSystem = await FileSystemManager.Mount(_volumePath);
            await FileSystem.CreateFile(FileName);
            File = await FileSystem.OpenFile(FileName);
        }
        
        public Task DisposeAsync()
        {
            if (System.IO.File.Exists(_volumePath))
            {
                System.IO.File.Delete(_volumePath);
            }

            return Task.CompletedTask;
        }
    }
}