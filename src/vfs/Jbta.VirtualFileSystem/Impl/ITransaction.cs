using System;

namespace Jbta.VirtualFileSystem.Impl
{
    internal interface ITransaction : IDisposable
    {
        Guid Id { get; }
        
        bool Commited { get; }
        
        void Commit();
        
        void Rollback();
    }
}