using System.Collections.Generic;

namespace Jbta.VirtualFileSystem.Impl
{
    internal class TransactionManager
    {
        private readonly ISet<ITransaction> _transactions;
        
        public TransactionManager()
        {
            _transactions = new HashSet<ITransaction>();
        }

        public ITransaction StartTransaction()
        {
            var transaction = new Transaction();
            _transactions.Add(transaction);
            return transaction;
        }
    }
}