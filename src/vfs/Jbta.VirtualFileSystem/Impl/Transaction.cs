using System;
using System.Collections.Generic;

namespace Vfs
{
    internal class Transaction : ITransaction
    {
        public Transaction()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
        
        public bool Commited { get; }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (!Commited) Commit();
        }
    }
}