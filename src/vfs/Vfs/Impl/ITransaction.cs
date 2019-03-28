using System;

namespace Vfs
{
    internal interface ITransaction : IDisposable
    {
        Guid Id { get; }
        bool Commited { get; }
        void Commit();
        void Rollback();
    }
}