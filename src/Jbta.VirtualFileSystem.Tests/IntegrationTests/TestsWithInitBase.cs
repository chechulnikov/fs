using System;
using System.Threading.Tasks;
using Xunit;

namespace Jbta.VirtualFileSystem.Tests.IntegrationTests
{
    public abstract class TestsWithInitBase : IAsyncLifetime
    {
        protected string VolumePath { get; private set; }

        public Task InitializeAsync()
        {
            VolumePath = $"test-volume_{Guid.NewGuid()}.dat";
            return FileSystemManager.Init(VolumePath);
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